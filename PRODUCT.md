# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

Brussels cinema-goers checking local cinema schedules to choose a film and showing.

## Product Purpose

Séances presents cinema showtimes by service date and cinema so visitors can scan films, rooms, and start times.

## Positioning

The web app serves a locally stored schedule snapshot for UGC De Brouckère and UGC Toison d’Or. It reads the snapshot; schedule collection is handled separately by the CLI.

## Operating Context

Visitors browse the available service dates for the two Brussels cinemas. The schedule is read-only in the web app.

## Capabilities and Constraints

- The web app reads a versioned JSON schedule snapshot from a local file.
- The snapshot contains cinema, film, published-date, and screening information. Room information may be unavailable.
- The app must support choosing a cinema and date and scanning showings by time and room.
- The user wants a modern, accessible calendar view. The specific accessibility conformance level remains undecided.
- Live schedule collection requires written approval and is outside this UI change.

## Evidence on Hand

- `data.json` contains a snapshot for both UGC cinemas.
- `tests/Seances.Presentation.Web.Tests/Integration/Fixtures` contains synthetic HTML fixtures; they are parser cases, not captured UGC responses.

## Product Principles

- Make it easy to find a film by cinema, service date, room, and start time.
- Keep the selected cinema’s schedule together and make cinema changes explicit.
- Show missing schedule and room data plainly rather than implying it is known.
- Keep the schedule usable with a keyboard and on a phone.
