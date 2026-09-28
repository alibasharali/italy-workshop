# 0002 – Domain events go to an outbox and an event store, in the same transaction

**Status:** accepted (recorded 2026-09-28)

## Context

Things will want to react when a game is completed: notifications, statistics, a scoreboard on a big screen. Publishing to a broker from inside a handler would lose events whenever the database commit and the publish disagree.

## Decision

`AppDbContext.SaveChangesAsync` collects the domain events from the tracked entities, and in the **same transaction** as the state change it writes each event to:
- `OutboxMessages`, for asynchronous publishing
- `EventStore`, an append-only audit log, versioned per stream

The stream id is the event's first `Guid` property whose name ends in `Id`.

## Consequences

- Raise an event with `AddDomainEvent` in the entity. Never publish from a handler.
- **No outbox processor exists yet.** `InMemoryMessagePublisher`, `DomainEventDispatcher` and the handlers in `Application/*/EventHandlers` are placeholders. A story that needs events delivered has to build the processor.
- The event store is the natural source for "who changed what, and when" (story #19) and for corrections (#14).
