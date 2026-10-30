using Tombaro.DrawEngine.Abstractions;
using Tombaro.DrawEngine.Contracts;
using Tombaro.DrawEngine.Exceptions;

namespace Tombaro.DrawEngine.Validation
{
    /// <summary>
    /// Validates all inputs required by the deterministic winner-selection algorithm.
    /// </summary>
    public sealed class DrawRequestValidator
    {
        public void ValidateAndThrow<TTicket>(DrawRequest<TTicket> request)
            where TTicket : IDrawTicket
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.AlgorithmVersion != DrawAlgorithm.Version)
                throw new DrawValidationException("Unsupported algorithm version.");

            if (string.IsNullOrWhiteSpace(request.DrawId))
                throw new DrawValidationException("DrawId is required.");

            if (request.DrawClosedUtc.Kind != DateTimeKind.Utc)
                throw new DrawValidationException("DrawClosedUtc must be specified as UTC.");

            if (string.IsNullOrWhiteSpace(request.DrawNonce))
                throw new DrawValidationException("DrawNonce is required.");

            if (request.WinnerCount <= 0)
                throw new DrawValidationException("WinnerCount must be greater than zero.");

            if (request.ValidTickets is null || request.ValidTickets.Count == 0)
                throw new DrawValidationException("At least one valid ticket is required.");

            var invalidNumbers = request.ValidTickets
                .Where(x => x is null || string.IsNullOrWhiteSpace(x.TicketNumber))
                .ToArray();

            if (invalidNumbers.Length > 0)
                throw new DrawValidationException("Every valid ticket must have a non-empty TicketNumber.");

            if (request.ValidTickets.Any(x => !IsCanonicalTicketNumber(x.TicketNumber)))
                throw new DrawValidationException("Ticket numbers must be positive decimal integers without leading zeroes, up to 2147483647.");

            var duplicates = request.ValidTickets
                .GroupBy(x => x.TicketNumber, StringComparer.Ordinal)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToArray();

            if (duplicates.Length > 0)
                throw new DrawValidationException(
                    $"ValidTickets contains duplicate ticket numbers: {string.Join(", ", duplicates)}");

            if (request.WinnerCount > request.ValidTickets.Count)
                throw new DrawValidationException(
                    $"WinnerCount ({request.WinnerCount}) cannot exceed the number of valid tickets ({request.ValidTickets.Count}).");
        }

        /// <summary>
        /// Determines whether a ticket number is in canonical numeric form.
        /// </summary>
        /// <param name="number">The ticket number text to validate. Must be a non-empty string of decimal digits with no leading zero.</param>
        /// <returns><see langword="true"/> when the value starts with a digit from 1 through 9, contains only decimal digits,
        /// and can be parsed as a 32-bit integer using invariant culture and no number styles; otherwise, <see
        /// langword="false"/>.</returns>
        private static bool IsCanonicalTicketNumber(string number)
        {
            if (number[0] < '1' || number[0] > '9' || number.Any(c => c < '0' || c > '9'))
                return false;

            return int.TryParse(number, System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture, out _);
        }
    }
}
