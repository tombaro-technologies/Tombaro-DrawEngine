namespace Tombaro.DrawEngine.Contracts.Seeds
{
    /// <summary>
    /// Contains the deterministic seed values used by the draw algorithm.
    /// </summary>
    /// <param name="BaseSeedHex">SHA-256 hash of the canonical draw input.</param>
    /// <param name="SelectionSeedHex">Seed used to deterministically order the eligible tickets.</param>
    public sealed record SeedBundle(
        string BaseSeedHex,
        string SelectionSeedHex);
}
