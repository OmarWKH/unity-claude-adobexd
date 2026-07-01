# Open Questions & Missing Assets — for the designer

Running list of things to confirm or obtain. Resolve items by noting the answer inline.
Primary way to resolve transition questions: interactively test the prototype in the browser
(XD preview mode) + read the XD interaction spec; ask the designer when still unclear.

## Missing assets

- **Background grid** — The sky-blue grid background (`#A9C4F0` with white grid lines) seen on
  Home / Gameplay is not in `Sprites/`. Need one of: (a) export of the background from XD, or
  (b) a tileable grid-cell sprite plus exact cell size, line width, and line color. Interim:
  flat `#A9C4F0` color Image placeholder (no grid lines).

## Transitions to verify (in-browser preview + spec)

- **Durations & easing** for every transition — read from XD interaction spec; video complements.
- **Results overlay in** — doc says "fades in from below," but backdrops slide without fading
  (per Omar). Confirm whether Results has any alpha fade or is a pure slide-up.
- **Home → Gameplay (Play)** — stage transition, Gameplay slides up. Confirm whether Home holds
  in place or also moves.
- **Gameplay → Leave (X)** — confirm it's an overlay slide-up like the others.

## Behavior to confirm

- **Timer at 0** — countdown starts at 30s, blinks between yellow and red at ≤10s remaining.
  What happens when it hits 0? (auto-submit / time-out screen / just stops?) Unspecified.
- **Timer blink colors** — exact yellow and red hex. Gold token `#EFC583`; red likely coral
  `#E2917F` — confirm.
- **Answer reveal** — when submitted, does ONLY the selected option recolor (green if right /
  coral if wrong), or is the correct option always highlighted too? (PNGs show only the
  selected option changing.)

## Known non-functional (per spec, no action needed)

- "1 / 10" question counter, the "✓ 5" badge, Notifications toggles, Difficulty selector,
  and Results content (always "Excellent! / 9 out of 10") are all hardcoded / non-functional.
