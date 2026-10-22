---
name: Séances
description: Clear cultural wayfinding for Brussels cinema schedules.
colors:
  canvas: "#f3f7f8"
  ink: "#142329"
  muted: "#465c65"
  paper: "#ffffff"
  line: "#bdcdd2"
  control-line: "#667f88"
  card-line: "#6d8790"
  row-alternate: "#eaf1f3"
  time-header: "#dae9ed"
  details-paper: "#f0f7f7"
  hover: "#e4f3ef"
  accent: "#006b64"
  accent-dark: "#004d48"
  accent-soft: "#d5eee8"
  on-accent: "#ffffff"
  focus: "#1646a0"
  notice-line: "#93652b"
  notice-paper: "#fff5e4"
  notice-ink: "#614117"
  dark-canvas: "#101c22"
  dark-ink: "#edf5f6"
  dark-muted: "#b5c8ce"
  dark-paper: "#16272e"
  dark-line: "#48616b"
  dark-control-line: "#819da7"
  dark-card-line: "#78949e"
  dark-row-alternate: "#1d323b"
  dark-time-header: "#263e47"
  dark-details-paper: "#1b3037"
  dark-hover: "#23453f"
  dark-accent: "#67d9c9"
  dark-accent-dark: "#a9f5e8"
  dark-accent-soft: "#234b43"
  dark-on-accent: "#092d29"
  dark-focus: "#f5ce72"
  dark-notice-line: "#cda15d"
  dark-notice-paper: "#382d1d"
  dark-notice-ink: "#f6d9aa"
  imdb-yellow: "#f5c518"
  imdb-black: "#000000"
typography:
  display:
    fontFamily: '"Source Sans 3", system-ui, sans-serif'
    fontSize: "clamp(2.25rem, 5vw, 3.5rem)"
    fontWeight: 700
    lineHeight: 1.08
    letterSpacing: "-.025em"
  display-compact:
    fontSize: "2rem"
    fontWeight: 700
    lineHeight: 1.08
  headline:
    fontSize: "1.75rem"
    fontWeight: 650
    lineHeight: 1.2
  headline-compact:
    fontSize: "1.25rem"
    fontWeight: 650
    lineHeight: 1.2
  detail-title:
    fontSize: "1.55rem"
    fontWeight: 600
    lineHeight: 1.25
  about-title:
    fontSize: "1.15rem"
    fontWeight: 600
    lineHeight: 1.3
  screening-title:
    fontSize: "1.0625rem"
    fontWeight: 650
    lineHeight: 1.25
  body:
    fontFamily: '"Source Sans 3", system-ui, sans-serif'
    fontSize: "1rem"
    fontWeight: 400
    lineHeight: 1.45
  intro:
    fontSize: "1.05rem"
    lineHeight: 1.5
  intro-compact:
    fontSize: ".9rem"
    lineHeight: 1.5
  freshness:
    fontSize: ".9rem"
    lineHeight: 1.45
  version:
    fontSize: ".92rem"
    lineHeight: 1.45
  screening-start:
    fontSize: ".9375rem"
    fontWeight: 700
    lineHeight: 1.45
  time:
    fontSize: ".875rem"
    fontWeight: 650
    lineHeight: 1.45
  metadata:
    fontSize: ".875rem"
    lineHeight: 1.45
  help:
    fontSize: ".875rem"
    lineHeight: 1.5
  detail-label:
    fontSize: ".875rem"
    fontWeight: 700
    lineHeight: 1.45
  language:
    fontSize: ".8125rem"
    fontWeight: 650
    lineHeight: 1.3
  attribution:
    fontSize: ".8125rem"
    lineHeight: 1.45
  imdb:
    fontFamily: 'Impact, "Arial Black", "Source Sans 3", system-ui, sans-serif'
    fontSize: ".75rem"
    fontWeight: 900
    lineHeight: 1.2
    letterSpacing: ".01em"
rounded:
  square: "0"
  resizer-grip: "1px"
  brand: ".2rem"
  language: ".25rem"
  action: ".35rem"
  control: ".45rem"
  screening: ".5rem"
  subpanel: ".65rem"
  panel: ".75rem"
