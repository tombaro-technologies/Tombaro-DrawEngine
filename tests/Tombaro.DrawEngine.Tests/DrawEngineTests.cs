using Tombaro.DrawEngine.Contracts;
using Tombaro.DrawEngine.Services;
using Tombaro.DrawEngine.Tests.TestDoubles;
using Tombaro.DrawEngine.Validation;
using Tombaro.DrawEngine.Serialization;

namespace Tombaro.DrawEngine.Tests
{
    public sealed class DrawEngineTests
    {
        [Test]
        public void Version1_MatchesPublishedTestVector()
        {
            var request = CreateRequest();
            var result = CreateEngine().Execute(request);
            Assert.That(CanonicalSerializer.SerializeDrawInput(request), Is.EqualTo(
                "{\"algorithmVersion\":\"1\",\"drawId\":\"draw-123\",\"drawClosedUtc\":\"2026-08-24T18:00:00.0000000Z\",\"drawNonce\":\"test-nonce\",\"winnerCount\":3,\"ticketNumbers\":[4,7,11,15,22,31,48,57]}"));
            Assert.That(result.BaseSeedHex, Is.EqualTo("6F47FB14DA7291A2A407E4739E6BDA7746753D46FDD0ADCD196C21BBA558C516"));
            Assert.That(result.SelectionSeedHex, Is.EqualTo("93C11881322E43E26BE3D7E95A666C4FD6187BAC9581BC4D7312A8BFACD9A034"));
            Assert.That(result.Winners.Select(x => x.Ticket.TicketNumber), Is.EqualTo(new[] { "4", "57", "15" }));
        }

        [Test]
        public void Execute_SameInputs_ProducesSameWinners()
        {
            var engine = CreateEngine();
            var request = CreateRequest();

            var first = engine.Execute(request);
            var second = engine.Execute(request);

            Assert.That(second.Winners.Select(x => x.Ticket.TicketNumber),
                Is.EqualTo(first.Winners.Select(x => x.Ticket.TicketNumber)));
            Assert.That(second.BaseSeedHex, Is.EqualTo(first.BaseSeedHex));
            Assert.That(second.SelectionSeedHex, Is.EqualTo(first.SelectionSeedHex));
        }

        [Test]
        public void Execute_ReturnsOriginalTicketObjectsWithDrawPositions()
        {
            var engine = CreateEngine();
            var request = CreateRequest();

            var result = engine.Execute(request);

            Assert.That(result.Winners, Has.Count.EqualTo(request.WinnerCount));
            Assert.That(result.Winners.Select(x => x.DrawPosition), Is.EqualTo(new[] { 1, 2, 3 }));
            Assert.That(result.Winners.All(x => request.ValidTickets.Contains(x.Ticket)), Is.True);
            Assert.That(result.Winners.Select(x => x.Ticket.TicketNumber).Distinct().Count(),
                Is.EqualTo(request.WinnerCount));
        }

        [Test]
        public void Execute_DifferentInputTicketOrder_ProducesSameDraw()
        {
            var engine = CreateEngine();
            var request = CreateRequest();
            var reordered = request with
            {
                ValidTickets = request.ValidTickets.Reverse().ToArray()
            };

            var first = engine.Execute(request);
            var second = engine.Execute(reordered);

            Assert.That(second.BaseSeedHex, Is.EqualTo(first.BaseSeedHex));
            Assert.That(second.Winners.Select(x => x.Ticket.TicketNumber),
                Is.EqualTo(first.Winners.Select(x => x.Ticket.TicketNumber)));
        }

        [Test]
        public void Execute_ApplicationSpecificTicketData_DoesNotAffectDraw()
        {
            var engine = CreateEngine();
            var request = CreateRequest();
            var sameNumbersDifferentIds = request with
            {
                ValidTickets = request.ValidTickets
                    .Select(x => new TestDrawTicket
                    {
                        TicketNumber = x.TicketNumber,
                        InternalId = Guid.NewGuid()
                    })
                    .ToArray()
            };

            var first = engine.Execute(request);
            var second = engine.Execute(sameNumbersDifferentIds);

            Assert.That(second.BaseSeedHex, Is.EqualTo(first.BaseSeedHex));
            Assert.That(second.Winners.Select(x => x.Ticket.TicketNumber),
                Is.EqualTo(first.Winners.Select(x => x.Ticket.TicketNumber)));
        }

        private static Tombaro.DrawEngine.Engine.DrawEngine CreateEngine()
        {
            var hasher = new Sha256Hasher();
            return new Tombaro.DrawEngine.Engine.DrawEngine(
                new DrawRequestValidator(),
                new SeedBuilder(hasher),
                new TicketOrderBuilder(hasher),
                new FakeClock(new DateTime(2026, 8, 24, 18, 30, 0, DateTimeKind.Utc)));
        }

        private static DrawRequest<TestDrawTicket> CreateRequest() => new()
        {
            AlgorithmVersion = "1",
            DrawId = "draw-123",
            DrawClosedUtc = new DateTime(2026, 8, 24, 18, 0, 0, DateTimeKind.Utc),
            DrawNonce = "test-nonce",
            WinnerCount = 3,
            ValidTickets = new[] { "4", "7", "11", "15", "22", "31", "48", "57" }
                .Select(number => new TestDrawTicket
                {
                    TicketNumber = number,
                    InternalId = Guid.NewGuid()
                })
                .ToArray()
        };
    }
}
