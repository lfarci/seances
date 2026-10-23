---
name: Séances
description: Clear cultural wayfinding for Brussels cinema schedules.
colors:
  canvas: "#f3f7f8"
  canvas-glow: "rgb(0 107 100 / .22)"
  canvas-glow-secondary: "rgb(0 107 100 / .1)"
  glass-panel: "rgb(248 250 252 / .8)"
  glass-details: "rgb(232 242 244 / .9)"
  glass-sheen: "rgb(255 255 255 / .8)"
  control-sheen: "rgb(255 255 255 / .65)"
  action-sheen: "rgb(255 255 255 / .06)"
  ink: "#142329"
  muted: "#354e5a"
  paper: "#ffffff"
  line: "#97adb8"
  control-line: "#546e79"
  card-line: "#5d7782"
  scrollbar-thumb: "#607d88"
  row-alternate: "#dce8ed"
  time-header: "#cadfe5"
  room-header: "#e4edf1"
  room-header-alternate: "#d4e2e8"
  details-paper: "#e8f2f4"
  hover: "#e4f3ef"
  accent: "#006b64"
  accent-dark: "#004d48"
  accent-soft: "#d5eee8"
  on-accent: "#ffffff"
  focus: "#1646a0"
  notice-line: "#93652b"
  notice-paper: "#fff5e4"
  notice-ink: "#614117"
  dark-canvas: "#1b1d29"
  dark-canvas-glow: "rgb(103 217 201 / .24)"
  dark-canvas-glow-secondary: "rgb(103 217 201 / .1)"
  dark-glass-panel: "rgb(34 37 51 / .8)"
  dark-glass-details: "rgb(32 41 54 / .9)"
  dark-glass-sheen: "rgb(255 255 255 / .12)"
  dark-control-sheen: "rgb(255 255 255 / .075)"
  dark-action-sheen: "rgb(255 255 255 / .22)"
  dark-ink: "#f4fafb"
  dark-muted: "#c3d5dc"
  dark-paper: "#303847"
  dark-line: "#6a7d8d"
  dark-control-line: "#9cb6c1"
  dark-card-line: "#8faab6"
  dark-scrollbar-thumb: "#91a7b4"
  dark-row-alternate: "#1e2532"
  dark-time-header: "#354255"
  dark-room-header: "#354357"
  dark-room-header-alternate: "#2b3749"
  dark-details-paper: "#202936"
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
    fontSize: "1rem"
    fontWeight: 700
    lineHeight: 1.45
  room-heading:
    fontSize: "1rem"
    fontWeight: 700
    lineHeight: 1.45
  axis-caption:
    fontSize: ".875rem"
    fontWeight: 600
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
  poster-resizer: ".35rem"
  notice: ".45rem"
  scrollbar-thumb: "999px"
  control: "1rem"
  screening: ".875rem"
  subpanel: "1.5rem"
spacing:
  metadata: ".2rem"
  xs: ".25rem"
  compact-gap: ".375rem"
  label-gap: ".4rem"
  sm: ".5rem"
  card-block: ".625rem"
  md: ".75rem"
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
    rounded: "{rounded.control}"
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
    rounded: "{rounded.control}"
    padding: ".55rem .8rem"
---

# Design System: Séances

## Overview

**Creative North Star: "Clear cultural wayfinding"**

Séances is a clear, task-focused schedule reader. Mineral surfaces, strong control borders, teal active states, and open sans-serif typography help visitors scan cinema, service date, room, and start time without decorative cinema-poster treatment.

This approved system replaces the cream, burgundy, and serif identity while preserving the existing workspace. Appearance defaults to the device preference and offers a persistent Theme choice of System, Light, or Dark: pale mineral and white in light mode; layered dark slate, pale text, and mint-teal in dark mode. Selection links a screening to its details without confusing selection with keyboard focus.

