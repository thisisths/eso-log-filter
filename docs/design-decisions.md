# Design decisions

Short records of *why* the filter works the way it does, so future changes
don't accidentally relitigate them.

## Two-layer filtering; events pass through by default

The filter has two layers: unit registration lines (`UNIT_ADDED` and friends)
are filtered by unit type, while event lines (`COMBAT_EVENT`, `EFFECT_CHANGED`,
casts) are kept unless the experimental option is enabled. This is enough for
the primary use case — uploading to [ESO Logs](https://www.esologs.com/) —
because ESO Logs ignores events of units that were never registered. Dropping
only the registrations hides the units while keeping the file maximally
faithful to what the game wrote.

## The event rule is target-based

The question the tool answers is "what happened *to* the units I care about."
An event is kept if its effective target is not a dropped unit; the target
falls back to the source for self-targeted (`*`) and dead-target events, and
`END_CAST` follows the verdict of its `BEGIN_CAST` via the cast id. Filtering
by *source* instead would remove NPC→player damage, making damage-taken stats
and death recaps misleading — players would appear to die to nothing.

## No source/target checkbox matrix

Considered and rejected (2026-07). Real fights are almost always purely PvP or
purely PvE, so a source×target matrix adds UI complexity for a rare mixed-fight
case. There is also a fundamental limit: no per-event filter can make *support*
stats "pure PvP" in a mixed fight, because heals cannot be causally attributed
to the damage they compensate (HoTs are even cast before the damage lands).
Shields are the partial exception — each `DAMAGE_SHIELDED` event names the
attacker — but exploiting that is an analysis feature, not a filtering feature.

## Enemy pets are their own category

The log records reactions from the logging player's perspective: your own
pets are `MONSTER`+`NPC_ALLY`, but enemy players' pets are `MONSTER`+`HOSTILE`
— the same bucket as guards. The two are distinguished by `ownerUnitId`: pets
are owned by a player unit, NPCs have owner `0`. Hence the separate
"Monster - Npc Enemy (Pet)" category (default-enabled), so PvP reports include
damage to enemy pets without letting guards back in.

## Fail-open parsing

Record types, unit types, and reactions the parser does not recognize are
passed through and kept. A game update that adds new record types must never
crash the filter or silently drop data. (Before this policy, a single unknown
line aborted the whole run.)

## The preview compares two files, not the filter run

The Preview tab (2026-07) summarizes two independently chosen files instead of
tapping into the filter pass. Piggybacking on the filter would be one pass
cheaper, but two-file compare is strictly more general: it works on any
archived raw/filtered pair (e.g. validating the experimental event filtering
against a log ESO Logs already accepted) and later on a single log for local
analysis. It also keeps the summarizer read-only by construction, so the
byte-identical filter guarantee cannot be affected by preview features. At
~2 s per 400 MB file the extra pass is cheap.

## Damage attribution mirrors ESO Logs

The Preview tab's damage table (2026-07) counts an event only when its
effective target is registered via `UNIT_ADDED` in the file being summarized —
the rule ESO Logs applies when it ignores events of unregistered units. This
makes the filtered file's summary show the effective upload without simulating
anything. Further rules, each deliberate: only real damage results count
(`FALL_DAMAGE` is excluded as self-inflicted), pets/summons/siege resolve to
their owning player via `ownerUnitId`, and nameless players (enemies in
Cyrodiil) are aggregated into one "(anonymous players)" bucket because their
unit ids are no stable identities — the same person reappears under new ids,
so per-id rows would fake precision that is not there.

## Determinism as the regression harness

Same input + same settings ⇒ byte-identical output, always. This makes old
filtered files a perfect regression oracle: after any refactoring, re-filter an
archived raw log with the same unit selection and compare file hashes.