spacing:
  metadata: ".2rem"
  xs: ".25rem"
  compact-gap: ".375rem"
  label-gap: ".4rem"
  sm: ".5rem"
  card-block: ".625rem"
  md: ".75rem"
  narrow-panel: ".8rem"
  lg: "1rem"
  notice-inline: "1.1rem"
  xl: "1.25rem"
  panel: "1.5rem"
  section: "2rem"
components:
  button-primary:
    backgroundColor: "{colors.accent}"
    textColor: "{colors.on-accent}"
    rounded: "{rounded.control}"
    padding: ".55rem .75rem"
    height: "2.75rem"
  button-primary-hover:
    backgroundColor: "{colors.accent-dark}"
  select:
    backgroundColor: "{colors.paper}"
    textColor: "{colors.ink}"
    rounded: "{rounded.control}"
    padding: ".55rem .75rem"
  date-navigation:
    rounded: "{rounded.action}"
    padding: ".45rem .65rem"
    textColor: "{colors.accent-dark}"
  language-tag:
    typography: "{typography.language}"
    rounded: "{rounded.language}"
    padding: ".0625rem .375rem"
  screening-card:
    backgroundColor: "{colors.paper}"
    textColor: "{colors.ink}"
    rounded: "{rounded.screening}"
    padding: ".625rem .75rem"
  screening-selected:
    backgroundColor: "{colors.accent}"
    textColor: "{colors.on-accent}"
  details:
    backgroundColor: "{colors.details-paper}"
    rounded: "{rounded.subpanel}"
    padding: "1.25rem"
  booking-link:
    backgroundColor: "{colors.accent}"
    textColor: "{colors.on-accent}"
    rounded: "{rounded.action}"
    padding: ".55rem .8rem"
---

# Design System: Séances

## Overview

**Creative North Star: "Clear cultural wayfinding"**

Séances is a clear, task-focused schedule reader. Mineral surfaces, strong control borders, teal active states, and open sans-serif typography help visitors scan cinema, service date, room, and start time without decorative cinema-poster treatment.

This approved system replaces the cream, burgundy, and serif identity while preserving the existing workspace. Appearance follows the device: pale mineral and white in light mode; blue-black mineral, pale text, and mint-teal in dark mode. Selection links a screening to its details without confusing selection with keyboard focus.

**Key Characteristics:**
- Mineral surfaces and teal emphasis, coordinated across device light and dark modes.
- Source Sans 3 throughout the interface, with an isolated IMDb brand badge exception.
- Flat, bordered grouping with a compact timeline and chronological phone list.
- Check-marked selection, dashed same-film treatment, and independently visible focus.

Implementation evidence: `src/Seances.Presentation.Web/wwwroot/app.css` and the Razor components under `Components/Schedule`, with font loading in `Components/App.razor`. Automated axe checks and palette contrast tests are evidence only; they do not establish full WCAG conformance. The product has no agreed conformance level.

## Colors

### Primary

Deep Teal (`accent`) drives actions and desktop selection; its deeper companion (`accent-dark`) emphasizes times, links, and hover. Pale Teal (`accent-soft`) marks mobile selection and other screenings of the selected film. `on-accent` is the paired foreground, not a universal white.

The `dark-` tokens replace the corresponding unprefixed roles under `prefers-color-scheme: dark`. Mint Teal takes over active states, pale mint emphasizes links and hover, and dark teal is the active foreground. The CSS role named `accent-dark` becomes lighter in dark mode: it is a semantic emphasis role, not a fixed lightness instruction.

The sidecar's representative tonal ramps are synthesized OKLCH preview strips, not implemented CSS scales or additional normative colors. The complete implemented palette is the frontmatter above; previews inherit the app's live CSS custom properties so light and dark treatments stay paired.

### Neutral

Mineral Canvas surrounds Paper reading surfaces. Ink and Muted distinguish primary and supporting text without opacity. Alternate Room and Time Header establish rows and the sticky axis; Details Paper distinguishes the reading section. Line groups surfaces, while Control Line and Card Line provide stronger interactive boundaries. Hover is a separate soft teal surface.

### Functional and brand exceptions

Focus is blue in light mode and amber in dark mode. Notice Line, Notice Paper, and Notice Ink form coordinated warning/empty-state roles in each mode. These are functional colors, not additional decorative accents. IMDb Yellow and IMDb Black are fixed brand colors in both modes, confined to the small badge; do not recolor the rest of the interface to match them.

**The Paired Appearance Rule.** Change each semantic role together with its dark counterpart and its foreground; never apply the light palette unchanged to dark surfaces.

