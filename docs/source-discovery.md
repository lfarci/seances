# UGC source discovery

Read-only investigation, 2026-10-16. Live public fragment retrieval is now proven for both Brussels cinemas; a production collector has not been implemented.

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

## Remaining technical work

- Implement and validate a parser across ordinary, event, sold-out, empty, and malformed responses.
- Check date coverage across both cinemas and distinguish explicit empty schedules from failures.
- Establish past-screening cutoff behavior; do not interpret missing past screenings as cancellations.
- Verify language-code meanings before deriving spoken language/subtitle fields; retain source labels meanwhile.
- Observe later refreshes to establish change frequency and cancellation behavior.
- Check OMDb matching only for genuine gaps; no external metadata was needed for sampled screenings.

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

## Implementation status (2026-10-17)

The checkout did not contain captured raw fragment responses. The integration fixtures are synthetic parser cases with their source URL, service date, and fixture provenance in adjacent JSON files. They cover both cinemas, duplicate IDs, an unavailable room, an upcoming film without a screening button, a sold-out label, an event, and a finish after midnight. They do not establish that every current UGC attribute or selector matches the live response.

The current `robots.txt` could not be read from the implementation environment. No live UGC collection was performed. The refresh command therefore requires `--allow-live`; recheck current directives and source terms before using that option, then record one bounded check and retain the actual response with its fetch provenance.
