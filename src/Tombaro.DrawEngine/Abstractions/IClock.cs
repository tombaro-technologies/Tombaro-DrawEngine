namespace Tombaro.DrawEngine.Abstractions
{
    /// <summary>
    /// Represents a clock that provides the current date and time in Coordinated Universal Time (UTC).
    /// </summary>
    /// <remarks>Implementations of this interface can be used to abstract time retrieval, enabling easier
    /// testing and control over time-dependent logic.</remarks>
    public interface IClock
    {
        /// <summary>Gets the current date and time expressed as Coordinated Universal Time (UTC).</summary>
        DateTime UtcNow { get; }
    }
}