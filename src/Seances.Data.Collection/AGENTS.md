# Collection project

Collection owns UGC requests, HTML parsing, source DTOs, and mapping from source data to Domain. Keep AngleSharp, source selectors, retry policy, and source-specific values here. Do not depend on Storage or Web.

## Structure and responsibilities

- Keep UGC implementation under `Ugc/` and align namespaces with folders.
- Keep request retry and pacing in the HTTP client; schedule and film retrieval in their own source modules; calendar, schedule, and film HTML parsing in focused parser types; source DTOs in `Models/`; and domain conversion in `Mapping/`.
- Keep `UgcAdapter` as the small entry point that coordinates schedule and film collection. Put source parsing, retry policy, and individual workflow steps in focused collaborators.
- Give each method one clear step. Extract stages with meaningful names when a method starts combining lookup, parsing, validation, mapping, or recovery.
- Store shared UGC URLs, cinema identity, time zone, request parameters, and provenance labels in named source configuration values. Reuse each value at every call site.
- Always put braces around `if`, `foreach`, `for`, `while`, and `catch` bodies, including single-line bodies.
- Keep source parsing deterministic and test it with synthetic HTML, without making HTTP requests. Keep request, retry, and pacing behavior covered by adapter tests with scripted handlers.
