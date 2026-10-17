## Repository guidance

Read `docs/agents/domain.md` before changing domain terminology. Read `docs/agents/issue-tracker.md` before tracker operations.

## Solution architecture

- `src/Seances.Web` owns the web host, Razor UI, HTTP endpoints, startup, and composition.
- `src/Seances.Application` coordinates user-facing workflows across capabilities; it owns schedule refresh orchestration.
- `src/Seances.Data.Collection` owns UGC requests, HTML parsing, external DTOs, and mapping to domain models.
- `src/Seances.Data.Storage` owns snapshot persistence, storage documents, and validation of persisted data.
- `src/Seances.Domain` owns internal models, invariants, and deterministic business rules; it has no presentation or infrastructure dependencies.
- Dependencies flow from Web to Application and its used capabilities, from Application to Collection, Storage, and Domain, and from Collection and Storage to Domain. Keep references acyclic.

## Shared conventions

- Keep namespaces aligned with project and folder names. Use one primary type per file, with a matching filename.
- Preserve established cinema and screening terminology in `GLOSSARY.md`.
- Prefer small, direct methods and explicit mappings at external and persistence boundaries.
- Target the stable .NET LTS version recorded in project files and keep dependencies compatible.
