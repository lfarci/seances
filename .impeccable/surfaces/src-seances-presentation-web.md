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

OWN-WORLD: Mineral canvas, layered reading surfaces, deep teal active states, and Source Sans 3 throughout. Following the user's OpenCode Desktop reference and subsequent explicit glass request, use generous panel radii, translucent schedule/details shells, bounded backdrop blur, luminous rims, and stronger static teal glow. Calendar lanes and controls remain opaque. Dark mode pairs slate glass with mint-teal accents. Keep strong control boundaries and separate focus outlines; provide opaque fallbacks for unsupported blur, reduced transparency, increased contrast, and forced colors.

STORY: Choose a cinema, service date, and optional room; scan start times; select a screening to read details. Selection has both a tonal change and a visible check, with a separate keyboard focus indicator.

FIRST VIEWPORT: Preserve the compact heading, cinema/address and checked-at row, labeled filter bar, and full-width room timeline. Details stay on the right when selected. On phones retain the chronological list and details drawer. Selected screenings visibly anchor the reading path without moving surrounding content.

FORM: Belgian modernist cultural wayfinding, candidate 3; seed:6da2d194f063c230. Standard web controls and unchanged navigation. The signature move is a check-marked active screening linking the time grid to the details pane.

FINISH: unreviewed and undocumented is unfinished; this build ends with the finish review, the verdict, DESIGN.md, and every shipping raster carrying its provenance
