using System.Globalization;
using Tombaro.DrawEngine.Contracts;
using Tombaro.DrawEngine.Services;
using Tombaro.DrawEngine.Tests.TestDoubles;
using Tombaro.DrawEngine.Validation;

namespace Tombaro.DrawEngine.Tests;

public sealed class DrawDistributionTests
{
    [TestCase(1, false)]
    [TestCase(5, true)]
    [TestCase(20, false)]
    public void IndependentDrawsDoNotPreferEarlyTicketPositions(int winnerCount, bool sparseNumbers)
    {
        // Fixed independent inputs make this a reproducible regression diagnostic, not a flaky live-random test.
        const int draws = 5000, count = 100;
        var hasher = new Sha256Hasher();
        var engine = new Tombaro.DrawEngine.Engine.DrawEngine(new DrawRequestValidator(), new SeedBuilder(hasher),
            new TicketOrderBuilder(hasher), new FakeClock(new DateTime(2026, 10, 26, 18, 0, 0, DateTimeKind.Utc)));
        var tickets = Enumerable.Range(0, count).Select(i => new TestDrawTicket {
            TicketNumber = (sparseNumbers ? 1000 + i * 7 : i + 1).ToString(CultureInfo.InvariantCulture),
            InternalId = Guid.Empty }).ToArray();
        var positions = tickets.Select((t, i) => (t.TicketNumber, i)).ToDictionary(x => x.TicketNumber, x => x.i);
        var mainBuckets = new int[5]; var allBuckets = new int[5];
        for (var draw = 0; draw < draws; draw++) {
            var result = engine.Execute(new DrawRequest<TestDrawTicket> {
                AlgorithmVersion = "1", DrawId = $"distribution-{winnerCount}-{draw}",
                DrawClosedUtc = new DateTime(2026, 10, 26, 18, 0, 0, DateTimeKind.Utc),
                DrawNonce = hasher.HashToHex($"independent-test-secret-{draw}"), WinnerCount = winnerCount,
                ValidTickets = draw % 2 == 0 ? tickets : tickets.Reverse().ToArray() });
            Assert.That(result.Winners.Select(x => x.Ticket.TicketNumber).Distinct().Count(), Is.EqualTo(winnerCount));
            mainBuckets[positions[result.Winners[0].Ticket.TicketNumber] / 20]++;
            foreach (var winner in result.Winners) allBuckets[positions[winner.Ticket.TicketNumber] / 20]++;
        }
        TestContext.Progress.WriteLine($"{draws} draws, {winnerCount} winner(s), first to last ticket fifths: main=[{string.Join(",", mainBuckets)}], all=[{string.Join(",", allBuckets)}]");
        // Wide bounds flag a substantial positional bias. Passing is not a proof of perfect randomness.
        Assert.That(mainBuckets.All(x => x >= 850 && x <= 1150), Is.True, string.Join(",", mainBuckets));
        Assert.That(allBuckets.All(x => Math.Abs(x - draws * winnerCount / 5.0) <= draws * winnerCount * .04), Is.True, string.Join(",", allBuckets));
    }
}