The OpenCode Desktop reference and the request for Apple Liquid Glass-like gloss inform the surface treatment, not a new identity: paired canvas glows, directional highlights, beveled edges, and cast shadows give the interface tangible relief while retaining teal and device themes. No noise or texture is used. Stronger lanes, a raised room rail, and a substantial time ruler make room and start time easier to scan. Headings and filters sit directly on the canvas; calendar and details are individual glossy surfaces, without a surrounding card. Opaque fallbacks preserve reading surfaces when blur is unsupported or transparency or contrast preferences require them.

**Key Characteristics:**
- Mineral surfaces and teal emphasis, coordinated across light and dark modes with explicit control and live device alignment in System mode.
- Source Sans 3 throughout the interface, with an isolated IMDb brand badge exception.
- Individual rounded glass calendar and details surfaces, inset timeline lanes, raised room/time headings, glossy rounded controls, and a chronological phone list.
- Check-marked selection, dashed same-film treatment, and independently visible focus.

Implementation evidence: `src/Seances.Presentation.Web/wwwroot/app.css`, `wwwroot/themes/dark.css`, `wwwroot/js/theme.js`, and the Razor components under `Components/Schedule` and `Components/Appearance`, with font and theme loading in `Components/App.razor`. Automated axe checks and palette contrast tests are evidence only; they do not establish full WCAG conformance. The product has no agreed conformance level.

## Colors

### Primary

Deep Teal (`accent`) drives actions and desktop selection; its deeper companion (`accent-dark`) emphasizes times, links, and hover. Pale Teal (`accent-soft`) marks mobile selection and other screenings of the selected film. `on-accent` is the paired foreground, not a universal white.

The `dark-` documentation tokens describe the corresponding unprefixed roles in the separately media-gated `themes/dark.css`; its existing dark token block moved intact from `app.css`, with only Scrollbar Thumb added in each palette. System mode gates the stylesheet with `prefers-color-scheme: dark`; explicit Dark uses `all`, and Light uses `not all`. The base stylesheet declares native `color-scheme: light`, and the dark stylesheet declares `dark`. Mint Teal takes over active states, pale mint emphasizes links and hover, and dark teal is the active foreground. The CSS role named `accent-dark` becomes lighter in dark mode: it is a semantic emphasis role, not a fixed lightness instruction.

The sidecar's representative tonal ramps are synthesized OKLCH preview strips, not implemented CSS scales or additional normative colors. The complete implemented palette is the frontmatter above; previews inherit the app's live CSS custom properties so light and dark treatments stay paired.

### Neutral

Mineral Canvas surrounds Paper reading surfaces without an outer schedule card. Dark mode uses slate layers rather than blue-black mineral. Ink and Muted distinguish primary and supporting text without opacity; dark metadata is brighter. Row Alternate differentiates screening lanes; Time Header distinguishes the sticky time ruler. Room Header and Room Header Alternate independently alternate the raised sticky room rail, rather than sharing lane backgrounds. Details Paper distinguishes the reading section. Line groups surfaces, while Control Line and Card Line strengthen interactive boundaries. Hover is a separate soft teal surface; teal accents retain their existing identity.

Canvas Glow and Canvas Glow Secondary provide paired ambient teal washes. Glass Panel fills the calendar shell, not the semantic schedule container; Glass Details is 90% opaque. Both sit beneath Glass Sheen, with Paper and Details Paper as opaque fallbacks. Control Sheen and Action Sheen provide directional gloss on controls and cards over opaque base colors. All color roles have paired dark replacements.

Scrollbar Thumb is a paired muted mineral role shared by calendar and details, distinct from control borders. Tracks and corners remain transparent; supported thumb hover uses Accent. Forced colors retain native standards-based colors and use CanvasText/Highlight for WebKit thumb/hover styling.

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
- **Body:** selectors and detail values. Labels, mobile times, and mobile room names use 650; generic buttons and booking links use 700. Address and booking-unavailable copy use 1.5 line-height, synopsis uses 1.55 and a 65ch measure, and film links use 1.2.
- **Intro / intro-compact / freshness:** supporting page text and checked-at timestamp.
- **Screening-start / time:** card start time and hourly axis labels. Axis labels use Ink, body size, and weight 700; mobile start times use body size and weight 650.
- **Room-heading / axis-caption:** weight-700 room names and corner Room label; the stacked Start time caption uses Muted at .875rem and weight 600.
- **Metadata / help / detail-label / version:** screening metadata, calendar guidance, detail terms and time/room summary, and version text. Detail notes inherit the metadata sizing; the time/room summary is functional content, not a decorative eyebrow despite its legacy CSS name.
- **Language / attribution:** compact version badges and TMDB attribution. The former uses 650; the latter uses normal body weight.
- **IMDb:** a local brand-only exception using Impact, Arial Black, then the UI stack. This compact heavy wordmark is not a heading font.

