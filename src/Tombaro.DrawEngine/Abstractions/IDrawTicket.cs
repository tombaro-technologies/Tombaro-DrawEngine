namespace Tombaro.DrawEngine.Abstractions
{
    /// <summary>
    /// Represents a ticket that can participate in a draw.
    /// </summary>
    /// <remarks>
    /// The draw engine only reads <see cref="TicketNumber"/>. Implementations may carry
    /// additional application-specific data, but that data never affects winner selection.
    /// </remarks>
    public interface IDrawTicket
    {
        /// <summary>Gets the positive decimal ticket number without leading zeroes, up to 2147483647.</summary>
        string TicketNumber { get; }
    }
}
