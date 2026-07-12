namespace EsoLogFilter.Tests
{
    /// <summary>
    /// Real-world shaped Encounter.log sample lines (names and ids anonymized).
    /// </summary>
    public static class TestLogLines
    {
        public const string BeginLog = "10,BEGIN_LOG,1783021379102,15,\"EU Megaserver\",\"en\",\"eso.live.12.0\"";

        public const string ZoneChanged = "10,ZONE_CHANGED,181,\"Cyrodiil\",NONE";

        // unitId 1, PLAYER
        public const string UnitAddedPlayer = "10,UNIT_ADDED,1,PLAYER,T,1,0,F,6,5,\"Jane Doe\",\"@janedoe\",1234567890123456789,50,2643,0,PLAYER_ALLY,T";

        // unitId 15908, MONSTER with reaction NPC_ALLY (a player pet)
        public const string UnitAddedPet = "191,UNIT_ADDED,15908,MONSTER,F,0,33152,F,0,0,\"Twilight Matriarch\",\"\",0,50,160,15906,NPC_ALLY,F";

        // unitId 15956, MONSTER with reaction HOSTILE and no owner (an NPC, e.g. a guard)
        public const string UnitAddedHostile = "6554,UNIT_ADDED,15956,MONSTER,F,0,51854,F,0,0,\"Dominion Mender\",\"\",0,50,160,0,HOSTILE,F";

        // unitId 16600, MONSTER with reaction HOSTILE owned by unit 777 (an enemy player's pet)
        public const string UnitAddedEnemyPet = "7000,UNIT_ADDED,16600,MONSTER,F,0,33152,F,0,0,\"Twilight Matriarch\",\"\",0,50,160,777,HOSTILE,F";

        // Enemy pet whose name contains a comma; owner and reaction must still be found relative to the line end.
        public const string UnitAddedEnemyPetNameWithComma = "7001,UNIT_ADDED,16601,MONSTER,F,0,33152,F,0,0,\"Matriarch, the Dark\",\"\",0,50,160,777,HOSTILE,F";

        // A monster name containing a comma shifts all following fields by one;
        // the reaction must still be found relative to the line end.
        public const string UnitAddedHostileNameWithComma = "100,UNIT_ADDED,20000,MONSTER,F,0,99999,F,0,0,\"Captain, the Cruel\",\"\",0,50,160,0,HOSTILE,F";

        public const string UnitAddedObject = "500,UNIT_ADDED,16383,OBJECT,F,0,0,F,0,0,\"\",\"\",0,50,160,0,NEUTRAL,F";

        public const string UnitAddedSiegeWeapon = "600,UNIT_ADDED,16400,SIEGE_WEAPON,F,0,0,F,0,0,\"Stone Trebuchet\",\"\",0,50,160,12,FRIENDLY,F";

        public const string UnitRemovedHostile = "3349,UNIT_REMOVED,15956";

        public const string UnitRemovedPlayer = "3350,UNIT_REMOVED,1";

        public const string UnitChangedPet = "141467,UNIT_CHANGED,15908,0,0,\"Dire Wolf\",\"\",0,50,160,0,NPC_ALLY,F";

        public const string PlayerInfo = "12,PLAYER_INFO,1,[142210,78219],[1,1],[[HEAD,207750,T,16,ARMOR_INFUSED,LEGENDARY,771,HEALTH,T,16,LEGENDARY]],[61902,61906],[61919,61932]";

        public const string AbilityInfoScribed = "10,ABILITY_INFO,240150,\"Warding Contingency\",\"/esoui/art/icons/ability_grimoire_staffdestro.dds\",F,T,\"Damage Shield\",\"Gladiator's Tenacity\",\"Protection\"";

        // source unitId 15907, target unitId 15898
        public const string CombatEventDamage = "75421,COMBAT_EVENT,HOT_TICK_CRITICAL,GENERIC,1,1165,0,46012471,32711,15907,47984/47984,10595/25209,24020/26634,281/500,0/1000,6404,0.7263,0.1815,5.9384,15898,32573/35027,15790/25546,20206/21488,500/500,0/1000,0,0.7263,0.1813,5.8742";

        // source unitId 1 (player), target '*' (self)
        public const string CombatEventSelfPlayer = "80000,COMBAT_EVENT,POWER_ENERGIZE,GENERIC,4,500,0,0,45382,1,10000/10000,5000/5000,5000/5000,100/500,0/1000,0,0.5000,0.5000,1.0000,*";

        // source unitId 15956 (hostile), target '*' (self)
        public const string CombatEventSelfHostile = "80001,COMBAT_EVENT,DOT_TICK,FIRE,1,300,0,0,16340,15956,25500/25500,31942/31942,13731/13731,290/500,0/1000,0,0.7265,0.1804,1.0924,*";

        // source unitId 1 (player), target unitId 15956 (hostile)
        public const string CombatEventPlayerHitsHostile = "80002,COMBAT_EVENT,DAMAGE,PHYSICAL,1,2500,0,0,16415,1,10000/10000,5000/5000,5000/5000,100/500,0/1000,0,0.5000,0.5000,1.0000,15956,20000/20000,0/0,0/0,0/0,0/0,0,0.6000,0.6000,2.0000";

        // source unitId 15956 (hostile), target unitId 1 (player)
        public const string CombatEventHostileHitsPlayer = "80003,COMBAT_EVENT,DAMAGE,PHYSICAL,1,1800,0,0,16415,15956,20000/20000,0/0,0/0,0/0,0/0,0,0.6000,0.6000,2.0000,1,10000/10000,5000/5000,5000/5000,100/500,0/1000,0,0.5000,0.5000,1.0000";

        // source unitId 1 (player), dead target (unitId 0) -> effective target is the source
        public const string CombatEventDeadTarget = "81000,COMBAT_EVENT,DAMAGE,PHYSICAL,1,2500,0,0,16415,1,10000/10000,5000/5000,5000/5000,100/500,0/1000,0,0.5000,0.5000,1.0000,0,0/0,0/0,0/0,0/0,0/0,0,0.0000,0.0000,0.0000";

        // source unitId 1310, target unitId 16001
        public const string EffectChanged = "75300,EFFECT_CHANGED,UPDATED,1,46003497,175660,1310,31015/31015,16531/27357,16270/16270,500/500,1000/1000,6404,0.7260,0.1825,6.0238,16001,34013/34013,27716/27716,21504/21504,500/500,0/1000,0,0.7266,0.1817,2.9955";

        // source unitId 15956 (hostile), target unitId 15956 via '*'
        public const string EffectChangedSelfHostile = "75301,EFFECT_CHANGED,GAINED,1,46003498,175660,15956,20000/20000,0/0,0/0,0/0,0/0,0,0.6000,0.6000,2.0000,*";

        // castTrackId 45940850, source unitId 1 (player), target '*'
        public const string BeginCastPlayer = "23,BEGIN_CAST,0,F,45940850,28541,1,58664/58664,5895/15766,17326/17326,500/500,777/1000,17710,0.7263,0.1814,4.7615,*";

        // castTrackId 45940851, source unitId 15956 (hostile), target '*'
        public const string BeginCastHostile = "24,BEGIN_CAST,1500,F,45940851,12345,15956,20000/20000,0/0,0/0,0/0,0/0,0,0.6000,0.6000,2.0000,*";

        // castTrackId 45940850 (the player cast)
        public const string EndCastPlayer = "25,END_CAST,COMPLETED,45940850,28541";

        // castTrackId 45940851 (the hostile cast)
        public const string EndCastHostile = "26,END_CAST,COMPLETED,45940851,12345";

        // unitId 1 (player)
        public const string HealthRegenPlayer = "219,HEALTH_REGEN,478,1,13911/37358,13786/21060,17791/22018,15/500,0/1000,0,0.7247,0.1980,1.3866";

        // unitId 15956 (hostile)
        public const string HealthRegenHostile = "220,HEALTH_REGEN,478,15956,13911/37358,13786/21060,17791/22018,15/500,0/1000,0,0.7247,0.1980,1.3866";

        public const string UnknownRecordType = "999,SOME_FUTURE_RECORD,1,2,3";

        public const string EndLog = "100000,END_LOG";
    }
}
