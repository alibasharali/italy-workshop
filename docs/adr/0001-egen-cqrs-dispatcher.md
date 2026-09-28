# 0001 – Our own CQRS dispatcher, no MediatR

**Status:** accepted (recorded 2026-09-28; the decision is older than the record)

## Context

Controllers need one way to reach use cases, with cross-cutting behaviour (validation, tracing, metrics) around every call. MediatR is the common choice. Why the original authors didn't use it wasn't written down. One reason that holds today: MediatR has had a commercial licence since v13.

## Decision

- The Application layer has its own small dispatcher: `ISender` / `Sender`, with `ICommand`, `ICommand<T>` and `IQuery<T>`, and handlers found by Scrutor.
- Pipeline behaviours run in a fixed order: `ObservabilityBehavior` outermost, then `ValidationBehavior`.
- Handlers return `Result`/`Result<T>` with an `Error`, not exceptions. A missing handler becomes an `Error.Unexpected` too, so it is traced like any other failure.

## Consequences

- No licence or version dependency, and every line of the pipeline is ours to read.
- A new cross-cutting concern means a new `IPipelineBehavior` registered in `Common/DependencyInjection.cs`, in the right order.
- Don't introduce MediatR, or another mediator next to this one.
