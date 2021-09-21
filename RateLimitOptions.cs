using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Magn.AspNetCore.RateLimiting
{
    public sealed class RateLimitOptions
    {
        public string ForwardedHeaderName { get; set; } = "X-Forwarded-For";

        public string LimitHeaderName { get; set; } = "X-Rate-Limit";

        public string RemainingHeaderName { get; set; } = "X-Rate-Limit-Remaining";

        public string ResetHeaderName { get; set; } = "X-Rate-Limit-Reset";

        public string RetryHeaderName { get; set; } = "X-Rate-Limit-Retry-After";

        public int StatusCode { get; set; } = StatusCodes.Status429TooManyRequests;

        /// <summary>
        /// Allows for custom response mapping to a action result
        /// </summary>
        public Func<RateLimitAttribute, HttpContext, Task<IActionResult>> QuoteExceeded { get; set; } = null;
    }
}