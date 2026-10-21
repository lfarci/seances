---
name: Séances
description: A clear way to browse Brussels cinema schedules.
colors:
  canvas: "#f7f4ef"
  ink: "#26211d"
  muted: "#51473e"
  paper: "#fffefa"
  divider: "#c5b9ab"
  control-border: "#7c6d60"
  screening-border: "#958575"
  alternate-room: "#efe8df"
  time-header: "#e4d9cb"
  details-paper: "#f5f0e9"
  hover: "#f7eae5"
  burgundy: "#762c27"
  burgundy-deep: "#56201c"
  burgundy-soft: "#ead4cc"
  focus-blue: "#175c9e"
typography:
  display:
    fontFamily: '"Source Serif 4", Georgia, serif'
    fontSize: "clamp(2.25rem, 5vw, 3.5rem)"
    fontWeight: 500
    lineHeight: 1.08
    letterSpacing: "-.025em"
  title:
    fontFamily: '"Source Serif 4", Georgia, serif'
    fontSize: "1.75rem"
    fontWeight: 500
    lineHeight: 1.2
  body:
    fontFamily: '"Source Sans 3", system-ui, sans-serif'
    fontSize: "1rem"
    lineHeight: 1.45
  screening-title:
    fontFamily: '"Source Sans 3", system-ui, sans-serif'
    fontSize: "1.0625rem"
    fontWeight: 650
    lineHeight: 1.25
  time:
    fontSize: "0.875rem"
    fontWeight: 650
rounded:
  control: "0.35rem"
  notice: "0.45rem"
  showing: "0.55rem"
  panel: "0.8rem"
components:
  button-primary:
    backgroundColor: "{colors.burgundy}"
    textColor: "#ffffff"
    rounded: "{rounded.control}"
    padding: "0.55rem 0.75rem"
  button-primary-hover:
    backgroundColor: "{colors.burgundy-deep}"
    textColor: "#ffffff"
  cinema-link-current:
    textColor: "{colors.burgundy-deep}"
---

# Design System: Séances

## Overview

**Creative North Star: “Showtimes at a glance”**

Séances is a task-focused schedule reader for Brussels cinema-goers. Its visual system keeps the incumbent cream-and-burgundy identity, with Source Serif 4 headings and Source Sans 3 calendar text. Darker supporting text, stronger screening outlines, and distinct warm row backgrounds keep cinema, date, room, and start time easy to scan.

The schedule is grouped by cinema and service date. At wide sizes, showings sit on a full-width horizontal timeline with rooms as rows. Selecting a showing opens a contextual details pane on the right; closing it restores the full timeline width. At narrower sizes, the same ordered information becomes a chronological list, with selected details in a right-side drawer. Controls retain visible labels and keyboard focus styling. The code provides no specified WCAG conformance level, and this document makes no conformance claim.

**Key Characteristics:**
- Cream page canvas, warm paper surfaces, and burgundy schedule accents.
- Locally served Source Serif 4 headings paired with Source Sans 3 film titles, controls, and schedule text.
- Horizontal duration-based timeline with rooms as rows on wider screens; chronological screening list on narrow screens. Shared cinema, date, and room filters update both layouts immediately.
- Visible keyboard focus treatment and labeled navigation and date controls.

## Colors

The palette pairs warm cream and paper neutrals with a deep burgundy accent; blue is reserved for keyboard focus.

### Primary
- **Burgundy**: primary actions, selected cinema indication, screening details, and time emphasis.
- **Deep Burgundy**: action and link hover states, selected navigation text, and detail summaries.

### Neutral
- **Cream Canvas**: the page background.
- **Ink**: primary text.
- **Muted Brown**: supporting copy and schedule metadata.
- **Warm Paper**: schedule panel and form control backgrounds.
- **Warm Divider**: panel outlines and schedule separators.
- **Burgundy Tint**: screening cards.
- **Focus Blue**: the visible keyboard focus outline.

## Typography

**Display Font:** Source Serif 4 (Georgia fallback)
**Body and Calendar Font:** Source Sans 3 (system sans-serif fallback)

**Character:** Source Serif 4 keeps the cinema programme’s editorial voice in page and detail headings. Source Sans 3’s open forms make dense film titles, room names, times, and controls easier to scan. The two Latin variable WOFF2 files are self-hosted, preloaded, and use font-display swap; their SIL Open Font Licenses are bundled in wwwroot/fonts. No external font connection is required.

### Hierarchy
- **Display** (500, responsive 2.25–3.5rem, 1.08 line-height): the page heading; compacted to 2rem in the desktop workspace.
- **Title** (500, 1.75rem, 1.2 line-height): the selected cinema heading; compacted to 1.25rem on desktop.
- **Screening titles** (650, 1.0625rem, 1.25 line-height): sans-serif film names, with two-line clipping in the timeline and complete titles in accessible names and details.
- **Body** (1rem, 1.45 line-height): controls and schedule details; time labels and small metadata use a 0.875rem floor. Secondary text remains dark rather than using opacity to mute it.
- **Labels** (650–700): control labels, room names, and time values use heavier sans-serif text for scanning.

