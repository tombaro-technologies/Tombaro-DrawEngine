using Tombaro.DrawEngine.Abstractions;
using Tombaro.DrawEngine.Contracts;
using Tombaro.DrawEngine.Services;
using Tombaro.DrawEngine.Time;
using Tombaro.DrawEngine.Validation;

var hasher = new Sha256Hasher();
var engine = new Tombaro.DrawEngine.Engine.DrawEngine(
    new DrawRequestValidator(), new SeedBuilder(hasher), new TicketOrderBuilder(hasher), new SystemClock());
var request = new DrawRequest<ExampleTicket>
{
    AlgorithmVersion = DrawAlgorithm.Version,
    DrawId = "draw-123",
    DrawClosedUtc = new DateTime(2026, 8, 24, 18, 0, 0, DateTimeKind.Utc),
    DrawNonce = "test-nonce",
    WinnerCount = 3,
    ValidTickets = new[] { 4, 7, 11, 15, 22, 31, 48, 57 }
        .Select(number => new ExampleTicket(number.ToString(System.Globalization.CultureInfo.InvariantCulture)))
        .ToArray()
};
var result = engine.Execute(request);
Console.WriteLine($"Input hash: {result.BaseSeedHex}");
Console.WriteLine($"Selection seed: {result.SelectionSeedHex}");
foreach (var winner in result.Winners)
    Console.WriteLine($"{winner.DrawPosition}: ticket {winner.Ticket.TicketNumber}");

internal sealed record ExampleTicket(string TicketNumber) : IDrawTicket;
