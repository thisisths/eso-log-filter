namespace EsoLogFilter.Core.Model.Analysis
{
    using System.Collections.Generic;

    /// <summary>
    /// One BEGIN_COMBAT/END_COMBAT block. Timestamps are ms offsets relative to
    /// the BEGIN_LOG epoch; null when the marker line had no parsable offset or
    /// the fight was never closed.
    /// </summary>
    public class FightSummary
    {
        public int Index { get; set; }

        public long? StartMs { get; set; }

        public long? EndMs { get; set; }

        // Keyed by player display name (pets resolve to their owner); damage from
        // sources that are no player lands under LogSummary.NonPlayerSourcesKey.
        public Dictionary<string, long> DamageBySourcePlayer { get; } = new Dictionary<string, long>();
    }
}