## Layout

On desktop, the app fills the viewport height and width with 0.75rem outer gutters. Compact headings and a checked-at timestamp share the top row, followed by one filter toolbar. The timeline takes the remaining width and height by default. Selecting a screening reveals a right-hand details pane, aligned with the calendar at both top and bottom; each pane scrolls internally. Closing details or pressing Escape restores the full-width timeline. The time scale expands to fill available width, with a minimum readable scale that scrolls horizontally when needed. Room names stay sticky on the left and time headers stay sticky above. Cinema, date, and room selectors apply on change. Previous/next arrows flank the date selector, with Today beside them.

The desktop details pane starts at 20.5rem but never exceeds 25% of the schedule workspace. Dragging the divider changes its width immediately in the browser, without server round trips. The minimum is the smaller of 16rem and 20% of the workspace. The last width is retained while closing and reopening details on the same page. The focusable separator supports Left/Right arrows, Shift for larger steps, Home/End for minimum/maximum width, and double-click to reset. Resizing the viewport reapplies the limits.

Below 52rem, the page returns to a naturally scrolling centered column; the timeline becomes a chronological list and selected details open in a fixed right-side drawer without a resize divider. Mobile filter fields stack, keeping date navigation together. Below 26rem, content gutters and the list time column tighten.

## Elevation & Depth

The interface uses flat surfaces, borders, and tonal differences; the stylesheet defines no box shadows. The cream canvas, paper panel, shaded table headers, and tinted showing cards establish grouping without elevation effects.

## Shapes

Controls use gently rounded corners, notices and showing cards use a slightly larger radius, and the main schedule panel has the broadest rounding. Thin warm borders define panels, controls, and cards; alternating backgrounds distinguish room rows without grid lines.

## Components

### Buttons
- **Shape:** gently rounded control corners (0.35rem).
- **Primary:** burgundy fill, white text, and compact horizontal padding (0.55rem 0.75rem).
- **Hover / Focus:** hover deepens the burgundy; keyboard focus uses a blue outline with a 3px width and 3px offset.

### Inputs / Fields
- Cinema, date, and room selectors share a filter bar above the schedule, with matching paper backgrounds, warm borders, widths, and control radii. Labels sit above each selector. Each selection immediately updates the schedule and its URL, preserving the other filters; there is no apply button. On narrow screens cinema spans one row, and date and room share the next.
- The selectors have visible “Cinema”, “Date”, and “Room” labels. Focus uses the shared visible keyboard outline. Previous/next date navigation preserves the selected cinema and room when available on the destination date.

### Navigation
- Cinema selection lives in the shared filter group rather than a separate navigation strip. Choosing a different cinema retains the chosen date and resets an unavailable room to all rooms.
- Labeled previous/next arrow controls flank the date selector; unavailable ends of the date range remain visible but disabled. Today is enabled only when the selected cinema has published coverage for the current Brussels date, and preserves the room filter when available. The checked-at timestamp remains visible without an out-of-date warning banner.

### Cards / Containers
- The schedule panel uses a paper surface, warm divider border, and broad corner rounding.
- Screening cards use a paper surface, a fine warm border, generous internal padding, and semibold Source Sans 3 film titles. The start time sits top-left and the language version (for example VFstNL) sits in a small outlined tag top-right; estimated finishes appear only in the details pane and accessible names. Hover adds a soft burgundy tint; selected desktop cards use a solid burgundy fill with white text. Mobile selection retains a pale tint. Room lanes are 6.5rem tall to give two-line film titles breathing room.

### Schedule
- The desktop timeline uses labeled room rows and selectable screening buttons. Its horizontal time axis and room names stay visible within the scroll region. Hourly time labels and alternating paper/cream room backgrounds replace grid lines; each room name shares its row’s background. A proportional time scale positions blocks left to right from their start to estimated finish, so simultaneous films align vertically. Overlapping screenings within one room use stacked lanes.
- Time scrolls horizontally and rooms scroll vertically, with a labeled selector for optionally focusing on one room. Compact screening blocks show their start time, language version, and title; format and estimated finish remain in the details pane. Full titles are retained in accessible button names and the details pane.
- Selecting a showing opens its version, format, runtime, estimated finish, and booking information in a contextual right-hand pane. The pane is absent when nothing is selected; the close control, Escape, or reselecting the same showing dismisses it.
- The narrow layout presents selectable showings in chronological order, with time and room kept alongside each film title; selected details open in a right-side drawer with a close button.

## Do's and Don'ts

### Do:
- **Do** keep the cream-and-burgundy palette, serif headings, and readable sans-serif schedule typography consistent in new schedule surfaces.
- **Do** preserve semantic labels and visible keyboard focus styling for interactive controls.
- **Do** keep cinema, service date, room, and start time easy to scan.
- **Do** preserve the wide duration-based calendar and narrow chronological-list behavior when extending the schedule.

### Don't:
- **Don't** claim a WCAG conformance level from the current design documentation; none has been specified.
- **Don't** use shadows as a new default depth treatment; the current system groups content with borders and tonal surfaces.
