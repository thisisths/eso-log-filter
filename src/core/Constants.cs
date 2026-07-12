namespace EsoLogFilter.Core
{
    public struct Constants
    {
        public struct LineTypes
        {
            public const string BeginLog = "BEGIN_LOG";
            public const string ZoneChanged = "ZONE_CHANGED";
            public const string UnitAdded = "UNIT_ADDED";
            public const string UnitRemoved = "UNIT_REMOVED";
            public const string UnitChanged = "UNIT_CHANGED";
            public const string BeginCombat = "BEGIN_COMBAT";
            public const string EndCombat = "END_COMBAT";
            public const string AbilityInfo = "ABILITY_INFO";
            public const string EffectInfo = "EFFECT_INFO";
            public const string PlayerInfo = "PLAYER_INFO";
            public const string MapChanged = "MAP_CHANGED";
            public const string BeginCast = "BEGIN_CAST";
            public const string EndCast = "END_CAST";
            public const string EffectChanged = "EFFECT_CHANGED";
            public const string CombatEvent = "COMBAT_EVENT";
            public const string HealthRegen = "HEALTH_REGEN";
            public const string EndLog = "END_LOG";
            public const string TrialInit = "TRIAL_INIT";
            public const string BeginTrial = "BEGIN_TRIAL";
            public const string EndTrial = "END_TRIAL";
        }

        public struct UnitTypes
        {
            public const string Player = "PLAYER";
            public const string Monster = "MONSTER";
            public const string Object = "OBJECT";
            public const string SiegeWeapon = "SIEGE_WEAPON";
        }

        public struct MonsterTypes
        {
            public const string Hostile = "HOSTILE";
            public const string NpcAlly = "NPC_ALLY";
            public const string Friendly = "FRIENDLY";
            public const string Neutral = "NEUTRAL";
        }

        // COMBAT_EVENT results whose hitValue is damage dealt to the target.
        // Deliberately excluded: FALL_DAMAGE (self-inflicted, would count as
        // "damage done" by the falling player) and marker results like
        // KILLING_BLOW whose damage is already recorded by the hit itself.
        public struct CombatResults
        {
            public const string Damage = "DAMAGE";
            public const string CriticalDamage = "CRITICAL_DAMAGE";
            public const string DotTick = "DOT_TICK";
            public const string DotTickCritical = "DOT_TICK_CRITICAL";
            public const string BlockedDamage = "BLOCKED_DAMAGE";
            public const string DamageShielded = "DAMAGE_SHIELDED";
        }
    }
}
