# Séances — initial product plan

Status: product plan and initial implementation direction agreed. Live source discovery is recorded. Build the local prototype next; validate sizing and the €5/month cost target before deployment.

## Purpose

Answer: “What can I watch, where, and when?”

Organize cinema listings around available time and location. Start with UGC as the first data source, with room to add other cinemas later.

## Confirmed choices

- Designed around the user's personal needs, initially covering UGC cinemas in Brussels, and delivered as a public website.
- Primary task: find screenings that fit the user's available hours.
- The full screening must fit within the selected time window; start time alone is insufficient.
- Compare selected cinemas together on a shared timeline, grouped by cinema, with room information where available.
- Exact room information is optional and must not block the first version.
- Include all audio languages by default; no English-only restriction.
- Finish estimates include a default 20-minute allowance for initial trailers and advertising, configurable per cinema. The site maintainer controls shared defaults; visitors can save personal overrides in their browser without accounts.
- Resolve missing film runtimes through movie metadata, with OMDb proposed as an enrichment source. Metadata supplies film duration; it does not establish the actual finish of a particular screening.
- If runtime remains unknown after enrichment, draw a provisional three-hour film-duration block labelled "duration unknown · 3h assumed". This is not a maximum or a reliable finish estimate; keep these screenings outside results estimated to fit the availability window.
- The availability window covers time at the cinema only. Do not include travel time.
- Prioritize the laptop timetable and provide a simplified chronological list on phones.
- Paid hosting is acceptable with a €5/month budget target; prioritize predictable costs and protections against unexpected spending. No paid resources have been provisioned.
- Use Blazor with server-side rendering for the interface. Cold starts after inactivity are acceptable. Start with static SSR and ordinary filter requests; interactive component requirements remain to be demonstrated.
- Source code is public. Use Blazor SSR on Azure Container Apps Consumption, conditional on a credible low-usage estimate fitting the €5/month target and being cheaper than paid App Service. Use a scheduled Container Apps Job sharing the website's image and a JSON snapshot in Blob Storage. Prefer simple implementation and minimal operating cost; final resource sizing follows prototype measurements.
- Minimize collection requests and align refreshes with observed UGC publication behavior. Four daily full refreshes are not a requirement.
- Refresh policy agreed in principle: one main update after UGC publishes its programme, with daily checks for subsequent changes. Exact request scope and scheduling implementation remain open.
- Determine the selectable date range and refresh policy from source coverage and update behavior during data discovery; no seven-day commitment yet.
- First visits show today's full schedule across UGC Brussels. The availability filter is optional.
- Returning visits open on today and restore cinema selections and personal pre-show allowances from the browser.
- With an active availability window, hide screenings estimated not to fit by default. A "show outside my hours" toggle reveals them dimmed; unknown-duration screenings remain separately marked.
- Prefer UGC's displayed finish time when available and its meaning has been verified. Do not add another pre-show allowance to that finish. Otherwise calculate an estimated finish from runtime plus the applicable cinema allowance; personal allowance overrides affect these calculated estimates only.
- Offer dates actually published by UGC, including advance screenings, without implying complete schedules for intervening dates. The default remains today; missing today's data must be explicit.

## Main view

- Select a date, one or more cinemas, and a time window.
- Show a horizontal time axis with rows grouped by cinema and, where available, room.
- Display screenings as blocks positioned at their start time and sized by their duration.
- Put title, start time, language/subtitles, and format on each block as space permits.
- Open screening details for runtime, end-time confidence, room, and a link to the official booking page.
- Offer a chronological list on narrow screens and as an accessible alternative to the timetable.
- Preserve filter choices so returning to the schedule takes little effort.

Room availability is a discovery question. When absent, show screenings in overlapping lanes within each cinema and label the room as unavailable; never infer room numbers.

## First release

Include date and cinema selection, time filtering, the timetable, language/subtitle filters, official booking links, and a visible last-successful-update time.

Defer accounts, payments, seat availability, recommendations, and automatic multi-film itinerary planning. A timetable should still let someone visually compare consecutive screenings.

## Data discovery comes first

