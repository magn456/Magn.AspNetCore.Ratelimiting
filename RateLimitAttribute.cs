using System;

namespace Magn.AspNetCore.RateLimiting
{
    /// <summary>
    /// Allows a method, controller or assembly to be rate limited.
    ///
    /// <remarks>When used on an assembly, it will be treated as a global rate limit for all methods and controllers.</remarks>
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class RateLimitAttribute
        : Attribute
    {
        public TimeSpan Every { get; set; }

        public uint Amount { get; set; }

        public RateLimitAttribute()
        {
            
        }
        
        public RateLimitAttribute(uint amount, string every)
        {
            Amount = amount;

            if (!TimeSpan.TryParse(every, out TimeSpan t))
            {
                throw new InvalidOperationException($"{nameof(every)} is not a valid timespan.");
            }

            Every = t;
        }
    }
}