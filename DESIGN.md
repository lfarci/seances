---
name: Séances
description: A clear way to browse Brussels cinema schedules.
colors:
  canvas: "#f7f4ef"
  ink: "#24211e"
  muted: "#5b554d"
  paper: "#fffefa"
  divider: "#d8d0c5"
  burgundy: "#873324"
  burgundy-deep: "#64251b"
  burgundy-soft: "#f3e7e2"
  focus-blue: "#175c9e"
typography:
  display:
    fontFamily: "Georgia, serif"
    fontSize: "clamp(2.25rem, 5vw, 3.5rem)"
    fontWeight: 500
    lineHeight: 1.08
    letterSpacing: "-.025em"
  title:
    fontFamily: "Georgia, serif"
    fontSize: "1.75rem"
    fontWeight: 500
    lineHeight: 1.2
  body:
    fontFamily: "system-ui, -apple-system, BlinkMacSystemFont, \"Segoe UI\", sans-serif"
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

Séances is a task-focused schedule reader for Brussels cinema-goers. Its visual system keeps the incumbent cream, burgundy, and Georgia identity while using a restrained neutral palette to keep cinema, date, room, and start time easy to scan.

The schedule is grouped by cinema and service date. At wide sizes, showings sit in a semantic room-by-time table; at narrower sizes, the same ordered information becomes a chronological list. Controls retain visible labels and keyboard focus styling. The code provides no specified WCAG conformance level, and this document makes no conformance claim.

**Key Characteristics:**
- Cream page canvas, warm paper surfaces, and burgundy schedule accents.
- Georgia headings paired with a system sans-serif interface font.
- Room-by-time table on wider screens and chronological screening list on narrow screens.
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

**Display Font:** Georgia (serif fallback)
**Body Font:** system-ui, -apple-system, BlinkMacSystemFont, “Segoe UI”, sans-serif

**Character:** Georgia gives page and cinema headings a familiar editorial voice. The system sans-serif keeps controls and schedule details legible with platform-native rendering.

### Hierarchy
- **Display** (500, responsive 2.25–3.5rem, 1.08 line-height): the page heading.
- **Title** (500, 1.75rem, 1.2 line-height): the selected cinema heading.
- **Body** (system default size): introductory copy, labels, and schedule content; local styles set smaller sizes for metadata and table headings.
- **Labels** (650–700): control labels, room names, and time values use heavier sans-serif text for scanning.

## Layout

The page uses a centered content column with a wide maximum on desktop and a narrower maximum below 52rem. A schedule panel groups the cinema address, date controls, freshness information, and schedule. On wide screens, the schedule is a room-by-time table inside a bounded scroll region; room headers and the time column remain sticky while scrolling. Below 52rem, the table is replaced by a chronological list, and date controls stack vertically. Below 26rem, the content gutters and list time column tighten. The cinema chooser can scroll horizontally when its links do not fit.

## Elevation & Depth

The interface uses flat surfaces, borders, and tonal differences; the stylesheet defines no box shadows. The cream canvas, paper panel, shaded table headers, and tinted showing cards establish grouping without elevation effects.

## Shapes

Controls use gently rounded corners, notices and showing cards use a slightly larger radius, and the main schedule panel has the broadest rounding. Thin warm borders define panels, controls, cards, and table cells. The cinema chooser uses a bottom rule with a burgundy underline for its current link.

## Components

### Buttons
- **Shape:** gently rounded control corners (0.35rem).
- **Primary:** burgundy fill, white text, and compact horizontal padding (0.55rem 0.75rem).
- **Hover / Focus:** hover deepens the burgundy; keyboard focus uses a blue outline with a 3px width and 3px offset.

### Inputs / Fields
- The date selector uses a paper background, ink text, a warm border, and the shared control radius. On narrow screens it fills the available width.
- The selector is paired with a visible “Choose date” label. Focus uses the shared visible keyboard outline.

### Navigation
- Cinema links are horizontally arranged, with a burgundy bottom indicator and deeper burgundy text for the selected cinema. Hover adds a warm neutral background.
- Date navigation keeps previous and next links beside the selected date; unavailable ends of the date range remain visible as disabled text.

### Cards / Containers
- The schedule panel uses a paper surface, warm divider border, and broad corner rounding.
- Screening cards use a pale burgundy tint, a fine warm border, and compact internal padding. Additional details are disclosed with the native details/summary control.

### Schedule
- The desktop table uses semantic column and row headers for rooms and times. Its time column and room headers stay visible within the scroll region.
- The narrow layout presents showings in chronological order, with time and room kept alongside each film title.

## Do's and Don'ts

### Do:
- **Do** keep the cream, burgundy, and Georgia identity visible in new schedule surfaces.
- **Do** preserve semantic labels and visible keyboard focus styling for interactive controls.
- **Do** keep cinema, service date, room, and start time easy to scan.
- **Do** preserve the wide table and narrow chronological-list behavior when extending the schedule.

### Don't:
- **Don't** claim a WCAG conformance level from the current design documentation; none has been specified.
- **Don't** use shadows as a new default depth treatment; the current system groups content with borders and tonal surfaces.
