# Technical plan

Status: implementation direction settled on 2026-10-17: Blazor static SSR, Container Apps Consumption, a scheduled Job sharing its image, and a JSON snapshot in Blob Storage. Prioritize simplicity and low operating cost. Container Apps deployment remains conditional on a credible estimate fitting €5/month. Sizing follows local prototype measurements. Nothing has been deployed.

## Recommended architecture

One .NET 10 Blazor Web App using static SSR and one container image:

```text
Browser -> Blazor SSR Container App -> Blob Storage snapshot
                                             ^
Scheduled Container Apps Job (same image) ----+
  -> UGC pages/fragments -> parse -> validate -> publish snapshot
```

The application supports two entry modes: serve the website, or run a refresh and exit. Shared C# code handles collection, normalization, finish calculations, and data access. There is no separately maintained frontend application or HTTP API required.

- UI: server-rendered Razor components and CSS. Filters use ordinary GET query parameters, with a little JavaScript for browser preferences and interaction where necessary. Do not enable a global interactive render mode just to use Blazor.
- Collection: built-in HttpClient plus AngleSharp for the verified HTML fragments.
- Data: built-in System.Text.Json, one complete versioned JSON snapshot in Blob Storage. No relational database initially.
- Hosting: Container Apps Consumption, web minimum zero/maximum one replica, single active revision. The user accepts cold starts.
- Scheduling: Container Apps Job, same image with a refresh argument, independent of website traffic. No separate Azure Function needed.
- Identity: managed identity for Blob access, with read access for the web app and write access for the job.
- Deployment: public source, container build and deployment workflow. Use a suitable public registry rather than automatically adding a paid Azure registry.

`Seances.Web` hosts the UI and startup. `Seances.Application` coordinates refresh workflows, `Seances.Data.Collection` owns UGC collection and source mapping, `Seances.Data.Storage` owns local snapshot persistence, and `Seances.Domain` owns internal models and scheduling rules. UGC response DTOs and versioned snapshot-file DTOs are mapped at those boundaries. This split keeps local development self-contained; cloud provisioning remains a separate deployment step.

## Blazor rendering behavior

Static SSR renders Razor components per request without a persistent interactive circuit. Query-bound filter values can produce bookmarkable schedule URLs. Start with normal GET forms; enhanced navigation can improve transitions without enabling Interactive Server globally.

Browser-saved preferences need a small JavaScript module. Static SSR does not invoke OnAfterRender, so use the documented static-SSR initialization/enhanced-navigation lifecycle rather than an interactive component's usual JS interop pattern. Enable an interactive component only if a specific interaction warrants it; C# event handlers such as @onclick do not work in static SSR alone.

## Collection

1. Fetch both cinema pages and parse their published date lists and request parameters.
2. Fetch each published cinema/date using the public HTML fragment request in [source discovery](source-discovery.md).
3. Start with serial requests, an approximately one-second gap, explicit timeouts, and bounded retries with backoff.
4. Extract actual screening buttons; upcoming-film blocks without buttons are not screenings.
5. Retain source screening/film IDs as strings, version labels verbatim, optional rooms, and source-reported finishes. Read runtime from the containing film block.
6. Enrich genuinely missing runtimes only, with conservative film matching. OMDb remains a candidate rather than a required initial dependency.
7. Validate all required data and requested cinema/date coverage. A malformed/error HTTP 200 response is not a valid empty schedule.
8. Publish only a complete validated snapshot, preserving the previous one on any fetch/parse failure. Avoid overlapping refreshes; parallelism one alone only limits each execution.

The source appears to omit already-started screenings. Full-day history is not promised until that behavior is established. Missing past screenings must not be mistaken for cancellations. Remove missing future screenings only after a successful reconciliation of the relevant cinema/date.

## Snapshot and time semantics

### Storage rationale

Blob Storage is the initial recommendation because the app reads a small complete schedule and the collector replaces it after validation. Local filtering over a cached snapshot needs no database queries, partition strategy, indexes, or multi-record reconciliation.

Cosmos DB is a valid alternative if independent record updates, queryable screening history, or server-side database queries become requirements. Its lifetime free tier currently includes 1000 RU/s and 25 GB, with one eligible account per subscription and explicit opt-in at account creation. That tier is for provisioned throughput, not serverless accounts; do not assume it is available in the user's subscription. The storage decision has not changed in response to the comparison question.

