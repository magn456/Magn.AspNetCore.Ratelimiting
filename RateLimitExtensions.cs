using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Magn.AspNetCore.RateLimiting
{
    public static class RateLimitExtensions
    {
        /// <summary>
        /// Adds rate limiting services to the service container.
        /// </summary>
        /// <param name="services">The service container to add services to.</param>
        /// <returns>The same service collection so multiple calls can be chained.</returns>
        /// <remarks>You can implement a custom <see cref="IRateLimitResolver" /> (using <see cref="ServiceCollectionDescriptorExtensions.Replace(IServiceCollection, ServiceDescriptor)" />) to override the key.</remarks>
        /// <exception cref="InvalidOperationException">When a <see cref="IDistributedCache" /> cannot be found.</exception>
        public static IServiceCollection AddRateLimit(this IServiceCollection services)
            => services.AddRateLimit(options => { });
        
        /// <inheritdoc cref="RateLimitExtensions.AddRateLimit(IServiceCollection)"/>
        public static IServiceCollection AddRateLimit(this IServiceCollection services, Action<RateLimitOptions> configureAction)
        {
            Type cacheType = typeof(IDistributedCache);
            if (services.All(x => x.ServiceType != cacheType))
            {
                throw new InvalidOperationException($"A '{cacheType.Name}' service is required.");
            }
            
            services.Configure(configureAction);

            services
                .AddTransient<IRateLimitResolver, DefaultRateLimitResolver>()
                .AddTransient<RateLimitMiddleware>();

            return services;      
        }
        
        /// <summary>
        /// Adds the rate limiting middleware
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <returns>The same application builder to multiple calls can be chained.</returns>
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app)
            => app.UseMiddleware<RateLimitMiddleware>();
    }
}