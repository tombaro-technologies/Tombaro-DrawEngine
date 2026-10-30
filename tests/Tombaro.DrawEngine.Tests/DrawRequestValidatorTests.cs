using Tombaro.DrawEngine.Contracts;
using Tombaro.DrawEngine.Exceptions;
using Tombaro.DrawEngine.Tests.TestDoubles;
using Tombaro.DrawEngine.Validation;

namespace Tombaro.DrawEngine.Tests
{
    public sealed class DrawRequestValidatorTests
    {
        private readonly DrawRequestValidator _validator = new();

        [TestCase("0")]
        [TestCase("01")]
        [TestCase("-1")]
        [TestCase("+1")]
        [TestCase("A-123")]
        [TestCase("1,2")]
        [TestCase("１２")]
        [TestCase("2147483648")]
        public void ValidateAndThrow_NonCanonicalTicketNumber_Throws(string number)
        {
            var request = CreateRequest() with { ValidTickets = CreateTickets("10", number) };
            Assert.Throws<DrawValidationException>(() => _validator.ValidateAndThrow(request));
        }

        [TestCase("")]
        [TestCase("2")]
        [TestCase("unknown")]
        public void ValidateAndThrow_UnsupportedVersion_Throws(string version)
        {
            var request = CreateRequest() with { AlgorithmVersion = version };
            Assert.Throws<DrawValidationException>(() => _validator.ValidateAndThrow(request));
        }

        [Test]
        public void ValidateAndThrow_WinnerCountExceedsValidTickets_Throws()
        {
            var request = CreateRequest() with
            {
                WinnerCount = 3,
                ValidTickets = CreateTickets("10", "20")
            };

            Assert.Throws<DrawValidationException>(() => _validator.ValidateAndThrow(request));
        }

        [Test]
        public void ValidateAndThrow_DuplicateTicketNumbers_Throws()
        {
            var request = CreateRequest() with
            {
                ValidTickets = CreateTickets("10", "20", "20")
            };

            Assert.Throws<DrawValidationException>(() => _validator.ValidateAndThrow(request));
        }

        [TestCase("")]
        [TestCase("   ")]
        public void ValidateAndThrow_EmptyTicketNumber_Throws(string ticketNumber)
        {
            var request = CreateRequest() with
            {
                ValidTickets = CreateTickets("10", ticketNumber)
            };

            Assert.Throws<DrawValidationException>(() => _validator.ValidateAndThrow(request));
        }

        [Test]
        public void ValidateAndThrow_ValidRequest_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _validator.ValidateAndThrow(CreateRequest()));
        }

        private static DrawRequest<TestDrawTicket> CreateRequest() => new()
        {
            AlgorithmVersion = "1",
            DrawId = "draw-123",
            DrawClosedUtc = new DateTime(2026, 8, 24, 18, 0, 0, DateTimeKind.Utc),
            DrawNonce = "test-nonce",
            WinnerCount = 2,
            ValidTickets = CreateTickets("10", "20", "30")
        };

        private static IReadOnlyList<TestDrawTicket> CreateTickets(params string[] numbers) =>
            numbers.Select(number => new TestDrawTicket
            {
                TicketNumber = number,
                InternalId = Guid.NewGuid()
            }).ToArray();
    }
}