Inheritance matters to advisory detectors: screening buttons inherit weight 700 from the generic button rule, so mobile screening metadata is 700 unless a child overrides it; metadata does not reset to body weight. Mobile times and room labels override to 650, as do film titles. Compact metadata, attribution, language badges, and the IMDb wordmark are intentionally recorded at their actual sizes, not silently normalized to a larger claimed floor.

**The One Reading Voice Rule.** Use Source Sans 3 for interface headings and content; the IMDb badge is a bounded brand exception, not permission to introduce another display family.

## Layout

At `min-width: 52.001rem`, the workspace fills `100dvh` with .75rem outer gutters and vertical padding. The page heading and supporting text share a compact row; cinema/address and checked-at text precede the labeled filters directly on the canvas. The semantic `.schedule` container has `min-width: 0` and desktop flex layout, but no border, background, shadow, padding, or filter at any breakpoint. The timeline fills the remainder. Without a selection, it spans the workspace. Room names stick left; the axis sticks above; both dimensions scroll internally.

The page header also holds the Theme picker, aligned to the inline end with a .625rem label/control gap, a .875rem label, and an 8.5rem-wide native select at 1rem. The desktop heading row wraps rather than clipping the control. Theme remains available when the schedule snapshot is missing.

Selection adds a .75rem separator and a right-hand details column, initially 20.5rem, capped at 25% of the workspace. Its minimum is the smaller of 16rem and 20%. The divider supports dragging, arrow keys, Shift-modified larger steps, Home/End, and double-click reset; closing and reopening preserves its current page-local width. The panes align vertically and scroll independently. Closing, Escape, or reselecting dismisses details and restores the full timeline.

At `max-width: 52rem`, use a naturally scrolling column capped at 42rem, 1rem outer gutters, and 2rem top padding. Filters stack: each labeled field spans the two-column filter grid rather than sharing the date and room in one row. The chronological list replaces the timeline. Selected details occupy a fixed right-side drawer, inset 1rem and capped at 23rem wide; no divider is shown.

At `max-width: 26rem`, outer gutters tighten to .625rem and the list time column becomes 4.4rem with a .65rem gap (normally 5.25rem and .9rem). At `max-width: 22rem`, the date toolbar becomes three columns: 2.75rem previous, flexible date, 2.75rem next. Today moves to its own second row, aligned right, avoiding clipping at 320px. The body has no fixed minimum width, so the layout can shrink to the usable viewport width when a native scrollbar occupies part of a narrow screen.

Spacing is compact and functional, predominantly quarter-rem increments with finer control/metadata adjustments. Timeline room lanes are 6.5rem high, the room column is 7.5rem, and the header is 4rem. The sticky corner stacks Room above Start time. The ruler sticks to the top and the raised room rail sticks left, with independently alternating rail backgrounds. Hourly labels have .75rem bottom ticks; unlabeled half-hour ticks are shorter (.375rem). Scroll padding is 4.25rem .5rem .5rem 8rem to clear the sticky axes. Overlapping screenings use stacked lanes; duration-based widths retain horizontal scrolling when needed.

## Elevation & Depth

