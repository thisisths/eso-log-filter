# Preview & compare

The **Preview** tab compares an unfiltered log with its filtered twin before you
upload, so a bad filter run is caught locally instead of on
[ESO Logs](https://www.esologs.com/).

## How to use it

1. Run a filter on the **Filter** tab — the Preview tab's file fields are
   prefilled with that run's source and target. Alternatively pick any two log
   files, for example an archived pair.
2. Hit **Analyze**. Both files are read once, streaming; roughly 2 seconds per
   400 MB, with constant memory regardless of file size. The progress bar shows
   real progress (bytes read).
3. Check the verdict strip and the tables.

## What it shows

- **Verdict strip** — file size, line count, units registered (kept/hidden), and
  fight count (`BEGIN_COMBAT`/`END_COMBAT` pairs with total combat time). The
  units tile also calls out **players hidden**, which should always be 0 for a
  PvP upload; it turns red otherwise.
- **Units** — per unit category: how many units the unfiltered log registered,
  how many the filtered log kept, and how many are hidden.
- **Damage** — damage done per source player, with a fight list on the left
  ("Whole log" or a single `BEGIN_COMBAT`/`END_COMBAT` block). Three columns per
  row: **Total** (everything in the unfiltered log), **On ESO Logs** (what the
  filtered file will show after upload), and **Removed** (damage to hidden
  units — guards, walls, siege). This answers the raid question directly: who
  hits players, who farms NPCs.
- **Record types** — line counts per record type in both files and the
  difference. In the default filter mode only the unit-registration record types
  show drops; event lines pass through unchanged.

## How damage is counted

- Damage results only: `DAMAGE`, `CRITICAL_DAMAGE`, `DOT_TICK`,
  `DOT_TICK_CRITICAL`, `BLOCKED_DAMAGE`, `DAMAGE_SHIELDED`. `FALL_DAMAGE` is
  excluded (self-inflicted; it would count as "damage done" by the falling
  player). Healing is out of scope for now.
- An event only counts if its target is still **registered** in that file —
  the same rule ESO Logs applies. That is why the two columns differ although
  the event lines are identical in default mode.
- Pets, summons and sieges count toward their owning player.
- Enemy players are logged without name; they are aggregated into one
  **(anonymous players)** row, because their unit ids are no stable identities.
  Damage from NPCs lands in **(non-player sources)**.

## Why the comparison is count-based, not a text diff

In the default mode the filtered file keeps every event line and only drops
unit registrations — a text diff would show a fraction of a percent of changed
lines and none of the meaning. The counts show the actual effect: which unit
categories disappeared and which record types shrank. ESO Logs ignores events
of units that were never registered, so hidden registrations are what makes
those units vanish from the report even though their event lines are still in
the file.

## Not yet included

Planned next (see the [roadmap](roadmap.md)): per-fight drill-down (abilities,
top targets, deaths), healing tables, and CSV/HTML export of the views.
