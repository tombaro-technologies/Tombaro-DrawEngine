using Tombaro.DrawEngine.Abstractions;
using Tombaro.DrawEngine.Contracts;
using Tombaro.DrawEngine.Contracts.Seeds;
using Tombaro.DrawEngine.Serialization;

namespace Tombaro.DrawEngine.Services
{
    /// <summary>
    /// Builds deterministic cryptographic seeds from the complete canonical draw input.
    /// </summary>
    public sealed class SeedBuilder
    {
        private readonly Sha256Hasher _hasher;

        public SeedBuilder(Sha256Hasher hasher)
        {
            _hasher = hasher;
        }

        public SeedBundle Build<TTicket>(DrawRequest<TTicket> request)
            where TTicket : IDrawTicket
        {
            ArgumentNullException.ThrowIfNull(request);

            var canonicalInput = CanonicalSerializer.SerializeDrawInput(request);
            var baseSeedHex = _hasher.HashToHex(canonicalInput);
            var selectionSeedHex = _hasher.HashToHex($"{baseSeedHex}|selection");

            return new SeedBundle(baseSeedHex, selectionSeedHex);
        }
    }
}
