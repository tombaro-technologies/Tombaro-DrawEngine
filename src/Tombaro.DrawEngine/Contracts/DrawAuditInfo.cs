namespace Tombaro.DrawEngine.Contracts
{
    /// <summary>
    /// Contains information that makes a completed draw independently verifiable.
    /// </summary>
    public sealed record DrawAuditInfo
    {
        /// <summary>Gets the number of valid tickets that participated in the draw.</summary>
        public required int ValidTicketCount { get; init; }

        /// <summary>Gets the number of winners requested by the caller.</summary>
        public required int WinnerCount { get; init; }

        /// <summary>
        /// Gets the SHA-256 hash of the canonical draw input. A verifier can reconstruct the
        /// canonical input and compare this hash before reproducing the draw.
        /// </summary>
        public required string DrawInputHashHex { get; init; }
    }
}
