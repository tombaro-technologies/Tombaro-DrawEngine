using Tombaro.DrawEngine.Abstractions;

namespace Tombaro.DrawEngine.Tests.TestDoubles
{
    internal sealed class FakeClock : IClock
    {
        public FakeClock(DateTime utcNow)
        {
            UtcNow = utcNow;
        }

        public DateTime UtcNow { get; }
    }
}
