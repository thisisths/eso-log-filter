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
        public void Reset_ClearsThePreviousRun()
        {
            this.Summarize(
                TestLogLines.BeginLog,
                TestLogLines.UnitAddedPlayer,
                TestLogLines.BeginCombat,
                TestLogLines.EndCombat);

            var summary = this.Summarize(TestLogLines.BeginLog);

            Assert.Equal(1, summary.TotalLines);
            Assert.Equal(0, summary.GetTotalUnitCount());
            Assert.Equal(0, summary.FightCount);
            Assert.Equal(0, summary.CombatTimeMs);
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
