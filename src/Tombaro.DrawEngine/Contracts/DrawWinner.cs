using Tombaro.DrawEngine.Abstractions;

namespace Tombaro.DrawEngine.Contracts
{
    /// <summary>
    /// Represents one selected winner and its 1-based position in the draw order.
    /// </summary>
    public sealed record DrawWinner<TTicket>
        where TTicket : IDrawTicket
    {
        /// <summary>Gets the 1-based position at which the ticket was selected.</summary>
        public required int DrawPosition { get; init; }

        /// <summary>Gets the original ticket object supplied by the calling application.</summary>
        public required TTicket Ticket { get; init; }
    }
}
