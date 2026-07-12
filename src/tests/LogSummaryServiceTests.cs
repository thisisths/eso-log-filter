namespace EsoLogFilter.Tests
{
    using EsoLogFilter.Core.Model;
    using EsoLogFilter.Core.Model.Analysis;
    using EsoLogFilter.Core.Model.Objects;
    using EsoLogFilter.Core.Services;

    using Xunit;

    public class LogSummaryServiceTests
    {
        private readonly LogSummaryService service = new LogSummaryService();

        [Fact]
        public void CountsTotalLinesAndRecordTypes()
        {
            var summary = this.Summarize(
                TestLogLines.BeginLog,
                TestLogLines.UnitAddedPlayer,
                TestLogLines.CombatEventPlayerHitsHostile,
                TestLogLines.CombatEventHostileHitsPlayer,
                TestLogLines.UnknownRecordType,
                TestLogLines.EndLog);

            Assert.Equal(6, summary.TotalLines);
            Assert.Equal(1, summary.GetLineCount("BEGIN_LOG"));
            Assert.Equal(1, summary.GetLineCount("UNIT_ADDED"));
            Assert.Equal(2, summary.GetLineCount("COMBAT_EVENT"));
            Assert.Equal(1, summary.GetLineCount("SOME_FUTURE_RECORD"));
            Assert.Equal(0, summary.GetLineCount("EFFECT_CHANGED"));
        }

        [Fact]
        public void CountsUnitsByCategory()
        {
            var summary = this.Summarize(
                TestLogLines.UnitAddedPlayer,
                TestLogLines.UnitAddedPet,
                TestLogLines.UnitAddedHostile,
                TestLogLines.UnitAddedEnemyPet,
                TestLogLines.UnitAddedEnemyPetNameWithComma,
                TestLogLines.UnitAddedObject,
                TestLogLines.UnitAddedSiegeWeapon);

            Assert.Equal(1, summary.GetUnitCount(UnitTypes.Player));
            Assert.Equal(1, summary.GetUnitCount(UnitTypes.MonsterNpcAlly));
            Assert.Equal(1, summary.GetUnitCount(UnitTypes.MonsterHostile));
            Assert.Equal(2, summary.GetUnitCount(UnitTypes.MonsterNpcEnemy));
            Assert.Equal(1, summary.GetUnitCount(UnitTypes.Object));
            Assert.Equal(1, summary.GetUnitCount(UnitTypes.SiegeWeapon));
            Assert.Equal(0, summary.GetUnitCount(UnitTypes.Unknown));
            Assert.Equal(7, summary.GetTotalUnitCount());
        }

        [Fact]
        public void SumsFightCountAndCombatTime()
        {
            var summary = this.Summarize(
                TestLogLines.BeginCombat,
                TestLogLines.EndCombat,
                "100000,BEGIN_COMBAT",
                "130000,END_COMBAT");

            Assert.Equal(2, summary.FightCount);
            Assert.Equal(90_000, summary.CombatTimeMs);
        }

        [Fact]
        public void EndCombatWithoutBegin_IsIgnored()
        {
            var summary = this.Summarize(TestLogLines.EndCombat);

            Assert.Equal(0, summary.FightCount);
            Assert.Equal(0, summary.CombatTimeMs);
        }

        [Fact]
        public void UnclosedFight_CountsButAddsNoCombatTime()
        {
            var summary = this.Summarize(TestLogLines.BeginCombat);

            Assert.Equal(1, summary.FightCount);
            Assert.Equal(0, summary.CombatTimeMs);
        }

        [Fact]
        public void ParsesTheEpochFromBeginLog()
        {
            var summary = this.Summarize(TestLogLines.BeginLog);

            Assert.Equal(1783021379102, summary.EpochMs);
        }

        [Fact]
        public void RecordsFightStartAndEnd()
        {
            var summary = this.Summarize(TestLogLines.BeginCombat, TestLogLines.EndCombat);

            var fight = Assert.Single(summary.Fights);
            Assert.Equal(1, fight.Index);
            Assert.Equal(1000, fight.StartMs);
            Assert.Equal(61000, fight.EndMs);
        }

        [Fact]
        public void DamageIsAttributedToTheSourcePlayer()
        {
            var summary = this.Summarize(
                TestLogLines.UnitAddedPlayer,
                TestLogLines.UnitAddedHostile,
                TestLogLines.CombatEventPlayerHitsHostile);

            Assert.Equal(2500, summary.DamageBySourcePlayer["@janedoe"]);
        }

        [Fact]
        public void PetDamage_CountsTowardTheOwner()
        {
            var summary = this.Summarize(
                TestLogLines.UnitAddedPlayer,
                TestLogLines.UnitAddedPlayerPet,
                TestLogLines.UnitAddedHostile,
                TestLogLines.CombatEventPlayerHitsHostile,
                TestLogLines.CombatEventPetHitsHostile);

            Assert.Equal(2500 + 1200, summary.DamageBySourcePlayer["@janedoe"]);
        }

        [Fact]
        public void AnonymousPlayers_AreDistinguishedByPerSessionId()
        {
            // The same person (perSessionId 2) re-registered under a new unitId,
            // plus a different person (perSessionId 9).
            var samePersonReAdded = "301,UNIT_ADDED,51,PLAYER,F,2,0,F,3,7,\"\",\"\",0,50,810,0,HOSTILE,F";
            var otherAnonymousPlayer = "302,UNIT_ADDED,52,PLAYER,F,9,0,F,1,4,\"\",\"\",0,50,900,0,HOSTILE,F";
            var unit50HitsPlayer = "80006,COMBAT_EVENT,DAMAGE,MAGIC,1,999,0,0,16415,50,20000/20000,0/0,0/0,0/0,0/0,0,0.6000,0.6000,2.0000,1,10000/10000,5000/5000,5000/5000,100/500,0/1000,0,0.5000,0.5000,1.0000";
            var unit51HitsPlayer = "80007,COMBAT_EVENT,DAMAGE,MAGIC,1,999,0,0,16415,51,20000/20000,0/0,0/0,0/0,0/0,0,0.6000,0.6000,2.0000,1,10000/10000,5000/5000,5000/5000,100/500,0/1000,0,0.5000,0.5000,1.0000";
            var unit52HitsPlayer = "80008,COMBAT_EVENT,DAMAGE,MAGIC,1,500,0,0,16415,52,20000/20000,0/0,0/0,0/0,0/0,0,0.6000,0.6000,2.0000,1,10000/10000,5000/5000,5000/5000,100/500,0/1000,0,0.5000,0.5000,1.0000";

            var summary = this.Summarize(
                TestLogLines.UnitAddedPlayer,
                TestLogLines.UnitAddedAnonymousPlayer,
                samePersonReAdded,
                otherAnonymousPlayer,
                unit50HitsPlayer,
                unit51HitsPlayer,
                unit52HitsPlayer);

            Assert.Equal(999 + 999, summary.DamageBySourcePlayer["(anonymous #2)"]);
            Assert.Equal(500, summary.DamageBySourcePlayer["(anonymous #9)"]);
            Assert.False(summary.DamageBySourcePlayer.ContainsKey(LogSummary.AnonymousPlayersKey));
        }

        [Fact]
        public void AnonymousPlayerWithoutPerSessionId_FallsBackToTheSharedBucket()
        {
            var playerWithoutAnyId = "303,UNIT_ADDED,53,PLAYER,F,0,0,F,0,0,\"\",\"\",0,50,810,0,HOSTILE,F";
            var unit53HitsPlayer = "80009,COMBAT_EVENT,DAMAGE,MAGIC,1,777,0,0,16415,53,20000/20000,0/0,0/0,0/0,0/0,0,0.6000,0.6000,2.0000,1,10000/10000,5000/5000,5000/5000,100/500,0/1000,0,0.5000,0.5000,1.0000";

            var summary = this.Summarize(
                TestLogLines.UnitAddedPlayer,
                playerWithoutAnyId,
                unit53HitsPlayer);

            Assert.Equal(777, summary.DamageBySourcePlayer[LogSummary.AnonymousPlayersKey]);
        }

        [Fact]
        public void NonPlayerDamage_LandsInTheSharedBucket()
        {
            var summary = this.Summarize(
                TestLogLines.UnitAddedPlayer,
                TestLogLines.UnitAddedHostile,
                TestLogLines.CombatEventHostileHitsPlayer);

            Assert.Equal(1800, summary.DamageBySourcePlayer[LogSummary.NonPlayerSourcesKey]);
        }

        [Fact]
        public void DamageToAnUnregisteredTarget_DoesNotCount()
        {
            // No UNIT_ADDED for the hostile target — this is what the filtered
            // file looks like, and ESO Logs would not attribute the hit either.
            var summary = this.Summarize(
                TestLogLines.UnitAddedPlayer,
                TestLogLines.CombatEventPlayerHitsHostile);

            Assert.Empty(summary.DamageBySourcePlayer);
        }

        [Fact]
        public void FallDamage_IsNotCounted()
        {
            var summary = this.Summarize(
                TestLogLines.UnitAddedPlayer,
                TestLogLines.CombatEventFallDamage);

            Assert.Empty(summary.DamageBySourcePlayer);
        }

        [Fact]
        public void NonDamageResults_AreNotCounted()
        {
            var summary = this.Summarize(
                TestLogLines.UnitAddedPlayer,
                TestLogLines.CombatEventSelfPlayer);

            Assert.Empty(summary.DamageBySourcePlayer);
        }

        [Fact]
        public void DamageIsScopedToTheCurrentFight()
        {
            var summary = this.Summarize(
                TestLogLines.UnitAddedPlayer,
                TestLogLines.UnitAddedHostile,
                TestLogLines.BeginCombat,
                TestLogLines.CombatEventPlayerHitsHostile,
                TestLogLines.EndCombat,
                TestLogLines.CombatEventPlayerHitsHostile);

            var fight = Assert.Single(summary.Fights);
            Assert.Equal(2500, fight.DamageBySourcePlayer["@janedoe"]);
            Assert.Equal(5000, summary.DamageBySourcePlayer["@janedoe"]);
        }

        [Fact]
        public void Reset_ClearsThePreviousRun()
        {
            this.Summarize(
                TestLogLines.BeginLog,
                TestLogLines.UnitAddedPlayer,
                TestLogLines.UnitAddedHostile,
                TestLogLines.BeginCombat,
                TestLogLines.CombatEventPlayerHitsHostile,
                TestLogLines.EndCombat);

            // The units of the first run are no longer registered, so the same
            // damage event must not count in the second run.
            var summary = this.Summarize(TestLogLines.BeginLog, TestLogLines.CombatEventPlayerHitsHostile);

            Assert.Equal(2, summary.TotalLines);
            Assert.Equal(0, summary.GetTotalUnitCount());
            Assert.Equal(0, summary.FightCount);
            Assert.Equal(0, summary.CombatTimeMs);
            Assert.Empty(summary.Fights);
            Assert.Empty(summary.DamageBySourcePlayer);
        }

        [Fact]
        public void Reset_DoesNotInvalidateAnAlreadyReturnedSummary()
        {
            var first = this.Summarize(TestLogLines.BeginLog, TestLogLines.UnitAddedPlayer);

            this.Summarize(TestLogLines.BeginLog);

            Assert.Equal(2, first.TotalLines);
            Assert.Equal(1, first.GetUnitCount(UnitTypes.Player));
        }

        private LogSummary Summarize(params string[] lines)
        {
            this.service.Reset();

            foreach (var line in lines)
            {
                this.service.ProcessLine(new LogEntry(line));
            }

            return this.service.GetSummary();
        }
    }
}
