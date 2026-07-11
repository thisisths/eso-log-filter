# How the filter works

ESO writes `Encounter.log` as CSV-like lines. Each line starts with a millisecond
offset and a record type (`UNIT_ADDED`, `COMBAT_EVENT`, `EFFECT_CHANGED`, ...).
The filter decides per line whether it is written to the output file; kept lines
are copied unchanged and in order.

## Unit classification

Every unit announces itself with a `UNIT_ADDED` line. The filter classifies it
into one of the selectable categories:

- **Players** (`PLAYER`)
- **Monster – Hostile / Npc Ally (Pet) / Friendly / Neutral** (`MONSTER`, split by its reaction)
- **Object** (doors, walls, and other attackable objects)
- **Siege weapon**

A unit whose category is selected in the UI is *kept*; otherwise it is *dropped*.
Units the parser cannot classify (e.g. added by a future game update) are always
kept, so no data is lost silently.

## Line categories

**Always kept** — structural and lookup records that downstream tools need:
`BEGIN_LOG`, `END_LOG`, `ZONE_CHANGED`, `MAP_CHANGED`, `BEGIN_COMBAT`,
`END_COMBAT`, `ABILITY_INFO`, `EFFECT_INFO`, trial records, and any record type
the app does not know.

**Kept only for kept units** — records that describe a single unit:
`UNIT_ADDED`, `UNIT_CHANGED`, `UNIT_REMOVED`, `PLAYER_INFO`, `HEALTH_REGEN`.

**Event records** — `COMBAT_EVENT`, `EFFECT_CHANGED`, `BEGIN_CAST`, `END_CAST`:

- By default these are **always kept**. This is the long-standing behavior: the
  output shrinks only a little, but tools like [ESO Logs](https://www.esologs.com/)
  no longer show the dropped units because their `UNIT_ADDED` registrations are gone.
- With **"Also filter combat/effect events of hidden units (experimental)"**
  enabled, an event is kept only if its *effective target* is not a dropped unit:
  - The target of the event decides; damage from an NPC **to a player** stays,
    damage from a player **to an NPC** goes.
  - Events targeting the source itself (self-buffs, self-heals) and events with
    no target use the source unit instead.
  - `END_CAST` lines have no unit information; they follow the verdict of the
    `BEGIN_CAST` with the same cast id.
  - Events referencing units that never had a `UNIT_ADDED` line are kept.

## Typical PvP use case

For a PvP raid log, keeping **Players** and **Monster – Npc Ally (Pet)** (the
defaults) removes guards, siege weapons, and attackable objects from the report.
Enabling the experimental event filter additionally removes the damage/effect
lines *onto* those units, so the log really only contains what happened to
players and their pets. On a typical Cyrodiil raid log only a few percent of the
event lines target NPCs — the point of the option is a cleaner report, not a
smaller file.

## Guarantees

- The output is deterministic: the same input with the same settings always
  produces the identical output file.
- Kept lines are byte-identical to the input lines; nothing is rewritten.
- An existing target file is fully overwritten.
