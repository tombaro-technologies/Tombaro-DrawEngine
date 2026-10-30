using Tombaro.DrawEngine.Abstractions;

namespace Tombaro.DrawEngine.Contracts
{
    /// <summary>
    /// Represents the complete output of a deterministic draw.
    /// </summary>
    public sealed record DrawResult<TTicket>
        where TTicket : IDrawTicket
    {
        public required string AlgorithmVersion { get; init; }
        public required string DrawId { get; init; }
        public required DateTime DrawExecutedUtc { get; init; }

        /// <summary>Gets the SHA-256 hash of the canonical draw input.</summary>
        public required string BaseSeedHex { get; init; }

        /// <summary>Gets the derived seed used to order eligible tickets.</summary>
        public required string SelectionSeedHex { get; init; }

        /// <summary>Gets the selected winners in draw order.</summary>
        public required IReadOnlyList<DrawWinner<TTicket>> Winners { get; init; }

        public required DrawAuditInfo AuditInfo { get; init; }
    }
}
