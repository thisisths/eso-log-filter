namespace EsoLogFilter.Core.Services
{
    using System.Collections.Generic;
    using EsoLogFilter.Core.Model;
    using EsoLogFilter.Core.Model.Analysis;
    using EsoLogFilter.Core.Model.Objects;

    public class LogSummaryService : ILogSummaryService
    {
        private const string NoUnitId = "0";

        private static readonly HashSet<string> DamageResults = new HashSet<string>
        {
            Constants.CombatResults.Damage,
            Constants.CombatResults.CriticalDamage,
            Constants.CombatResults.DotTick,
            Constants.CombatResults.DotTickCritical,
            Constants.CombatResults.BlockedDamage,
            Constants.CombatResults.DamageShielded,
        };

        private readonly HashSet<string> registeredUnitIds = new HashSet<string>();
        private readonly Dictionary<string, string> sourcePlayerKeyByUnitId = new Dictionary<string, string>();

        private LogSummary summary = new LogSummary();
        private FightSummary currentFight;

        public void Reset()
        {
            // A new instance instead of clearing, so a summary handed out by
            // GetSummary() stays valid while the next file is processed.
            this.summary = new LogSummary();
            this.currentFight = null;
            this.registeredUnitIds.Clear();
            this.sourcePlayerKeyByUnitId.Clear();
        }

        public void ProcessLine(LogEntry logEntry)
        {
            this.summary.TotalLines++;

            var recordType = logEntry.LineArray[1];
            this.summary.LinesByRecordType.TryGetValue(recordType, out var lineCount);
            this.summary.LinesByRecordType[recordType] = lineCount + 1;

            switch (logEntry.LineType)
            {
                case LineTypes.BeginLog:
                    if (logEntry.LineArray.Length > 2 && long.TryParse(logEntry.LineArray[2], out var epochMs))
                    {
                        this.summary.EpochMs = epochMs;
                    }

                    break;
                case LineTypes.UnitAdded:
                    var unitType = logEntry.GetUnitType();
                    this.summary.UnitsByType.TryGetValue(unitType, out var unitCount);
                    this.summary.UnitsByType[unitType] = unitCount + 1;
                    this.RegisterUnit(logEntry, unitType);
                    break;
                case LineTypes.BeginCombat:
                    this.summary.FightCount++;
                    this.currentFight = new FightSummary
                    {
                        Index = this.summary.FightCount,
                        StartMs = GetTimestampMs(logEntry),
                    };
                    this.summary.Fights.Add(this.currentFight);
                    break;
                case LineTypes.EndCombat:
                    this.CloseFight(logEntry);
                    break;
                case LineTypes.CombatEvent:
                    this.AddDamage(logEntry);
                    break;
            }
        }

        public LogSummary GetSummary()
        {
            return this.summary;
        }

        private static long? GetTimestampMs(LogEntry logEntry)
        {
            // Unmatched markers and malformed offsets skip the time accounting
            // instead of aborting the run (fail-open).
            return long.TryParse(logEntry.LineArray[0], out var offsetMs) ? offsetMs : null;
        }

        private void RegisterUnit(LogEntry logEntry, UnitTypes unitType)
        {
            var unitId = logEntry.GetIdString();
            this.registeredUnitIds.Add(unitId);

            if (unitType == UnitTypes.Player)
            {
                var displayName = logEntry.GetUnitDisplayName();
                var key = !string.IsNullOrEmpty(displayName) ? displayName : logEntry.GetUnitName();
                this.sourcePlayerKeyByUnitId[unitId] = !string.IsNullOrEmpty(key) ? key : LogSummary.AnonymousPlayersKey;
                return;
            }

            // Pets, summons and siege count toward their owning player. The owner
            // is always registered before its pet, so the lookup resolves here;
            // units without a player owner (guards, walls, ...) get no key and
            // land in the non-player bucket.
            var ownerUnitId = logEntry.GetOwnerUnitIdString();
            if (ownerUnitId != NoUnitId && this.sourcePlayerKeyByUnitId.TryGetValue(ownerUnitId, out var ownerKey))
            {
                this.sourcePlayerKeyByUnitId[unitId] = ownerKey;
            }
        }

        private void CloseFight(LogEntry logEntry)
        {
            // An END_COMBAT without a BEGIN_COMBAT is ignored.
            if (this.currentFight == null)
            {
                return;
            }

            var endMs = GetTimestampMs(logEntry);
            this.currentFight.EndMs = endMs;

            if (this.currentFight.StartMs.HasValue && endMs.HasValue && endMs.Value >= this.currentFight.StartMs.Value)
            {
                this.summary.CombatTimeMs += endMs.Value - this.currentFight.StartMs.Value;
            }

            this.currentFight = null;
        }

        private void AddDamage(LogEntry logEntry)
        {
            if (!DamageResults.Contains(logEntry.GetCombatResult()))
            {
                return;
            }

            var hitValue = logEntry.GetHitValue();
            if (hitValue <= 0)
            {
                return;
            }

            // ESO Logs only attributes events whose target unit was registered via
            // UNIT_ADDED. Mirroring that rule is what makes the filtered file's
            // summary show the effective upload: its event lines are identical to
            // the unfiltered file in default mode, only registrations differ.
            if (!this.registeredUnitIds.Contains(logEntry.GetEffectiveTargetIdString()))
            {
                return;
            }

            this.sourcePlayerKeyByUnitId.TryGetValue(logEntry.GetSourceIdString(), out var sourceKey);
            var key = sourceKey ?? LogSummary.NonPlayerSourcesKey;

            AddTo(this.summary.DamageBySourcePlayer, key, hitValue);

            if (this.currentFight != null)
            {
                AddTo(this.currentFight.DamageBySourcePlayer, key, hitValue);
            }
        }

        private static void AddTo(Dictionary<string, long> damageByKey, string key, long value)
        {
            damageByKey.TryGetValue(key, out var current);
            damageByKey[key] = current + value;
        }
    }
}