1. Inspect how UGC's public website loads cinemas, films, and showtimes.
2. Identify whether public structured responses provide stable screening IDs, cinema IDs, dates, languages, formats, room numbers, runtime, and booking links.
3. Determine the available schedule horizon and how changed or cancelled screenings appear.
4. Prefer structured public responses if usable; use HTML extraction if necessary, and browser automation only if rendering is required.
5. Check source access requirements before choosing the collection approach.
6. Capture a small real sample and document which fields are confirmed, missing, or derived.

Live discovery established repeatable access to a public HTML fragment for both Brussels cinemas, with IDs, rooms where available, runtimes, and displayed finishes. See [source discovery](docs/source-discovery.md) for requests, sample counts, publication timing, and remaining limitations, and [technical plan](docs/technical-plan.md) for the proposed implementation.

## Minimum data model

- Cinema: source ID, name, location, timezone.
- Film: source ID, title, runtime when available.
- Screening: source ID or stable composite key, cinema, film, nullable room, start timestamp, nullable source-reported finish timestamp, language, subtitles, format, booking URL. Keep any calculated finish and its basis separate from the source-reported finish.
- Provenance: source URL, fetched time, last successful refresh, and whether the end time is supplied or estimated.

Use Europe/Brussels for display, preserve dates across midnight, and retain distinct screenings of the same film. Runtime-based end times must be labelled estimates because trailers and other pre-show time may affect the actual finish. If runtime remains unknown, use the explicitly provisional three-hour fallback described above; never present its endpoint as a reliable finish time. Apply the cinema's pre-show allowance separately from the assumed film duration.

## Collection and serving

A small collector fetches listings, enriches missing runtimes from movie metadata, normalizes them into persistent storage, and supplies the schedule to the public website. Select the implementation stack and hosting after data discovery. Metadata-provider credentials must stay on the server.

For metadata matching, prefer a shared film identifier when available; otherwise verify title, year, and edition to avoid applying a different film's runtime. OMDb supports lookup by IMDb ID or title with an optional year and requires an API key. Its suitability and runtime coverage still need to be checked against real UGC listings.

Cache data so changing filters does not trigger new source requests. Make refresh frequency configurable, refresh before the displayed data becomes stale, and retain the last successful dataset during failures with a visible freshness message. Reconcile each successfully fetched cinema/date to remove screenings no longer listed; a failed fetch must not erase that date's data.

## Delivery sequence

1. **Prove data access:** collect a real sample for one cinema and date; record field availability and limitations.
2. **Agree on the timetable:** use that sample to settle row grouping, density, filtering, and behavior when rooms or end times are missing.
3. **Build a vertical slice:** one source, collection, storage, and a usable schedule with booking links.
4. **Expand coverage:** selected cinemas, upcoming dates, scheduled refreshes, and clear stale-data behavior.

## Acceptance criteria

- A person can select an evening and cinemas and see their options on a shared time axis.
- Screenings have clear location, start time, and language information where supplied.
- Overlapping screenings remain individually readable and selectable.
- Missing room or duration data is explicit.
- Estimated end times cannot be confused with confirmed finish times.
- Screening links lead to the corresponding official booking page where available.
- Empty schedules, collection failures, and stale data have distinct states.

## Technical discovery remaining

- Robust collection and parsing across dates, including complete versus partial coverage. Initial public request and sample fields are verified.
- Meaning of UGC's displayed finish times; the field is source-reported, not a guarantee of actual exit time.
- Source update behavior and refresh policy.
- Metadata provider selection and any associated cost.
- Low-cost hosting architecture that supports the proven collection method and refresh policy, with an explicit budget and realistic cost controls.

## Next milestone

The user confirmed the product plan and selected the simple shared-image hosting direction in [technical plan](docs/technical-plan.md). Repeatable fresh collection is demonstrated, with a factual sample saved. Build a local vertical slice, then measure memory and collection time to validate the cost estimate and deployment configuration. Use source film runtime before requesting external metadata.

## Sources inspected

- https://www.ugc.be/
- https://www.ugc.be/cinemas.html
- https://www.omdbapi.com/
