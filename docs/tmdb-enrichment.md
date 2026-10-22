# Film enrichment

Scope confirmed during the 2026-10-21 interview. This supersedes the external-metadata deferral in [collection improvements](collection-improvements.md).

## Goal

Enrich collected films for a local, noncommercial app. UGC remains the source of schedules, screenings, and most film details. TMDB adds only what UGC lacks: a YouTube trailer, an English synopsis, a missing original title, an IMDb ID, and a TMDB rating.

## UGC film details

A live check of one UGC film page (`film.html?id=9860`) on 2026-10-21 found a schema.org `Movie` JSON-LD block containing:

- `datePublished` (Belgian release date), `duration` (ISO 8601, e.g. `PT2H52M`), `genre`, `director`, `actor`, `description` (French synopsis), and `image` (poster URL).

The page also shows Belgian age-rating and content pictograms. It has no IMDb ID or YouTube link; its trailer loads into UGC's own player. Search-indexed copies of other film pages show the same fields.

- Read the JSON-LD block from each film's detail page, fetching it once per new film.
- Display UGC's release date, runtime, genres, director, cast, and poster. Record UGC as runtime provenance.
- Missing JSON-LD or fields leave values unknown and produce a warning; schedules are preserved.

## TMDB source and rights

- TMDB is free for noncommercial use with attribution. It is not a blanket copyright license for images or trailers.
- Link to media; do not download or redistribute it.
- Display the TMDB logo and “This product uses the TMDB API but is not endorsed or certified by TMDB.” in an About or Credits area.
- Refresh TMDB data at least every six months. Reassess terms before any public or commercial use.

References: [TMDB FAQ](https://developer.themoviedb.org/docs/faq), [search](https://developer.themoviedb.org/reference/search-movie), [details](https://developer.themoviedb.org/reference/movie-details), [videos](https://developer.themoviedb.org/reference/movie-videos), [rate limits](https://developer.themoviedb.org/docs/rate-limiting).

## Matching

- Search TMDB with the original title when available, otherwise the UGC title, normalized for case, accents, punctuation, and format suffixes such as 3D or VOST. Filter by UGC's release year.
- Request candidate details with `append_to_response=credits,videos,external_ids`.
- Confirm a candidate when its director matches UGC's director, or its runtime is within 5 minutes of UGC's runtime.
- Leave the film without TMDB data when no candidate is confirmed or several are. Never select the first search result by ranking.
- A local corrections file maps UGC film IDs to TMDB IDs and overrides matching.
- Store each film's TMDB ID and reuse it in later collections instead of searching again.

## TMDB data

- Synopsis: English; otherwise French; otherwise UGC's French synopsis.
- Trailer: one official YouTube trailer link, preferring English, then French, then the original language. No embedded player.
- Original title, only when UGC has none.
- IMDb ID, linking to the IMDb page.
- TMDB rating and vote count.
- Poster: UGC first; TMDB's poster only when UGC has none.

## Workflow and failures

- Enrich during collection only; web requests read the saved snapshot and never call UGC or TMDB.
- Read the TMDB credential from local configuration or user secrets, never source control. Without one, skip TMDB enrichment with a warning.
- When a credential is configured for live collection, validate its format and confirm authentication with TMDB before making any UGC requests. If validation fails or authentication cannot be confirmed, fail collection without writing a snapshot. Fixture collection does not contact TMDB.
- Failed requests, rate limits (HTTP 429), or no confirmed match produce warnings and preserve schedule data.

## Implementation checklist

- Parse film-page JSON-LD in Collection under `Ugc/`, and add a synthetic fixture modelled on the observed structure.
- Add a TMDB client, DTOs, matching, and explicit mapping in Collection under `Tmdb/`.
- Add optional film fields to `Film`, storage documents, mappings, and validation, keeping existing snapshots readable.
- Orchestrate UGC detail collection and TMDB enrichment in Application after films are merged.
- Show poster, trailer link, synopsis, genres, release date, runtime, director, cast, IMDb link, and rating in the selected-screening pane, plus TMDB attribution.
- Test with synthetic fixtures: JSON-LD parsing and gaps, director and runtime confirmation, ambiguous and missing matches, corrections, saved-match reuse, language fallbacks, trailer selection, failures, persistence compatibility, and UI rendering.
