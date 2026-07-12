namespace EsoLogFilter.Tests
{
    using EsoLogFilter.Core.Model;
    using EsoLogFilter.Core.Model.Objects;

    using Xunit;

    public class LogEntryTests
    {
        [Theory]
        [InlineData(TestLogLines.BeginLog, LineTypes.BeginLog)]
        [InlineData(TestLogLines.ZoneChanged, LineTypes.ZoneChanged)]
        [InlineData(TestLogLines.UnitAddedPlayer, LineTypes.UnitAdded)]
        [InlineData(TestLogLines.UnitRemovedHostile, LineTypes.UnitRemoved)]
        [InlineData(TestLogLines.UnitChangedPet, LineTypes.UnitChanged)]
        [InlineData(TestLogLines.PlayerInfo, LineTypes.PlayerInfo)]
        [InlineData(TestLogLines.AbilityInfoScribed, LineTypes.AbilityInfo)]
        [InlineData(TestLogLines.CombatEventDamage, LineTypes.CombatEvent)]
        [InlineData(TestLogLines.EffectChanged, LineTypes.EffectChanged)]
        [InlineData(TestLogLines.BeginCastPlayer, LineTypes.BeginCast)]
        [InlineData(TestLogLines.EndCastPlayer, LineTypes.EndCast)]
        [InlineData(TestLogLines.HealthRegenPlayer, LineTypes.HealthRegen)]
        [InlineData(TestLogLines.EndLog, LineTypes.EndLog)]
        public void SetsLineType(string line, LineTypes expected)
        {
            Assert.Equal(expected, new LogEntry(line).LineType);
        }

        [Fact]
        public void UnknownRecordType_IsUnknownInsteadOfThrowing()
        {
            Assert.Equal(LineTypes.Unknown, new LogEntry(TestLogLines.UnknownRecordType).LineType);
        }

        [Theory]
        [InlineData(TestLogLines.UnitAddedPlayer, "1")]
        [InlineData(TestLogLines.UnitRemovedHostile, "15956")]
        [InlineData(TestLogLines.UnitChangedPet, "15908")]
        [InlineData(TestLogLines.PlayerInfo, "1")]
        [InlineData(TestLogLines.HealthRegenPlayer, "1")]
        public void GetIdString_ReturnsUnitId(string line, string expected)
        {
            Assert.Equal(expected, new LogEntry(line).GetIdString());
        }

        [Theory]
        [InlineData(TestLogLines.UnitAddedPlayer, UnitTypes.Player)]
        [InlineData(TestLogLines.UnitAddedPet, UnitTypes.MonsterNpcAlly)]
        [InlineData(TestLogLines.UnitAddedHostile, UnitTypes.MonsterHostile)]
        [InlineData(TestLogLines.UnitAddedHostileNameWithComma, UnitTypes.MonsterHostile)]
        [InlineData(TestLogLines.UnitAddedEnemyPet, UnitTypes.MonsterNpcEnemy)]
        [InlineData(TestLogLines.UnitAddedEnemyPetNameWithComma, UnitTypes.MonsterNpcEnemy)]
        [InlineData(TestLogLines.UnitAddedObject, UnitTypes.Object)]
        [InlineData(TestLogLines.UnitAddedSiegeWeapon, UnitTypes.SiegeWeapon)]
        public void GetUnitType_ClassifiesUnit(string line, UnitTypes expected)
        {
            Assert.Equal(expected, new LogEntry(line).GetUnitType());
        }

        [Fact]
        public void GetUnitType_UnknownUnitType_IsUnknownInsteadOfThrowing()
        {
            var line = "100,UNIT_ADDED,20001,SOME_FUTURE_UNIT,F,0,0,F,0,0,\"\",\"\",0,50,160,0,HOSTILE,F";

            Assert.Equal(UnitTypes.Unknown, new LogEntry(line).GetUnitType());
        }

        [Fact]
        public void GetUnitType_UnknownMonsterReaction_IsUnknownInsteadOfThrowing()
        {
            var line = "100,UNIT_ADDED,20002,MONSTER,F,0,0,F,0,0,\"\",\"\",0,50,160,0,SOME_FUTURE_REACTION,F";

            Assert.Equal(UnitTypes.Unknown, new LogEntry(line).GetUnitType());
        }

        [Theory]
        [InlineData(TestLogLines.CombatEventDamage, "15907")]
        [InlineData(TestLogLines.EffectChanged, "1310")]
        [InlineData(TestLogLines.BeginCastPlayer, "1")]
        public void GetSourceIdString_ReturnsSourceUnitId(string line, string expected)
        {
            Assert.Equal(expected, new LogEntry(line).GetSourceIdString());
        }

        [Theory]
        [InlineData(TestLogLines.CombatEventDamage, "15898")]
        [InlineData(TestLogLines.CombatEventSelfPlayer, "*")]
        [InlineData(TestLogLines.EffectChanged, "16001")]
        [InlineData(TestLogLines.EffectChangedSelfHostile, "*")]
        [InlineData(TestLogLines.BeginCastPlayer, "*")]
        public void GetTargetIdString_ReturnsTargetToken(string line, string expected)
        {
            Assert.Equal(expected, new LogEntry(line).GetTargetIdString());
        }

        [Theory]
        [InlineData(TestLogLines.CombatEventDamage, "15898")] // real target
        [InlineData(TestLogLines.CombatEventSelfPlayer, "1")] // '*' falls back to the source
        [InlineData(TestLogLines.CombatEventDeadTarget, "1")] // unit 0 falls back to the source
        [InlineData(TestLogLines.EffectChangedSelfHostile, "15956")]
        public void GetEffectiveTargetIdString_FallsBackToSource(string line, string expected)
        {
            Assert.Equal(expected, new LogEntry(line).GetEffectiveTargetIdString());
        }

        [Theory]
        [InlineData(TestLogLines.BeginCastPlayer, "45940850")]
        [InlineData(TestLogLines.EndCastPlayer, "45940850")]
        [InlineData(TestLogLines.BeginCastHostile, "45940851")]
        [InlineData(TestLogLines.EndCastHostile, "45940851")]
        public void GetCastTrackId_ReturnsCastTrackId(string line, string expected)
        {
            Assert.Equal(expected, new LogEntry(line).GetCastTrackId());
        }
    }
}
