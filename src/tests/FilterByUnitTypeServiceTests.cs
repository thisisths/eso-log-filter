namespace EsoLogFilter.Tests
{
    using EsoLogFilter.Core.Model;
    using EsoLogFilter.Core.Model.Objects;
    using EsoLogFilter.Core.Services;

    using Xunit;

    public class FilterByUnitTypeServiceTests
    {
        private static readonly UnitTypes[] PlayersAndPets = { UnitTypes.Player, UnitTypes.MonsterNpcAlly };

        private readonly FilterByUnitTypeService service = new FilterByUnitTypeService();

        [Fact]
        public void IsUnitInFilterAndAdd_KeepsSelectedTypes_DropsOthers()
        {
            Assert.True(this.service.IsUnitInFilterAndAdd(new LogEntry(TestLogLines.UnitAddedPlayer), PlayersAndPets));
            Assert.True(this.service.IsUnitInFilterAndAdd(new LogEntry(TestLogLines.UnitAddedPet), PlayersAndPets));
            Assert.False(this.service.IsUnitInFilterAndAdd(new LogEntry(TestLogLines.UnitAddedHostile), PlayersAndPets));
            Assert.False(this.service.IsUnitInFilterAndAdd(new LogEntry(TestLogLines.UnitAddedObject), PlayersAndPets));
        }

        [Fact]
        public void IsUnitInFilterAndAdd_KeepsUnknownUnitTypes()
        {
            var unknownUnit = new LogEntry("100,UNIT_ADDED,20001,SOME_FUTURE_UNIT,F,0,0,F,0,0,\"\",\"\",0,50,160,0,HOSTILE,F");

            Assert.True(this.service.IsUnitInFilterAndAdd(unknownUnit, PlayersAndPets));
        }

        [Fact]
        public void UnitScopedLines_AreKeptOnlyForKeptUnits()
        {
            this.service.IsUnitInFilterAndAdd(new LogEntry(TestLogLines.UnitAddedPlayer), PlayersAndPets);
            this.service.IsUnitInFilterAndAdd(new LogEntry(TestLogLines.UnitAddedHostile), PlayersAndPets);

            Assert.True(this.service.ShouldAddUnitRemoved(new LogEntry(TestLogLines.UnitRemovedPlayer)));
            Assert.False(this.service.ShouldAddUnitRemoved(new LogEntry(TestLogLines.UnitRemovedHostile)));
            Assert.True(this.service.ShouldAddPlayerInfo(new LogEntry(TestLogLines.PlayerInfo)));
            Assert.True(this.service.ShouldAddHealthRegen(new LogEntry(TestLogLines.HealthRegenPlayer)));
            Assert.False(this.service.ShouldAddHealthRegen(new LogEntry(TestLogLines.HealthRegenHostile)));
        }

        [Fact]
        public void Reset_ClearsAllState()
        {
            this.service.IsUnitInFilterAndAdd(new LogEntry(TestLogLines.UnitAddedPlayer), PlayersAndPets);

            this.service.Reset();

            Assert.False(this.service.ShouldAddUnitRemoved(new LogEntry(TestLogLines.UnitRemovedPlayer)));
        }

        [Fact]
        public void ShouldAddCombatEvent_DropsEventsTargetingDroppedUnits()
        {
            this.service.IsUnitInFilterAndAdd(new LogEntry(TestLogLines.UnitAddedPlayer), PlayersAndPets);
            this.service.IsUnitInFilterAndAdd(new LogEntry(TestLogLines.UnitAddedHostile), PlayersAndPets);

            // player -> hostile: the target was deselected, drop it
            Assert.False(this.service.ShouldAddCombatEvent(new LogEntry(TestLogLines.CombatEventPlayerHitsHostile)));

            // hostile -> player: the target is kept
            Assert.True(this.service.ShouldAddCombatEvent(new LogEntry(TestLogLines.CombatEventHostileHitsPlayer)));

            // hostile on itself ('*'): effective target is the dropped source
            Assert.False(this.service.ShouldAddCombatEvent(new LogEntry(TestLogLines.CombatEventSelfHostile)));

            // player on itself ('*')
            Assert.True(this.service.ShouldAddCombatEvent(new LogEntry(TestLogLines.CombatEventSelfPlayer)));
        }

        [Fact]
        public void ShouldAddCombatEvent_KeepsEventsReferencingUnknownUnits()
        {
            // Source 15907 and target 15898 were never registered via UNIT_ADDED.
            Assert.True(this.service.ShouldAddCombatEvent(new LogEntry(TestLogLines.CombatEventDamage)));
        }

        [Fact]
        public void ShouldAddEffectChanged_UsesEffectiveTarget()
        {
            this.service.IsUnitInFilterAndAdd(new LogEntry(TestLogLines.UnitAddedHostile), PlayersAndPets);

            Assert.False(this.service.ShouldAddEffectChanged(new LogEntry(TestLogLines.EffectChangedSelfHostile)));
            Assert.True(this.service.ShouldAddEffectChanged(new LogEntry(TestLogLines.EffectChanged)));
        }

        [Fact]
        public void EndCast_FollowsTheVerdictOfItsBeginCast()
        {
            this.service.IsUnitInFilterAndAdd(new LogEntry(TestLogLines.UnitAddedPlayer), PlayersAndPets);
            this.service.IsUnitInFilterAndAdd(new LogEntry(TestLogLines.UnitAddedHostile), PlayersAndPets);

            Assert.True(this.service.ShouldAddBeginCast(new LogEntry(TestLogLines.BeginCastPlayer)));
            Assert.False(this.service.ShouldAddBeginCast(new LogEntry(TestLogLines.BeginCastHostile)));

            Assert.True(this.service.ShouldAddEndCast(new LogEntry(TestLogLines.EndCastPlayer)));
            Assert.False(this.service.ShouldAddEndCast(new LogEntry(TestLogLines.EndCastHostile)));
        }

        [Fact]
        public void EndCast_WithoutSeenBeginCast_IsKept()
        {
            Assert.True(this.service.ShouldAddEndCast(new LogEntry(TestLogLines.EndCastPlayer)));
        }
    }
}
