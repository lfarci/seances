---
version: 1
slug: "src-seances-presentation-web"
primary_target: "src/Seances.Presentation.Web"
related_targets: ["src/Seances.Presentation.Web/wwwroot/app.css"]
---

# Schedule browser

Mode: Operate. Scope: the existing web schedule, filters, screening cards, and details pane. Keep the layout and all schedule behavior; replace the warm serif identity. The user chose clarity first, device-aware light/dark appearance, and the first (teal) direction.

## Direction contract

THESIS: Clear cultural wayfinding for choosing a screening; no decorative cinema-poster treatment or rearranged workspace.

OWN-WORLD: The user clarified that 'texture' means physical relief, NOT a grain pattern. Keep the workspace unboxed and noise-free. Use directional highlights, bevelled edges, restrained cast shadows, and teal glass accents to separate raised controls from recessed screening lanes. Make calendar orientation prominent: a raised sticky room rail, a taller time ruler with larger hourly labels, and distinct half-hour tick marks. Retain strong lane/text contrast, Source Sans 3, paired themes, and independent focus outlines. Accessibility preferences remove glass and relief decoration while retaining solid boundaries and time ticks.

STORY: Choose a cinema, service date, and optional room; scan start times; select a screening to read details. Selection has both a tonal change and a visible check, with a separate keyboard focus indicator. A visible Theme selector offers Light, Dark, and System, persists locally before paint, and keeps live device alignment in System mode. Calendar and details use slim, rounded scroll thumbs on transparent tracks, inset from the pane's curved ends.

FIRST VIEWPORT: Preserve the compact heading, cinema/address and checked-at row, labeled filter bar, and full-width room timeline. Details stay on the right when selected. On phones retain the chronological list and details drawer. Selected screenings visibly anchor the reading path without moving surrounding content.

FORM: Belgian modernist cultural wayfinding, candidate 3; seed:6da2d194f063c230. Standard web controls and unchanged navigation. The signature move is a check-marked active screening linking the time grid to the details pane.

FINISH: unreviewed and undocumented is unfinished; this build ends with the finish review, the verdict, DESIGN.md, and every shipping raster carrying its provenance
