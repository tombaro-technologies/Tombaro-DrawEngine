# How the Tombaro Draw Works

[Deutsch: So funktioniert die Ziehung](SO-FUNKTIONIERT-DIE-ZIEHUNG.md)

This page explains the Tombaro Draw Engine without requiring programming knowledge.

## What does the Draw Engine do?

The Draw Engine has one simple task:

> It receives a list of valid ticket numbers and selects the required number of winners from that list.

For example, suppose 83 tickets were sold and the raffle rules require two winners. Tombaro passes the 83 valid ticket numbers to the Draw Engine and tells it to select two winners.

The engine returns two different ticket numbers. A ticket that was not included in the valid ticket list cannot win.

## Who decides how many winners there are?

Not the Draw Engine.

The rules of the raffle determine how many winners are required. Those rules may depend on the jurisdiction, the approved raffle plan or choices made by the organizer.

The Draw Engine only receives the final instruction, for example:

> Select 2 winners from these 83 valid tickets.

This separation is intentional. It keeps legal and raffle-specific rules out of the drawing algorithm.

## Is the draw reproducible?

Yes. This is one of the most important properties of the system.

The engine uses cryptographic SHA-256 hashing to create a deterministic ordering of the valid tickets. The required number of tickets at the beginning of that ordering become the winners.

"Deterministic" means that the same complete draw information always produces the same result.

If ticket numbers 47 and 126 are selected today, somebody who later verifies the draw using exactly the same inputs and the same algorithm will again obtain ticket numbers 47 and 126 in the same order.

## Why is reproducibility useful?

A traditional draw may require observers to trust that the process was conducted correctly at that particular moment.

A reproducible digital draw can additionally be checked afterwards.

A club, auditor, regulator or technically interested participant can use the published algorithm and the original draw information to reproduce the result independently.

If any relevant draw information has been changed, the cryptographic fingerprint of the draw changes as well.

## Does the engine know who owns a ticket?

No.

The Draw Engine works with ticket sequence numbers. It does not need to know a participant's name, address, payment information or how much that person spent.

It also does not know which prize a particular winning position represents. The application using the engine assigns prizes after the winners have been selected.

## Can an unsold or invalid ticket win?

No, provided it is not included in the valid ticket list supplied to the engine.

The engine only orders and selects from the ticket numbers it receives as eligible tickets.

## What if there are not enough valid tickets?

The Draw Engine refuses to perform a draw if it is asked to select more unique winners than there are valid tickets.

What should then happen to the raffle - for example cancellation or refunds - is determined by the raffle rules and the application using the Draw Engine.

## What information affects the result?

The draw uses a defined set of inputs, including:

- the algorithm version;
- the raffle identifier;
- the end of ticket sales;
- the draw nonce;
- the required number of winners; and
- the complete set of valid ticket numbers.

These inputs are combined in a defined format and cryptographically hashed. The resulting values are then used to order the valid tickets.

The technical README in this repository documents the exact process.

## Why is there a draw nonce?

The nonce is an additional value that becomes part of the draw input. Changing it changes the result.

For that reason, the surrounding raffle system must have a clear process for generating and recording the nonce. It must not be treated as a way to repeatedly try different draws until a preferred result appears.

The technical documentation calls this out explicitly so that systems using the open-source engine can choose an appropriate transparent nonce process.

## Why publish the source code?

The purpose is transparency.

Clubs and participants should not have to rely only on the statement that the drawing software works correctly. Publishing the Draw Engine allows independent developers, auditors and other interested parties to inspect the actual winner-selection algorithm.

The algorithm can therefore be both **examined** and **reproduced**.

## Want the technical details?

The repository's main `README.md` describes the exact canonical input format, SHA-256 operations, ticket ordering, validation rules and information required to independently reproduce a draw.


## How does Tombaro prevent nonce substitution?

Tombaro uses a commit-reveal process. When the raffle is created, it generates a cryptographically random 256-bit nonce. The nonce remains secret until the draw, while its SHA-256 hash is immediately recorded as a commitment in the append-only audit trail.

After sales close, Tombaro uses that previously committed nonce for the draw. It then reveals the nonce together with the complete verification record. An independent verifier can first confirm that the revealed nonce matches the earlier commitment and then reproduce the draw result.

The earlier commitment makes a later change to the nonce detectable when it is compared with an independently retained copy. This depends on the commitment being generated and retained before sales, and does not by itself prove that the ticket list is complete or that no one who knew the nonce could influence other draw inputs.
