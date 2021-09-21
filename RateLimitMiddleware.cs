using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Magn.AspNetCore.RateLimiting
{
    internal sealed class RateLimitMiddleware
        : IMiddleware
    {
        private readonly IDistributedCache _cache;
        private readonly IRateLimitResolver _resolver;
        private readonly RateLimitOptions _options;

        public RateLimitMiddleware(
            IOptions<RateLimitOptions> optionsAccessor,
            IDistributedCache cache,
            IRateLimitResolver resolver)
        {
            _options = optionsAccessor.Value;
            _cache = cache;
            _resolver = resolver;
        }
        
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            Endpoint? endpoint = context.GetEndpoint();
            if (endpoint == null)
            {
                await next(context);
                return;
            }

            ControllerActionDescriptor? controllerDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
            if (controllerDescriptor == null)
            {
                await next(context);
                return;
            }
            
            
            ActionContext actionContext = new ActionContext(context, context.GetRouteData(), controllerDescriptor);
            RateLimitAttribute? methodRateLimit =
                controllerDescriptor.MethodInfo.GetCustomAttribute<RateLimitAttribute>();
            if (methodRateLimit != null)
            {
                // use method rate limit
                await ExecuteAsync(methodRateLimit, context, actionContext, next);
                return;
            }
            
            RateLimitAttribute? controllerRateLimit =
                controllerDescriptor.ControllerTypeInfo.GetCustomAttribute<RateLimitAttribute>();
            if (controllerRateLimit != null)
            {
                // use controller rate limit
                await ExecuteAsync(controllerRateLimit, context, new ControllerContext(actionContext), next);
                return;
            }
            
            RateLimitAttribute? globalRateLimit = Assembly.GetEntryAssembly()!.GetCustomAttribute<RateLimitAttribute>();
            if (globalRateLimit != null)
            {
                // use global rate limit
                await ExecuteAsync(globalRateLimit, context, actionContext, next, true);
                return;
            }
            
            // if no rate limiters exists, just proceed as normal
            await next(context);
        }

        private async Task ExecuteAsync(RateLimitAttribute rateLimit, HttpContext context, ActionContext actionContext, RequestDelegate next, bool global = false)
        {
            string key = await _resolver.ResolveAsync(context);
            string counterData = await _cache.GetStringAsync(key);
            
            RateLimitCounter counter;
            if (string.IsNullOrEmpty(counterData))
            {
                counter = new RateLimitCounter()
                {
                    Remaining = rateLimit.Amount,
                    Started = DateTimeOffset.UtcNow,
                    Global = global
                };
            }
            else
            {
                counter = JsonSerializer.Deserialize<RateLimitCounter>(counterData);
            }
            
            DateTimeOffset expiresAt = counter.Started + rateLimit.Every;
            
            if (counter.Remaining == 0)
            {
                IActionResult result;
                if (_options.QuoteExceeded != null)
                {
                    result = await _options.QuoteExceeded.Invoke(rateLimit, context);
                }
                else
                {
                    result = new StatusCodeResult(_options.StatusCode);
                }
                
                context.Response.Headers[_options.RetryHeaderName] = expiresAt.ToUnixTimeSeconds().ToString();
                await result.ExecuteResultAsync(actionContext);
                return;
            }
            
            counter.Remaining--;
            counterData = JsonSerializer.Serialize(counter);
            await _cache.SetStringAsync(key, counterData, new DistributedCacheEntryOptions().SetAbsoluteExpiration(expiresAt));
            
            context.Response.Headers[_options.RemainingHeaderName] = counter.Remaining.ToString();
            context.Response.Headers[_options.ResetHeaderName] =
                expiresAt.ToUnixTimeSeconds().ToString();
            context.Response.Headers[_options.LimitHeaderName] = rateLimit.Amount.ToString();

            await next(context);
        }
    }
}