using Tombaro.DrawEngine.Abstractions;
using Tombaro.DrawEngine.Contracts;
using Tombaro.DrawEngine.Services;
using Tombaro.DrawEngine.Validation;

namespace Tombaro.DrawEngine.Engine
{
    /// <summary>
    /// Deterministically selects a requested number of unique winners from supplied valid tickets.
    /// </summary>
    public sealed class DrawEngine : IDrawEngine
    {
        private readonly DrawRequestValidator _validator;
        private readonly SeedBuilder _seedBuilder;
        private readonly TicketOrderBuilder _ticketOrderBuilder;
        private readonly IClock _clock;

        public DrawEngine(
            DrawRequestValidator validator,
            SeedBuilder seedBuilder,
            TicketOrderBuilder ticketOrderBuilder,
            IClock clock)
        {
            _validator = validator;
            _seedBuilder = seedBuilder;
            _ticketOrderBuilder = ticketOrderBuilder;
            _clock = clock;
        }

        public DrawResult<TTicket> Execute<TTicket>(DrawRequest<TTicket> request)
            where TTicket : IDrawTicket
        {
            ArgumentNullException.ThrowIfNull(request);
            _validator.ValidateAndThrow(request);

            var seeds = _seedBuilder.Build(request);
            var orderedTickets = _ticketOrderBuilder.Build(
                request.ValidTickets,
                seeds.SelectionSeedHex);

            var winners = orderedTickets
                .Take(request.WinnerCount)
                .Select((ticket, index) => new DrawWinner<TTicket>
                {
                    DrawPosition = index + 1,
                    Ticket = ticket
                })
                .ToArray();

            return new DrawResult<TTicket>
            {
                AlgorithmVersion = request.AlgorithmVersion,
                DrawId = request.DrawId,
                DrawExecutedUtc = _clock.UtcNow,
                BaseSeedHex = seeds.BaseSeedHex,
                SelectionSeedHex = seeds.SelectionSeedHex,
                Winners = winners,
                AuditInfo = new DrawAuditInfo
                {
                    ValidTicketCount = request.ValidTickets.Count,
                    WinnerCount = request.WinnerCount,
                    DrawInputHashHex = seeds.BaseSeedHex
                }
            };
        }
    }
}
