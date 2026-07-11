namespace EsoLogFilter.Core.Model.Objects
{
    public enum LineTypes
    {
        Unknown = -1,
        BeginLog = 0,
        ZoneChanged = 1,
        UnitAdded = 2,
        UnitRemoved = 3,
        UnitChanged = 4,
        BeginCombat = 5,
        EndCombat = 6,
        AbilityInfo = 7,
        EffectInfo = 8,
        PlayerInfo = 9,
        MapChanged = 10,
        BeginCast = 11,
        EndCast = 12,
        EffectChanged = 13,
        CombatEvent = 14,
        HealthRegen = 15,
        EndLog = 16,
        TrialInit = 17,
        BeginTrial = 18,
        EndTrial = 19
    }
}
