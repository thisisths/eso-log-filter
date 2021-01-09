namespace EsoLogFilter.Core.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using EsoLogFilter.Core.Model;
    using EsoLogFilter.Core.Model.Objects;
    
    public class FilterByUnitTypeService : IFilterByUnitTypeService
    {
        private readonly List<string> idsToAdd;
        private readonly List<string> idsToFilter;

        public FilterByUnitTypeService()
        {
            this.idsToAdd = new List<string>();
            this.idsToFilter = new List<string>();
        }

        public bool IsUnitInFilterAndAdd(LogEntry logEntry, UnitTypes[] unitTypes)
        {
            var unitType = logEntry.GetUnitType();

            if (unitTypes.Contains(unitType))
            {
                this.idsToAdd.Add(logEntry.GetIdString());
                return true;
            }

            this.idsToFilter.Add(logEntry.GetIdString());
            return false;
        }

        public bool ShouldAddUnitRemoved(LogEntry logEntry)
        {
            var unitId = logEntry.GetIdString();

#if DEBUG
            this.CheckUnitId(logEntry, unitId);
#endif

            return this.idsToAdd.Contains(unitId);
        }

        public bool ShouldAddUnitChanged(LogEntry logEntry)
        {
            var unitId = logEntry.GetIdString();

#if DEBUG
            this.CheckUnitId(logEntry, unitId);
#endif

            return this.idsToAdd.Contains(unitId);
        }

        public bool ShouldAddPlayerInfo(LogEntry logEntry)
        {
            var unitId = logEntry.GetIdString();

#if DEBUG
            this.CheckUnitId(logEntry, unitId);
#endif

            return this.idsToAdd.Contains(unitId);
        }

        public bool ShouldAddBeginCast(LogEntry logEntry)
        {
            return true;
            var sourceId = logEntry.GetSourceIdString();

#if DEBUG
            this.CheckUnitId(logEntry, sourceId);
#endif

            return this.idsToAdd.Contains(sourceId);
        }

        public bool ShouldAddEndCast(LogEntry logEntry)
        {
            return true;
            var sourceId = logEntry.GetSourceIdString();

#if DEBUG
            this.CheckUnitId(logEntry, sourceId);
#endif

            return this.idsToAdd.Contains(sourceId);
        }

        public bool ShouldAddEffectChanged(LogEntry logEntry)
        {
            return true;
            var targetId = logEntry.GetTargetIdString();

#if DEBUG
            this.CheckUnitId(logEntry, targetId);
#endif

            return this.idsToAdd.Contains(targetId);
        }

        public bool ShouldAddCombatEvent(LogEntry logEntry)
        {
            return true;
            var targetId = logEntry.GetTargetIdString();

#if DEBUG
            this.CheckUnitId(logEntry, targetId);
#endif

            return this.idsToAdd.Contains(targetId);
        }

#if DEBUG
        private void CheckUnitId(LogEntry logEntry, string unitId)
        {
            if (!this.idsToAdd.Contains(unitId) && !this.idsToFilter.Contains(unitId))
            {
                throw new System.Exception($"unknown UnitId '{unitId}': Line: {logEntry.Line}");
            }
        }
#endif
    }
}
