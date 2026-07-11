# Roadmap / ideas

Rough backlog, in no particular order. No promises — see the disclaimer in the
[README](../README.md).

## Local analysis (long-term goal)

Analyze a raid log locally without uploading it anywhere:

- Per-fight summaries (`BEGIN_COMBAT`/`END_COMBAT` blocks): duration, involved
  players, damage done/taken.
- Damage-to-players breakdown per source player — the main PvP question ("who
  actually hits enemy players?").
- Top-target tables (which enemies received the most damage/healing).
- Export as CSV/HTML instead of only a filtered log.

## Filtering

- Promote the experimental event filtering to a stable option once it has been
  validated against ESO Logs uploads a few more times.
- Optional rule variants, e.g. "keep events where source *or* target is kept".

## Usability

- Progress bar with real progress (bytes read / file size) instead of the
  indeterminate spinner.
- Drag & drop for the source file; prefill the target filename next to the
  source file.
- Remember the last used settings.

## Tooling

- CLI/batch mode as a first-class feature (the console harness already covers
  the basics) for automatically filtering fresh logs after a raid.
- CI workflow that builds and runs the tests on pull requests.
- Bump Avalonia when a release fixes the `Tmds.DBus.Protocol` advisory
  (GHSA-xrw6-gwf8-vvr9) currently reported as NU1903 during restore.
- Linux build target in the release workflow.
