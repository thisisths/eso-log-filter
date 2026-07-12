namespace EsoLogFilter.Core.Model.Analysis
{
    using System.Collections.Generic;
    using System.Linq;
    using EsoLogFilter.Core.Model.Objects;

    /// <summary>
    /// Aggregate counts of one log file, built in a single streaming pass.
    /// Holds only aggregates, never lines, so multi-GB logs stay cheap.
    /// </summary>
    public class LogSummary
    {
        // Bucket for damage whose source is no player and not owned by one
        // (guards, walls, siege NPCs, ...).
        public const string NonPlayerSourcesKey = "(non-player sources)";

        // Bucket for players registered without name and display name — enemy
        // players in Cyrodiil. Their unit ids are no stable identities (the same
        // person can appear under several ids), so they are aggregated instead
        // of shown as pseudo-individual rows.
        public const string AnonymousPlayersKey = "(anonymous players)";

        public long FileSizeBytes { get; set; }

        // Absolute Unix epoch in ms from BEGIN_LOG; every other timestamp in the
        // file is an offset relative to it. 0 when the file has no BEGIN_LOG.
        public long EpochMs { get; set; }

        public long TotalLines { get; set; }

        // Keyed by the raw record-type token so record types this app does not
        // know yet still show up under their real name.
        public Dictionary<string, long> LinesByRecordType { get; } = new Dictionary<string, long>();

        public Dictionary<UnitTypes, long> UnitsByType { get; } = new Dictionary<UnitTypes, long>();

        public int FightCount { get; set; }

        public long CombatTimeMs { get; set; }

        public List<FightSummary> Fights { get; } = new List<FightSummary>();

        // Whole-log damage per source player; see FightSummary for the key rules.
        // Only hits on registered units are counted, mirroring how ESO Logs
        // attributes events.
        public Dictionary<string, long> DamageBySourcePlayer { get; } = new Dictionary<string, long>();

        public long GetTotalUnitCount()
        {
            return this.UnitsByType.Values.Sum();
        }

        public long GetUnitCount(UnitTypes unitType)
        {
            return this.UnitsByType.TryGetValue(unitType, out var count) ? count : 0;
        }

        public long GetLineCount(string recordType)
        {
            return this.LinesByRecordType.TryGetValue(recordType, out var count) ? count : 0;
        }
    }
}