Borders and tonal layering remain structural; directional highlights, bevels, and cast shadows create relief, not texture. The body uses two static radial washes: `--canvas-glow` at 90% 0 fading to transparent at 55%, and `--canvas-glow-secondary` at 0 85% fading at 50%. Calendar and details combine top, side, and bottom inset reflections (`--surface-edge`) with paired `--panel-shadow`; selected desktop cards and the booking action combine reflected edges with `--selection-glow`. Selects, date navigation, and close controls combine `--surface-edge` with `--control-shadow` and a 180-degree Control Sheen fading at 65%. Unselected timeline cards combine reflected edges with `--card-shadow` and 160-degree Control Sheen fading at 55%; selected cards/booking use Action Sheen fading at 60%. The raised ruler uses `--header-shadow` and 180-degree sheen fading at 75%; the room rail uses `--rail-shadow` and 160-degree sheen fading at 70%. Screening lanes use recessed `--lane-shadow`. Exact light/dark shadow values are recorded in the sidecar. No new motion is introduced, and glow never replaces focus or state indicators.

Opaque Paper and Details Paper are the baseline. Only when standard or WebKit backdrop filtering is supported do calendar and details receive `blur(24px) saturate(125%)` and a 135-degree Glass Sheen gradient fading to transparent at 30%, over Glass Panel or Glass Details. Calendar lanes, timeline cards, and controls retain opaque base colors. The schedule ancestor is never filtered, so the fixed mobile drawer needs no special ancestor-filter override. It retains supported blur and sits at z-index 10 without an added scrim. Timeline headers use z-indices 2–4; focused cards rise locally to 1 so their indicator remains visible.

At the end of the stylesheet, `prefers-reduced-transparency: reduce`, `prefers-contrast: more`, or `forced-colors: active` remove body washes, backdrop filtering, gradients, and decorative shadows. Restore solid Paper and Details Paper backgrounds while retaining contrasting room-rail backgrounds, ruler ticks, structural borders, solid selected colors, visible focus, selection checks, and dashed same-film boundaries.

**The Bounded Glass Rule.** Confine blurred glass shells and panel shadows to calendar and details, with no outer schedule card; create relief through directional highlights, cast shadows, and beveled edges, never noise or texture. Keep selection glow on selected desktop cards and booking only; preserve opaque reading bases, solid preference fallbacks, contrasting room rails, ruler ticks, mobile drawer positioning, borders, visible focus, selection checks, and dashed same-film cues.

## Shapes

The frontmatter radius vocabulary records all actual values. The three shared CSS tokens are `--radius-section` (calendar/details, 1.5rem), `--radius-card` (screenings, .875rem), and `--radius-control` (native controls, date navigation, booking, and film links, 1rem), mapped to subpanel, screening, and control respectively. There is no outer schedule radius token. Square mobile rows, the one-pixel divider grip, compact brand/language corners, poster/resizer corners, and notice corners retain their original sizes. Borders are generally 1px. Same-film cards use dashed outlines; disabled date navigation uses a dashed boundary. Check marks and arrow/close icons are inline SVG, not glyph icons.

## Components

### Buttons

Primary buttons use paired accent colors and the control radius. Their minimum height is 2.75rem (the frontmatter height is the target, not a fixed CSS height). Booking links share the control radius, use their own inline padding, and carry selection glow. Close controls use a paper background, strong border, a 2.75rem square target, and a soft accent hover. Timeline cards deliberately override the generic button minimum height.

Global keyboard focus is a 3px outline with 3px offset. The timeline scroll region uses a -3px inset; timeline cards use -4px so overflow cannot clip focus. Selected timeline cards use `on-accent` for the outline, keeping it distinct on the active fill. Other controls retain the dedicated focus color.

### Inputs / Fields

Native cinema, date, and room selectors retain visible labels, paper surfaces, strong control borders, and the control radius. Hover changes the border to accent. Their minimum height is 2.75rem. Changes update the schedule and URL directly; there is no Apply button. Standard fields are 12rem wide on desktop; the date selector is also 12rem before responsive flex/grid sizing.

The native `Components/Appearance/ThemePicker.razor` selector is labeled Theme, with System, Light, and Dark options. System is the default and follows live OS appearance changes; explicit Light or Dark overrides the OS. The early classic `js/theme.js` script in the global head reads `localStorage` key `seances.theme` before paint and sets the `dark-theme` link media, root native color scheme, and theme-color metadata for browser chrome. It synchronizes the picker on DOM ready and first interactive render. Changes persist locally when storage is available; blocked storage still permits an in-page choice. Both themes and the picker are available without a snapshot. Without JavaScript, the stylesheet's native media query follows the OS; explicit selection and persistence require JavaScript.

