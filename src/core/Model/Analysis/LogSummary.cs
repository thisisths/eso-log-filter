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
        public long FileSizeBytes { get; set; }

        public long TotalLines { get; set; }

        // Keyed by the raw record-type token so record types this app does not
        // know yet still show up under their real name.
        public Dictionary<string, long> LinesByRecordType { get; } = new Dictionary<string, long>();

        public Dictionary<UnitTypes, long> UnitsByType { get; } = new Dictionary<UnitTypes, long>();

        public int FightCount { get; set; }

        public long CombatTimeMs { get; set; }

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
