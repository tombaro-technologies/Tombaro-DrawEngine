using Tombaro.DrawEngine.Abstractions;

namespace Tombaro.DrawEngine.Time
{
    /// <summary>
    /// Production implementation of <see cref="IClock"/> that returns the real system UTC time.
    /// </summary>
    public sealed class SystemClock : IClock
    {
        /// <summary>Gets the current date and time expressed as Coordinated Universal Time (UTC).</summary>
        public DateTime UtcNow => DateTime.UtcNow;
    }
}