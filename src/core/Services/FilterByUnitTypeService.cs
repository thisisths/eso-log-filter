namespace EsoLogFilter.Core.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using EsoLogFilter.Core.Model;
    using EsoLogFilter.Core.Model.Objects;

    public class FilterByUnitTypeService : IFilterByUnitTypeService
    {
        private readonly HashSet<string> keptUnitIds = new HashSet<string>();
        private readonly HashSet<string> droppedUnitIds = new HashSet<string>();
        private readonly HashSet<string> droppedCastTrackIds = new HashSet<string>();

        public void Reset()
        {
            this.keptUnitIds.Clear();
            this.droppedUnitIds.Clear();
            this.droppedCastTrackIds.Clear();
        }

        public bool IsUnitInFilterAndAdd(LogEntry logEntry, UnitTypes[] unitTypes)
        {
            var unitType = logEntry.GetUnitType();
            var unitId = logEntry.GetIdString();

            // Units of unknown type are kept: silently dropping data the parser does
            // not understand would corrupt the output.
            if (unitType == UnitTypes.Unknown || unitTypes.Contains(unitType))
            {
                this.keptUnitIds.Add(unitId);
                this.droppedUnitIds.Remove(unitId);
                return true;
            }

            this.droppedUnitIds.Add(unitId);
            this.keptUnitIds.Remove(unitId);
            return false;
        }

        public bool ShouldAddUnitRemoved(LogEntry logEntry)
        {
            return this.keptUnitIds.Contains(logEntry.GetIdString());
        }

        public bool ShouldAddUnitChanged(LogEntry logEntry)
        {
            return this.keptUnitIds.Contains(logEntry.GetIdString());
        }

        public bool ShouldAddPlayerInfo(LogEntry logEntry)
        {
            return this.keptUnitIds.Contains(logEntry.GetIdString());
        }

        public bool ShouldAddHealthRegen(LogEntry logEntry)
        {
            return this.keptUnitIds.Contains(logEntry.GetIdString());
        }

        public bool ShouldAddBeginCast(LogEntry logEntry)
        {
            var shouldAdd = this.ShouldAddEvent(logEntry);

            if (!shouldAdd)
            {
                // Remember the cast so its END_CAST lines can be dropped as well.
                this.droppedCastTrackIds.Add(logEntry.GetCastTrackId());
            }

            return shouldAdd;
        }

        public bool ShouldAddEndCast(LogEntry logEntry)
        {
            return !this.droppedCastTrackIds.Contains(logEntry.GetCastTrackId());
        }

        public bool ShouldAddEffectChanged(LogEntry logEntry)
        {
            return this.ShouldAddEvent(logEntry);
        }

        public bool ShouldAddCombatEvent(LogEntry logEntry)
        {
            return this.ShouldAddEvent(logEntry);
        }

        private bool ShouldAddEvent(LogEntry logEntry)
        {
            // Drop only events whose effective target is a unit the user deselected.
            // Events referencing units never seen in a UNIT_ADDED line are kept to
            // stay on the safe side (e.g. logs that start mid-session).
            return !this.droppedUnitIds.Contains(logEntry.GetEffectiveTargetIdString());
        }
    }
}