Sources: [Cosmos DB free tier](https://learn.microsoft.com/en-us/azure/cosmos-db/free-tier), [serverless billing](https://learn.microsoft.com/en-us/azure/cosmos-db/serverless).

### Data fields

- Manifest: schema version, actual successful collection timestamp, source, cinemas, and published dates per cinema.
- Film: source ID, title, optional runtime, runtime provenance, optional metadata match ID.
- Screening: source ID, film/cinema ID, service date, start, nullable room, source version label, optional format, source-reported finish, official booking URL.
- Store source facts separately from computed finishes and browser preferences.
- Display in Europe/Brussels. Preserve service dates, represent timestamps with offsets, handle finishes after midnight, and account for daylight-saving transitions.
- Finish priority: verified source-reported finish; otherwise runtime plus cinema allowance; otherwise explicitly assumed 180-minute runtime plus allowance.
- Never add an allowance twice to UGC's reported finish. Personal allowance overrides affect calculated estimates only.
- Assumed runtimes appear separately from screenings estimated to fit the availability window.

[Extracted sample facts](samples/ugc-2026-10-17.json) cover both cinemas, including midnight and missing-room cases.

## Refresh and serving

UGC's calendar message states that the full Wednesday–Tuesday programme becomes available on Tuesday at 09:00. The user accepts a main update after publication plus daily checks for subsequent changes.

Propose one scheduled run daily at 08:17 UTC (09:17 Brussels winter / 10:17 summer), with Tuesday's run serving as the main programme update. Initially fetch all published dates for simplicity; observe changes before optimizing which dates get rechecked. A daily check still needs source requests even if facts have not changed.

- Publish the true check timestamp after successful collection, including unchanged schedules.
- Retain the previous snapshot on failure and show freshness in the UI; propose a warning after 36 hours without a successful check.
- Web requests read a short-lived in-memory snapshot cache and never initiate UGC scraping.
- Keep the snapshot in Blob Storage, not container-local files, so it survives scale-to-zero and deployments.
- Bound job execution time, retry count, resource allocation, and log volume.

## Cost and alternatives

The budget target is €5/month. Estimate expected personal traffic and sustained activity separately before committing to deployment. Free grants and maximum replica count do not enforce a monetary ceiling. Budget alerts do not stop spending.

See [Azure hosting options](azure-hosting-options.md) for cost controls and comparisons:

- Container Apps: recommended candidate for a low-traffic, scale-to-zero .NET app, with variable costs and cold starts.
- App Service: an alternative for fixed-baseline compute; the selected tier must fit a regional quote.
- Static delivery on GitHub Pages plus an Actions collector: still viable if avoiding runtime hosting is preferred.
- Standalone Blazor WebAssembly can use static hosting; normal Razor Pages, MVC, and Blazor SSR require a .NET runtime host.

## Delivery sequence

1. Collect and normalize one date at both cinemas, with explicit failures and review against source data.
2. Render the real sample as a Blazor static SSR timetable and simplified list.
3. Add all published dates, filtering, saved preferences, overlap lanes, and freshness states.
4. Package the two entry modes into one image; prepare infrastructure and cost estimate for review.
5. Deploy only after the concrete hosting configuration and cost implications are agreed.

## Implementation follow-up

- Build the local prototype with a local JSON snapshot; add Blob Storage as the deployment data store rather than requiring Azure resources for local development.
- Measure web memory use and collection duration; choose the smallest adequate resource configuration.
- Validate the conditional Container Apps choice against a concrete low-usage cost estimate before provisioning.
- Prepare region, Blob, job schedule, logging, and cost-alert configuration for deployment review. Prefer West Europe as the initial costing reference; confirm its account-specific suitability during deployment preparation.

## References

- [Live source discovery](source-discovery.md)
- [.NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core)
- [Blazor render modes](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/render-modes?view=aspnetcore-10.0)
- [Static SSR JavaScript lifecycle](https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/static-server-rendering?view=aspnetcore-10.0)
- [AngleSharp](https://github.com/AngleSharp/AngleSharp)
- [Container Apps billing](https://learn.microsoft.com/en-us/azure/container-apps/billing)
- [Container Apps jobs](https://learn.microsoft.com/en-us/azure/container-apps/jobs)
- [Azure budget behavior](https://learn.microsoft.com/en-us/azure/cost-management-billing/costs/tutorial-acm-create-budgets)
