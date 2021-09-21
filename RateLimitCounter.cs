using System;

namespace Magn.AspNetCore.RateLimiting
{
    internal sealed class RateLimitCounter
    {
        private DateTimeOffset _started;
        
        public uint Remaining { get; set; }
        
        public bool Global { get; set; }
        
        public DateTimeOffset Started
        {
            get => _started;
            set
            {
                if (_started != DateTimeOffset.MinValue)
                {
                    throw new InvalidOperationException($"{nameof(Started)} cannot be set again.");
                }

                _started = value;
            }
        }
    }
}