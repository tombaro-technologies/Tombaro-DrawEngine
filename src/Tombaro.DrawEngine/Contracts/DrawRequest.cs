using Tombaro.DrawEngine.Abstractions;

namespace Tombaro.DrawEngine.Contracts
{
    /// <summary>
    /// Represents the complete set of inputs required to execute a deterministic draw.
    /// </summary>
    public sealed record DrawRequest<TTicket>
        where TTicket : IDrawTicket
    {
        /// <summary>Gets the algorithm version identifier included in seed derivation.</summary>
        public required string AlgorithmVersion { get; init; }

        /// <summary>Gets the unique identifier of this draw.</summary>
        public required string DrawId { get; init; }

        /// <summary>Gets the UTC timestamp at which eligibility for the draw was closed.</summary>
        public required DateTime DrawClosedUtc { get; init; }

        /// <summary>Gets the nonce used as an explicit input to the deterministic draw.</summary>
        public required string DrawNonce { get; init; }

        /// <summary>Gets the number of unique winners to select.</summary>
        public required int WinnerCount { get; init; }

        /// <summary>Gets all valid tickets eligible to win.</summary>
        public required IReadOnlyList<TTicket> ValidTickets { get; init; }
    }
}
