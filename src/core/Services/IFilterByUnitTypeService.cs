namespace EsoLogFilter.Core.Services
{
    using EsoLogFilter.Core.Model;
    using EsoLogFilter.Core.Model.Objects;

    public interface IFilterByUnitTypeService
    {
        void Reset();

        bool IsUnitInFilterAndAdd(LogEntry logEntry, UnitTypes[] unitTypes);

        bool ShouldAddUnitRemoved(LogEntry logEntry);

        bool ShouldAddUnitChanged(LogEntry logEntry);

        bool ShouldAddPlayerInfo(LogEntry logEntry);

        bool ShouldAddBeginCast(LogEntry logEntry);

        bool ShouldAddEndCast(LogEntry logEntry);

        bool ShouldAddEffectChanged(LogEntry logEntry);

        bool ShouldAddCombatEvent(LogEntry logEntry);

        bool ShouldAddHealthRegen(LogEntry logEntry);
    }
}
