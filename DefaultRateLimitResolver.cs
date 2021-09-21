using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Magn.AspNetCore.RateLimiting
{
    internal sealed class DefaultRateLimitResolver
        : IRateLimitResolver
    {
        private readonly RateLimitOptions _options;
        
        public DefaultRateLimitResolver(IOptions<RateLimitOptions> optionsAccessor)
        {
            _options = optionsAccessor.Value;
        }
        
        public Task<string> ResolveAsync(HttpContext context)
        {
            string key = context.Connection.RemoteIpAddress?.ToString();
            if (context.Request.Headers.ContainsKey(_options.ForwardedHeaderName))
            {
                key = context.Request.Headers[_options.ForwardedHeaderName];
            }
            
            return Task.FromResult(key);
        }
    }
}