## Typography

**Display and Body Font:** Source Sans 3, with system-ui and sans-serif fallbacks. The locally served variable face supports weights 200–900 and uses `font-display: swap`; only Source Sans 3 is preloaded. The old Source Serif 4 asset remains on disk but is unused and is not part of the active type system.

The frontmatter records every explicit type size rather than imposing an invented scale or minimum. Unspecified font families inherit Source Sans 3. The root line-height is 1.45 and the default body size is the browser's 1rem. No monospaced face is used; times use tabular numerals.

### Hierarchy

- **Display / display-compact:** page title, responsive by default and compact on desktop; both retain the display tracking.
- **Headline / headline-compact:** cinema title; the compact size also applies to notice headings via their own heading rules.
- **Detail-title / about-title:** film title and About heading, with the lighter 600 weight.
- **Screening-title:** semibold film names, clamped to two lines only in the timeline. Accessible names and details retain the full title.
- **Body:** selectors and detail values. Labels, room headings, mobile times, and mobile room names use 650; generic buttons and booking links use 700. Address and booking-unavailable copy use 1.5 line-height, synopsis uses 1.55 and a 65ch measure, and film links use 1.2.
- **Intro / intro-compact / freshness:** supporting page text and checked-at timestamp.
- **Screening-start / time:** card start time and axis time. Mobile start times use body size rather than the smaller desktop time size.
- **Metadata / help / detail-label / version:** screening metadata, calendar guidance, detail terms and time/room summary, and version text. Detail notes inherit the metadata sizing; the time/room summary is functional content, not a decorative eyebrow despite its legacy CSS name.
- **Language / attribution:** compact version badges and TMDB attribution. The former uses 650; the latter uses normal body weight.
- **IMDb:** a local brand-only exception using Impact, Arial Black, then the UI stack. This compact heavy wordmark is not a heading font.

Inheritance matters to advisory detectors: screening buttons inherit weight 700 from the generic button rule, so mobile screening metadata is 700 unless a child overrides it; metadata does not reset to body weight. Mobile times and room labels override to 650, as do film titles. Compact metadata, attribution, language badges, and the IMDb wordmark are intentionally recorded at their actual sizes, not silently normalized to a larger claimed floor.

**The One Reading Voice Rule.** Use Source Sans 3 for interface headings and content; the IMDb badge is a bounded brand exception, not permission to introduce another display family.

## Layout

At `min-width: 52.001rem`, the workspace fills `100dvh` with .75rem outer gutters and vertical padding. The page heading and supporting text share a compact row; cinema/address and checked-at text precede the labeled filters. The schedule panel uses 1rem padding and the timeline fills the remainder. Without a selection, it spans the workspace. Room names stick left; the axis sticks above; both dimensions scroll internally.

Selection adds a .75rem separator and a right-hand details column, initially 20.5rem, capped at 25% of the workspace. Its minimum is the smaller of 16rem and 20%. The divider supports dragging, arrow keys, Shift-modified larger steps, Home/End, and double-click reset; closing and reopening preserves its current page-local width. The panes align vertically and scroll independently. Closing, Escape, or reselecting dismisses details and restores the full timeline.

At `max-width: 52rem`, use a naturally scrolling column capped at 42rem, 1rem outer gutters, and 2rem top padding. Filters stack: each labeled field spans the two-column filter grid rather than sharing the date and room in one row. The chronological list replaces the timeline. Selected details occupy a fixed right-side drawer, inset 1rem and capped at 23rem wide; no divider is shown.

At `max-width: 26rem`, outer gutters tighten to .625rem, panel padding becomes .8rem, and the list time column becomes 4.4rem with a .65rem gap (normally 5.25rem and .9rem). At `max-width: 22rem`, the date toolbar becomes three columns: 2.75rem previous, flexible date, 2.75rem next. Today moves to its own second row, aligned right, avoiding clipping at 320px. The body minimum width is 20rem; this is not a claim of support below that width.

Spacing is compact and functional, predominantly quarter-rem increments with finer control/metadata adjustments. Timeline room lanes are 6.5rem high, the room column is 7.5rem, and the header is 3rem. Overlapping screenings use stacked lanes; duration-based widths retain horizontal scrolling when needed.

## Elevation & Depth

