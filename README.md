# Tombaro Draw Engine

A small, deterministic and independently verifiable draw engine.

The engine has one responsibility:

> Select **X unique winners** from a supplied set of **valid tickets** and return those original ticket objects in draw order.

It deliberately does not contain Tombaro, raffle prize or regulatory rules. The calling application decides which tickets are eligible and how many winners are required.

For a non-technical explanation see:

- [How it works - English](docs/HOW-IT-WORKS.md)
- [So funktioniert die Ziehung - Deutsch](docs/SO-FUNKTIONIERT-DIE-ZIEHUNG.md)

## Build and test

Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0). The draw engine itself has no external package dependencies. Its test project restores NUnit and other test packages from NuGet.

The MIT license applies to this repository's own source and documentation. Restored third-party packages have their own licenses; they are not included in the repository.

```sh
dotnet build src/Tombaro.DrawEngine/Tombaro.DrawEngine.csproj
dotnet test tests/Tombaro.DrawEngine.Tests/Tombaro.DrawEngine.Tests.csproj
dotnet run --project examples/BasicDraw/BasicDraw.csproj
```

The example uses fixed fictional data; the winner numbers should be `4`, `57`, and `15`. It is not suitable as a source of randomness for a real draw. See [the example source](examples/BasicDraw/Program.cs) for complete engine construction.

## Verify an exported Tombaro draw

The mobile live display can export a completed draw's public verification data as JSON. To check it with this separate command-line program:

```sh
dotnet run --project tools/VerifyDraw/VerifyDraw.csproj -- examples/verification-example.json
```

Pass the two fingerprints from the club's independently retained commitment and ticket-list receipts as additional arguments to compare against those earlier records:

```sh
dotnet run --project tools/VerifyDraw/VerifyDraw.csproj -- examples/verification-example.json 76F7E83B282AF1878A66AF9975751E7A58299A39BE0263872EFAFA8524BAF80B 6F90156448533E638007F635DC9429137B037986BDF99D88359F7CDFB82AE662
```

The example values are fictional. Replace them and the file path with the actual export and receipts. Exit code `0` means all checks performed passed, `1` means a mismatch, and `2` means the data or command could not be processed. Without both independently retained fingerprints, this checks consistency of the supplied data only. The tool does not validate PDF signatures, establish the signing time, or prove the initial ticket list is complete.

## Ticket contract

Applications integrate through one minimal interface:

```csharp
public interface IDrawTicket
{
    string TicketNumber { get; }
}
```

An application may attach any additional data it needs:

```csharp
public sealed class MyDrawTicket : IDrawTicket
{
    public required string TicketNumber { get; init; }
    public required Guid InternalTicketId { get; init; }
    public required Guid SaleId { get; init; }
}
```

Only `TicketNumber` affects the draw. Additional application-specific fields are carried through unchanged and are never hashed or inspected by the engine.

## Input

```csharp
var request = new DrawRequest<MyDrawTicket>
{
    AlgorithmVersion = "1",
    DrawId = "draw-123",
    DrawClosedUtc = new DateTime(2026, 8, 24, 18, 0, 0, DateTimeKind.Utc),
    DrawNonce = "published-draw-nonce",
    WinnerCount = 2,
    ValidTickets = tickets
};
```

The engine does not calculate percentages, prize quotas or regulatory minimums.

## Output

The result contains the original application-owned ticket objects:

```csharp
foreach (var winner in result.Winners)
{
    Console.WriteLine($"{winner.DrawPosition}: {winner.Ticket.TicketNumber}");
}
```

`DrawPosition` is 1-based. Position 1 is the first selected winner, position 2 the second, and so on. The calling application decides what those positions mean, for example which prize is assigned to each position.

Because the original `TTicket` instance is returned, the calling application does not need to look the winning ticket up again by ticket number.

## Determinism and independent verification

Every input that can affect winner selection is included in the canonical draw input:

- algorithm version;
- draw identifier;
- draw closing timestamp;
- draw nonce;
- winner count; and
- the complete set of eligible **ticket numbers**.

Version 1 accepts ticket numbers as canonical positive decimal integers from 1 through 2147483647, without leading zeroes, signs, spaces or other characters. They are serialized as JSON numbers in ascending numeric order, so the caller's collection order cannot change the result. The ticket interface represents these numbers as strings; `"001"` is invalid, not an alternative representation of `"1"`.

