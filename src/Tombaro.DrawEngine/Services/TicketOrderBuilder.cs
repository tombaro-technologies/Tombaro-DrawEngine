using Tombaro.DrawEngine.Abstractions;

namespace Tombaro.DrawEngine.Services
{
    /// <summary>
    /// Produces a deterministic pseudo-random ordering of supplied eligible tickets.
    /// </summary>
    public sealed class TicketOrderBuilder
    {
        private readonly Sha256Hasher _hasher;

        public TicketOrderBuilder(Sha256Hasher hasher)
        {
            _hasher = hasher;
        }

        public IReadOnlyList<TTicket> Build<TTicket>(
            IReadOnlyList<TTicket> validTickets,
            string selectionSeedHex)
            where TTicket : IDrawTicket
        {
            ArgumentNullException.ThrowIfNull(validTickets);
            ArgumentException.ThrowIfNullOrWhiteSpace(selectionSeedHex);

            return validTickets
                .Select(ticket => new
                {
                    Ticket = ticket,
                    Hash = _hasher.HashToHex($"{selectionSeedHex}|{ticket.TicketNumber}")
                })
                .OrderBy(x => x.Hash, StringComparer.Ordinal)
                .ThenBy(x => x.Ticket.TicketNumber, StringComparer.Ordinal)
                .Select(x => x.Ticket)
                .ToArray();
        }
    }
}
