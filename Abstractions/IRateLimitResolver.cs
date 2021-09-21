using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Magn.AspNetCore.RateLimiting
{
    public interface IRateLimitResolver
    {
        /// <summary>
        /// Resolves a key to use when limiting a request
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<string> ResolveAsync(HttpContext context);
    }
}