The canonical input is a compact JSON object encoded as UTF-8, without a byte-order mark or trailing newline. Fields appear in exactly this order: `algorithmVersion` (string), `drawId` (string), `drawClosedUtc` (UTC .NET round-trip format with seven fractional digits and `Z`), `drawNonce` (string), `winnerCount` (integer), `ticketNumbers` (array of integers). JSON escaping is that of .NET's default `Utf8JsonWriter`. Hash the exact bytes, not a reformatted JSON file. The engine currently accepts only algorithm version `1`.

Example canonical input:

```json
{"algorithmVersion":"1","drawId":"draw-123","drawClosedUtc":"2026-08-24T18:00:00.0000000Z","drawNonce":"test-nonce","winnerCount":3,"ticketNumbers":[4,7,11,15,22,31,48,57]}
```

For these bytes the base seed is `6F47FB14DA7291A2A407E4739E6BDA7746753D46FDD0ADCD196C21BBA558C516`, the selection seed is `93C11881322E43E26BE3D7E95A666C4FD6187BAC9581BC4D7312A8BFACD9A034`, and the winners in order are `4`, `57`, `15`.

```text
BaseSeed = SHA256(CanonicalDrawInput)
SelectionSeed = SHA256(BaseSeed + "|selection")
TicketHash = SHA256(SelectionSeed + "|" + TicketNumber)
```

Each intermediate hash is represented as uppercase hexadecimal text; the next hash uses the UTF-8 bytes of that text, including the literal separators. Tickets are ordered by uppercase `TicketHash` and then by the canonical ticket-number string using ordinal comparison as a tie-breaker. The first `WinnerCount` tickets are the winners.

Application-specific fields such as database IDs are deliberately excluded. An independent verifier only needs the public draw inputs and ticket numbers, not access to the calling application's database.

## Validation

The engine requires:

- non-empty `AlgorithmVersion`, `DrawId` and `DrawNonce`;
- a UTC `DrawClosedUtc`;
- `WinnerCount > 0`;
- at least one valid ticket;
- a unique canonical positive integer `TicketNumber` in the range 1–2147483647 for every ticket;
- enough valid tickets to satisfy `WinnerCount`.

If validation fails, no draw is performed and a `DrawValidationException` is thrown. Unknown algorithm versions are rejected.

## What the engine does not decide

The engine does not know how many tickets were registered, what winner percentage applies, which laws apply, what prizes exist, who bought a ticket, or what should happen when a draw cannot proceed. Those responsibilities remain entirely with the calling application.

## Algorithm versioning

The canonical serialization and winner-selection behavior are part of the algorithm version. Once a version has been used for a completed draw, it must remain stable so historical draws remain reproducible.

## Security note about the draw nonce

The draw nonce affects the result. The surrounding system is responsible for generating, recording and, where appropriate, committing or publishing it in a way that prevents a party from repeatedly changing the nonce after all other inputs are known.

## License

This project is licensed under the [MIT License](LICENSE).


## Nonce lifecycle and commit-reveal integration

The engine accepts the draw nonce as an input and deliberately does not prescribe how an integrating application stores it. A production integration should use a commit-reveal lifecycle:

1. Generate a nonce with a cryptographically secure random-number generator before ticket sales begin.
2. Persist the nonce as a write-once secret and publish or audit SHA256(nonce) as its commitment.
3. Close sales and freeze the complete eligible-ticket set.
4. Execute the draw with the previously committed nonce.
5. Reveal the nonce and persist the complete canonical inputs and outputs so an independent verifier can confirm both the commitment and the result.

Tombaro generates a 256-bit nonce when the raffle is created, records its SHA-256 commitment in the append-only raffle audit trail, keeps the nonce out of pre-draw API DTOs, and reveals it only as part of the completed draw's verification record. A verifier first checks that SHA256(revealed nonce) equals the earlier commitment and then reproduces the draw from the recorded inputs.

In Tombaro, the nonce is stored as an uppercase hexadecimal string, and its commitment is the SHA-256 hash of the UTF-8 bytes of that string, not of the decoded bytes. The club retains a signed commitment receipt before sales and a snapshot receipt after sales. Comparing with those independently retained receipts matters: a comparison using only data supplied later by the same server does not independently establish what was fixed beforehand. This process does not by itself prove that the original ticket list was complete or that the person who knew the nonce could not influence other inputs.
