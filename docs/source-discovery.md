# UGC source discovery

Initial read-only investigation, 2026-10-16. Live public fragment retrieval was proven for both Brussels cinemas; implementation findings are recorded below.

## Use and permission status

**Permission status (checked 2026-10-18): no written approval recorded.** The project is local-only for now, with no public hosting or sharing with friends. The initial architecture's UGC workflow is to retrieve the currently published schedules for both Brussels cinemas through their public HTTP pages/fragments, parse all returned screenings, and store a complete local JSON snapshot for the web app. Collection may be repeated when schedules change. UGC's [reservation terms](https://www.ugc.be/conditions-Reservations.html) describe personal, non-commercial use through the site's normal features and restrict copying or reuse for other purposes.

The legal question is whether this repeated collection and local storage are permitted without prior written approval. The FPS Economy [exceptions summary](https://economie.fgov.be/fr/themes/propriete-intellectuelle/droits-de-propriete/droits-dauteur-et-droits/droit-des-bases-de-donnees/protection-des-bases-de/protection-par-le-droit-sui/exceptions-au-droit-exclusif) lists private extraction. Belgian CDE Article XI.310's private-use exception for extracting a substantial part is specifically worded for a **non-electronic** database; UGC schedules are online. Article XI.311 separately addresses a lawful user's extraction or reuse of non-substantial parts, subject to limits including normal exploitation and the producer's legitimate interests. Substantiality, repetition, and the site terms need to be considered for the actual workflow. This is a preliminary risk summary, not a legal opinion.

As a cautious project policy, pause live UGC collection and storage until UGC confirms the specific workflow in writing or an authorized feed is available. The `--allow-live` flag and a robots.txt check are operational safeguards, not permission. Any request should describe the repeated HTTP retrieval, schedule parsing, local JSON storage, and exclusive local use, and ask UGC to confirm in writing through its [contact form](https://www.ugc.be/contact.html).

Until this gate is resolved, use synthetic fixtures for development and UGC's own site for schedule consultation. The live retrieval findings below record technical behavior only; they do not establish authorization to reuse the data.

## Live findings (supersede earlier access uncertainties below)

Direct HTTPS retrieval outside the network sandbox succeeded without login or browser automation. The cinema page embeds this GET request:

```text
https://www.ugc.be/showingsCinemaAjaxAction!getShowingsForCinemaPage.action
  ?cinemaId=2&date=2026-10-17&page=30007
```

- `cinemaId=2`: De Brouckère; `cinemaId=1`: Toison d'Or.
- `date`: ISO calendar date. Both 2026-10-16 and 2026-10-17 returned matching screening dates.
- `page=30007`: value used by the cinema page; retain as an adapter parameter rather than assuming a public API contract.
- Response is an HTML fragment, not JSON. No JavaScript execution was required.
- `button[data-showing]` contains the screening ID, film ID/title, cinema ID, date, time, and version in data attributes. HTML parsers lowercase attribute names.
- Child classes `screening-time-end` and `screening-room` supply finish and optional room. Film blocks `bloc-showing-film-<id>` contain runtime and film links. The button supplies its reservation URL.
- Upcoming films without screening buttons also appear; do not turn every film block into a screening.

Observed for 2026-10-17:

| Cinema | Unique screenings | Displayed finish present | Runtime present | Room missing |
| --- | ---: | ---: | ---: | ---: |
| De Brouckère | 60 | 60 | 60 | 0 |
| Toison d'Or | 71 | 71 | 71 | 4 |

A repeated De Brouckère request returned the same 60 screening IDs. This establishes repeatability for this sample, not completeness across all dates. See [extracted sample facts](samples/ugc-2026-10-17.json), including missing-room and midnight cases. Indexed content differed from freshly fetched content in places; production must use fresh requests rather than search results.

The page embeds published dates in `initCalendarDate([...])` and `nav_date_YYYY-MM-DD` elements. Its calendar message states that the full coming Wednesday–Tuesday programme is available each Tuesday at 09:00. This is source-stated publication timing; updates and cancellations may occur at other times. Each cinema has its own available dates.

Today's fragment returned 20 future screenings versus 60 for tomorrow, with today's starts from 19:30 onward at the time of retrieval. This is consistent with past screenings being omitted, but the exact cutoff remains unverified. The product should describe today's view as upcoming screenings unless full-day retrieval is established.

The inspected examples' finish spans equal film runtime plus 20 minutes. Treat displayed finishes as UGC's reported estimates; the sample does not establish an actual exit-time guarantee or a universal rule for special events.

`https://www.ugc.be/robots.txt` redirects and was read following the redirect. It lists `/AjaxAction!` as disallowed; the discovered path starts `/showingsCinemaAjaxAction!`, a different literal prefix. It also lists unrelated path restrictions. Recheck source directives before operating the production collector; robots directives are not an API stability commitment.

## Open source behavior questions

- Confirm coverage and explicit-empty behavior across more cinema/date combinations.
- Establish the past-screening cutoff; do not interpret missing past screenings as cancellations.
- Verify language-code meanings before deriving spoken-language or subtitle fields; retain source labels meanwhile.
- Observe schedule changes over later checks to establish cancellation and update behavior.
- Check whether any runtime gaps justify metadata enrichment; none were found in the inspected sample.

## Earlier indexed investigation

## Evidence

- The cinema index's search-indexed content lists UGC De Brouckère and UGC Toison d'Or in Brussels: https://www.ugc.be/cinemas.html
- Direct retrieval of https://www.ugc.be/bioscoop-ugc-de-brouckere.html exposes a date selector with near-term dates and scattered later dates, through December in the retrieved sample. This does not prove complete daily schedule coverage through December.
- Search-indexed content for that same cinema URL includes film runtimes, screening start and displayed finish times, rooms, and version labels such as VOstBIL and VFstNL. Direct page retrieval returned a shell without those screening details, so reliable collector access is still unverified.
- Example in indexed content: MALFAISANTE has a 1h34 runtime and a 16:45–18:39 screening in Salle 2. The displayed span exceeds runtime by 20 minutes. This supports investigating an existing pre-show allowance; it does not prove all UGC finish times use the same rule.
- Public booking pages have a shape such as https://www.ugc.be/reservationSeances.html?id=320021162448 . The mapping from collected screenings to booking URLs remains to be verified.

## Implications to discuss

- Prefer a source-supplied finish if its meaning is established. Avoid adding a pre-show allowance twice.
- Source film runtime may reduce the need for OMDb enrichment.
- Room information appears promising but remains optional until dependable retrieval is established.
- Let actual published dates drive date selection rather than assuming a fixed seven-day horizon.

## Earlier unresolved questions (see live findings above)

- Structured endpoint, request parameters, pagination, and stable IDs.
- Reproducible fresh extraction of screening details across both cinemas and dates.
- Full ordinary-program schedule horizon versus isolated advance screenings.
- Publication cadence, changes/cancellations, and an appropriate refresh frequency.
- Meaning of displayed finish times and behavior of personal allowance overrides when a finish is supplied.
- Metadata coverage, film matching, and access requirements.

Search-indexed evidence is discovery evidence only; do not use search results as the production showtime feed.

## Implementation status (2026-10-18)

The integration fixtures are synthetic parser cases with their source URL, service date, and fixture provenance in adjacent JSON files. They cover both cinemas, duplicate IDs, an unavailable room, an upcoming film without a screening button, a sold-out label, an event, and a finish after midnight.

During live-refresh troubleshooting on 2026-10-17, the current cinema index contained an empty `#nav-cinemas` container and loaded its links from `/cinemasAjaxAction!getCinemasList.action`. The returned cinema cards linked to `cinema-ugc-de-brouckere.html` and `cinema-ugc-toison-d-or.html`. Current screening fragments use `data-filmId`, `data-cinemaId`, `data-seanceHour`, and `data-seanceDate` (day/month/year); finish text includes a `(fin HH:mm)` prefix, and booking links appear in `onclick` as a relative path. Toison d’Or's fragment for 2026-10-17 had film cards with “Prochaine séance le …” text and no showing buttons, indicating no screenings for that service date.

One bounded live refresh for 2026-10-17 completed successfully after the collector was updated for these formats. The same date was no longer published at De Brouckère when requested for 2026-10-16. Current `robots.txt` was read before the refresh; it disallows `/AjaxAction!`, while the observed cinema-list and screening paths begin `/cinemasAjaxAction!` and `/showingsCinemaAjaxAction!` respectively. Recheck current directives and source terms before future live collection. Raw live responses were inspected locally and are not committed; deterministic tests continue to use synthetic fixtures.

Later on 2026-10-17, UGC's calendar no longer offered 2026-10-17 at De Brouckère, while both cinemas published 2026-10-19. A live CLI collection for 2026-10-19 returned 60 De Brouckère and 71 Toison d’Or screenings. A full-calendar run collected 34 cinema-date responses and 485 screenings, and a no-date refresh published the complete snapshot. The CLI extracts schedule data to stdout or writes JSON/CSV to an explicit `--output` path; the web host independently reads `SEANCES_SNAPSHOT_PATH` or its local application-data default. Configure the web host to read the CLI JSON output only when desired.
