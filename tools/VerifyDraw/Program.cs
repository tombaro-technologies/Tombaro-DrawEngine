using System.Globalization;
using System.Text;
using System.Text.Json;
using Tombaro.DrawEngine.Abstractions;
using Tombaro.DrawEngine.Contracts;
using Tombaro.DrawEngine.Services;
using Tombaro.DrawEngine.Time;
using Tombaro.DrawEngine.Validation;

if (args.Length is not (1 or 3))
{
    Console.Error.WriteLine("Usage: VerifyDraw <proof.json> [<club-commitment-fingerprint> <club-snapshot-fingerprint>]");
    return 2;
}

try
{
    using var proof = JsonDocument.Parse(await File.ReadAllTextAsync(args[0]));
    var root = proof.RootElement;
    var snapshotJson = root.GetProperty("snapshotJson").GetString() ?? throw new InvalidDataException("Missing snapshot.");
    using var document = JsonDocument.Parse(snapshotJson);
    var snapshot = document.RootElement;
    var format = snapshot.GetProperty("formatVersion").GetInt32();
    var raffleId = snapshot.GetProperty("raffleId").GetString() ?? "";
    var closedUtc = snapshot.GetProperty("salesClosedUtc").GetString() ?? "";
    var version = snapshot.GetProperty("algorithmVersion").GetString() ?? "";
    var commitment = snapshot.GetProperty("nonceCommitment").GetString() ?? "";
    var winnerCount = snapshot.GetProperty("winnerCount").GetInt32();
    var numbers = snapshot.GetProperty("ticketNumbers").EnumerateArray().Select(x => x.GetInt32()).ToArray();
    var nonce = root.GetProperty("drawNonce").GetString() ?? "";
    var snapshotHash = root.GetProperty("snapshotFingerprint").GetString() ?? "";
    var commitmentHash = root.GetProperty("commitmentFingerprint").GetString() ?? "";

    if (format != 1 || version != DrawAlgorithm.Version || !Guid.TryParseExact(raffleId, "D", out var id)
        || id.ToString("D") != raffleId || !DateTime.TryParseExact(closedUtc, "O", CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind, out var closed) || closed.Kind != DateTimeKind.Utc
        || commitment.Length != 64 || !commitment.All(Uri.IsHexDigit) || commitment != commitment.ToUpperInvariant())
        throw new InvalidDataException("Unsupported or invalid snapshot format.");

    string WriteJson(Action<Utf8JsonWriter> write)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
            write(writer);
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    var canonicalSnapshot = WriteJson(writer =>
    {
        writer.WriteStartObject();
        writer.WriteNumber("formatVersion", format);
        writer.WriteString("raffleId", raffleId);
        writer.WriteString("salesClosedUtc", closedUtc);
        writer.WriteString("algorithmVersion", version);
        writer.WriteString("nonceCommitment", commitment);
        writer.WriteNumber("winnerCount", winnerCount);
        writer.WriteStartArray("ticketNumbers");
        foreach (var number in numbers.OrderBy(x => x))
            writer.WriteNumberValue(number);
        writer.WriteEndArray();
        writer.WriteEndObject();
    });
    var commitmentJson = WriteJson(writer =>
    {
        writer.WriteStartObject();
        writer.WriteNumber("formatVersion", format);
        writer.WriteString("raffleId", raffleId);
        writer.WriteString("algorithmVersion", version);
        writer.WriteString("nonceCommitment", commitment);
        writer.WriteEndObject();
    });

    var hasher = new Sha256Hasher();
    var checks = new List<(string Name, bool Pass)>
    {
        ("Nonce matches commitment", hasher.HashToHex(nonce) == commitment),
        ("Commitment fingerprint matches", hasher.HashToHex(commitmentJson) == commitmentHash),
        ("Canonical snapshot fingerprint matches", snapshotJson == canonicalSnapshot && hasher.HashToHex(snapshotJson) == snapshotHash)
    };
    var engine = new Tombaro.DrawEngine.Engine.DrawEngine(
        new DrawRequestValidator(), new SeedBuilder(hasher), new TicketOrderBuilder(hasher), new SystemClock());
    var result = engine.Execute(new DrawRequest<VerifyTicket>
    {
        AlgorithmVersion = version,
        DrawId = raffleId,
        DrawClosedUtc = closed,
        DrawNonce = nonce,
        WinnerCount = winnerCount,
        ValidTickets = numbers.Select(n => new VerifyTicket(n.ToString(CultureInfo.InvariantCulture))).ToArray()
    });
    checks.Add(("Draw input hash matches", result.BaseSeedHex == root.GetProperty("drawInputHash").GetString()));
    var publishedWinners = root.GetProperty("winners").EnumerateArray()
        .Select(x => (Position: x.GetProperty("position").GetInt32(), Number: x.GetProperty("ticketNumber").GetInt32()))
        .ToArray();
    checks.Add(("Winner order matches", result.Winners.Select(x => (x.DrawPosition,
        int.Parse(x.Ticket.TicketNumber, CultureInfo.InvariantCulture))).SequenceEqual(publishedWinners)));

    if (args.Length == 3)
    {
        checks.Add(("Club commitment fingerprint matches", string.Equals(args[1], commitmentHash, StringComparison.OrdinalIgnoreCase)));
        checks.Add(("Club snapshot fingerprint matches", string.Equals(args[2], snapshotHash, StringComparison.OrdinalIgnoreCase)));
    }
    else
    {
        Console.WriteLine("No independent club fingerprints provided: only internal consistency can be checked.");
    }

    foreach (var check in checks)
        Console.WriteLine($"{(check.Pass ? "PASS" : "FAIL")}: {check.Name}");
    return checks.All(x => x.Pass) ? 0 : 1;
}
catch (Exception exception) when (exception is JsonException or InvalidDataException or IOException
    or ArgumentException or InvalidOperationException or KeyNotFoundException or FormatException
    or Tombaro.DrawEngine.Exceptions.DrawValidationException)
{
    Console.Error.WriteLine($"Unable to verify draw: {exception.Message}");
    return 2;
}

internal sealed record VerifyTicket(string TicketNumber) : IDrawTicket;
