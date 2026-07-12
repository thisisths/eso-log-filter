namespace EsoLogFilter.Core.Services
{
    using EsoLogFilter.Core.Model;
    using EsoLogFilter.Core.Model.Analysis;
    using EsoLogFilter.Core.Model.Objects;

    public class LogSummaryService : ILogSummaryService
    {
        private LogSummary summary = new LogSummary();
        private long? openFightStartMs;

        public void Reset()
        {
            // A new instance instead of clearing, so a summary handed out by
            // GetSummary() stays valid while the next file is processed.
            this.summary = new LogSummary();
            this.openFightStartMs = null;
        }

        public void ProcessLine(LogEntry logEntry)
        {
            this.summary.TotalLines++;

            var recordType = logEntry.LineArray[1];
            this.summary.LinesByRecordType.TryGetValue(recordType, out var lineCount);
            this.summary.LinesByRecordType[recordType] = lineCount + 1;

            switch (logEntry.LineType)
            {
                case LineTypes.UnitAdded:
                    var unitType = logEntry.GetUnitType();
                    this.summary.UnitsByType.TryGetValue(unitType, out var unitCount);
                    this.summary.UnitsByType[unitType] = unitCount + 1;
                    break;
                case LineTypes.BeginCombat:
                    this.summary.FightCount++;
                    this.openFightStartMs = GetTimestampMs(logEntry);
                    break;
                case LineTypes.EndCombat:
                    var endMs = GetTimestampMs(logEntry);
                    if (this.openFightStartMs.HasValue && endMs.HasValue && endMs.Value >= this.openFightStartMs.Value)
                    {
                        this.summary.CombatTimeMs += endMs.Value - this.openFightStartMs.Value;
                    }

                    this.openFightStartMs = null;
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
    }
}