There are no box shadows. Borders, alternating surfaces, sticky layering, and an opaque details drawer establish depth. The mobile drawer sits at z-index 10 without an added scrim. Timeline headers use z-indices 2–4; focused cards rise locally to 1 so their indicator remains visible.

**The Flat Grouping Rule.** Use borders and tonal surfaces for grouping, not a new default shadow treatment.

## Shapes

The frontmatter radius vocabulary records all actual values: square mobile rows; the one-pixel divider grip; compact brand and language corners; action/poster corners; select/button/notice corners; screening cards; calendar/details subpanels; and the outer schedule panel. Do not collapse these to the stale single control radius. Borders are generally 1px. Same-film cards use dashed outlines; disabled date navigation uses a dashed boundary. Check marks and arrow/close icons are inline SVG, not glyph icons.

## Components

### Buttons

Primary buttons use paired accent colors and the control radius. Their minimum height is 2.75rem (the frontmatter height is the target, not a fixed CSS height). Booking links use the smaller action radius and their own inline padding. Close controls use a paper background, strong border, a 2.75rem square target, and a soft accent hover. Timeline cards deliberately override the generic button minimum height.

Global keyboard focus is a 3px outline with 3px offset. The timeline scroll region uses a -3px inset; timeline cards use -4px so overflow cannot clip focus. Selected timeline cards use `on-accent` for the outline, keeping it distinct on the active fill. Other controls retain the dedicated focus color.

### Inputs / Fields

Native cinema, date, and room selectors retain visible labels, paper surfaces, strong control borders, and the control radius. Hover changes the border to accent. Their minimum height is 2.75rem. Changes update the schedule and URL directly; there is no Apply button. Standard fields are 12rem wide on desktop; the date selector is also 12rem before responsive flex/grid sizing.

### Navigation

Previous and next are outlined icon links around the date selector. Today is a text link with the same target height; the current date adds a soft fill, accent border, and underline. Unavailable links remain visible as non-navigating disabled spans with muted text and dashed borders. Date navigation preserves the cinema and room query; cinema changes retain the chosen date, with unavailable room selection resolved by the schedule model.

### Chips

Language versions are compact outlined metadata tags, not interactive filters. Their small radius and fine padding keep them subordinate to the film title. Selected desktop tags inherit the active foreground and use a current-color border.

### Cards / Containers

Screening cards use strong card borders, paper surfaces, and compact padding. Hover uses the dedicated hover surface. Selected desktop cards use solid accent with paired text; mobile selection uses a soft fill and emphasis text. Both include an SVG check and `aria-pressed`. Other screenings of the same film use soft fill plus a dashed border and an explanatory accessible name, not the selected check. On mobile the dashed styling applies to the row's existing bottom border, not a newly enclosed card.

The details pane is a labeled `section` (`Screening details`, polite live updates), not an `aside`. It holds actual start/room information, a film heading, version, optional poster and external links, runtime/finish data, booking availability, and film information. Its close control is visibly separate from the content. Posters retain their 2:3 ratio, existing URLs, and alternative text; this redesign ships no raster changes.

Film links have outlined action styling and a 2.75rem minimum height. The IMDb badge alone uses the fixed brand palette and brand type. TMDB attribution remains compact supporting text with an underlined link. Notices and empty states use the coordinated notice palette and the control radius.

### Schedule

Timeline cards show start, language, and title; version/format metadata is hidden there but available in details. The narrow ordered list shows time, room, title, and compact version/format metadata. Full titles and estimated finishes remain in accessible names. The scroll area has visible focus and scroll padding to account for sticky headers. Only timeline-card color/border transitions are animated (.16s ease); reduced-motion preference removes them.

## Do's and Don'ts

### Do:
- **Do** maintain paired mineral/teal light and dark roles following the device preference.
- **Do** preserve labels, inline SVG selection checks, dashed same-film cues, and independent visible keyboard focus.
- **Do** preserve the wide timeline, narrow chronological list, and the smallest date-toolbar wrapping rule.
- **Do** keep Source Sans 3 as the interface voice and IMDb styling confined to its brand badge.

### Don't:
- **Don't** reintroduce cream/burgundy surfaces or the unused serif face as the current identity.
- **Don't** replace selection, focus, or same-film distinctions with color alone.
- **Don't** infer full WCAG conformance from axe results, contrast tests, or this document.
- **Don't** add default shadows or change snapshot/raster assets as part of documenting the system.