### Navigation

Previous and next are outlined icon links around the date selector. Today is a text link with the same target height; the current date adds a soft fill, accent border, and underline. Unavailable links remain visible as non-navigating disabled spans with muted text and dashed borders. Date navigation preserves the cinema and room query; cinema changes retain the chosen date, with unavailable room selection resolved by the schedule model.

### Chips

Language versions are compact outlined metadata tags, not interactive filters. Their small radius and fine padding keep them subordinate to the film title. Selected desktop tags inherit the active foreground and use a current-color border.

### Cards / Containers

Screening cards use strong card borders, paper surfaces, compact padding, directional sheen, reflected edges, and a small cast shadow above recessed lanes. Hover retains sheen over the dedicated hover surface. Selected desktop cards use an opaque accent base with paired text, Action Sheen, reflected edges, and selection glow; mobile selection retains a soft fill and emphasis text without glow. Both include an SVG check and `aria-pressed`. Other screenings of the same film use soft fill plus a dashed border and an explanatory accessible name, not the selected check. On mobile the dashed styling applies to the row's existing bottom border, not a newly enclosed card.

The details pane is a labeled `section` (`Screening details`, polite live updates), not an `aside`. It holds actual start/room information, a film heading, version, optional poster and external links, runtime/finish data, booking availability, and film information. Its close control is visibly separate from the content. Posters retain their 2:3 ratio, existing URLs, and alternative text; no raster assets change.

Film links have outlined action styling, the control radius, and a 2.75rem minimum height. The IMDb badge alone uses the fixed brand palette and brand type. TMDB attribution remains compact supporting text with an underlined link. Notices and empty states use the coordinated notice palette and retain the smaller notice radius.

### Schedule

Timeline cards show start, language, and title; version/format metadata is hidden there but available in details. The narrow ordered list shows time, room, title, and compact version/format metadata. Full titles and estimated finishes remain in accessible names. The scroll area has visible focus and scroll padding to account for sticky headers. Only timeline-card color/border transitions are animated (.16s ease); reduced-motion preference removes them.

Calendar and details share thin standards-based scrollbars with Scrollbar Thumb on transparent tracks and stable gutters. Under `@supports selector(::-webkit-scrollbar)`, Chromium/Safari reset standard width/color to auto and use a .625rem (10px) gutter with a .125rem (2px) transparent thumb border: a 6px visible thumb at the default root size. The thumb is fully rounded (999px), uses padding-box clipping, and changes to Accent on hover. Track margins use `--radius-section` (1.5rem / 24px) to clear the panes' curved ends; tracks and corners are transparent and arrow buttons are hidden. Forced colors restore standard `scrollbar-color: auto` and use CanvasText thumbs with Highlight hover for WebKit styling. These scrollbars add neither noise nor enclosing surface treatment.

## Do's and Don'ts

### Do:
- **Do** maintain paired mineral/teal light and dark roles, persist explicit Theme choices when storage is available, and follow live device changes in System mode.
- **Do** preserve labels, inline SVG selection checks, dashed same-film cues, and independent visible keyboard focus.
- **Do** preserve the wide timeline, narrow chronological list, and the smallest date-toolbar wrapping rule.
- **Do** keep Source Sans 3 as the interface voice and IMDb styling confined to its brand badge.

### Don't:
- **Don't** add noise or texture; relief comes from directional highlights, cast shadows, and beveled edges.
- **Don't** reintroduce cream/burgundy surfaces or the unused serif face as the current identity.
- **Don't** replace selection, focus, or same-film distinctions with color alone.
- **Don't** infer full WCAG conformance from axe results, contrast tests, or this document.
- **Don't** restore the outer schedule card, blur its ancestor, add animated glow, bypass solid preference fallbacks, or change snapshot data or poster assets as part of documenting the system.
