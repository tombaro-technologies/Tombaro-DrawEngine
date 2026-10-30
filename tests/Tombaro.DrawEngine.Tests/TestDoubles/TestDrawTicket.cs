using Tombaro.DrawEngine.Abstractions;

namespace Tombaro.DrawEngine.Tests.TestDoubles
{
    internal sealed record TestDrawTicket : IDrawTicket
    {
        public required string TicketNumber { get; init; }
        public required Guid InternalId { get; init; }
    }
}
