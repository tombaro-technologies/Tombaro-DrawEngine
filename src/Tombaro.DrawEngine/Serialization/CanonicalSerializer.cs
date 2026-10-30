using Tombaro.DrawEngine.Abstractions;
using Tombaro.DrawEngine.Contracts;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Tombaro.DrawEngine.Serialization
{
    public static class CanonicalSerializer
    {
        public static string SerializeDrawInput<TTicket>(DrawRequest<TTicket> request)
            where TTicket : IDrawTicket
        {
            ArgumentNullException.ThrowIfNull(request);

            var ticketNumbers = request.ValidTickets
                .Select(x => int.Parse(x.TicketNumber, NumberStyles.None, CultureInfo.InvariantCulture))
                .OrderBy(x => x);

            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream))
            {
                writer.WriteStartObject();
                writer.WriteString("algorithmVersion", request.AlgorithmVersion);
                writer.WriteString("drawId", request.DrawId);
                writer.WriteString("drawClosedUtc", request.DrawClosedUtc.ToString("O", CultureInfo.InvariantCulture));
                writer.WriteString("drawNonce", request.DrawNonce);
                writer.WriteNumber("winnerCount", request.WinnerCount);
                writer.WriteStartArray("ticketNumbers");
                foreach (var number in ticketNumbers)
                    writer.WriteNumberValue(number);
                writer.WriteEndArray();
                writer.WriteEndObject();
            }

            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }
}
