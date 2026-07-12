namespace EsoLogFilter.Tests
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using EsoLogFilter.Core.Model.Objects;
    using EsoLogFilter.Core.Services;
    using EsoLogFilter.Infrastructure.File.Services;

    using Microsoft.Extensions.Logging.Abstractions;

    using Xunit;

    public class FileSummarizerTests : IDisposable
    {
        private readonly string tempDirectory;
        private readonly FileSummarizer fileSummarizer;

        public FileSummarizerTests()
        {
            this.tempDirectory = Path.Combine(Path.GetTempPath(), "EsoLogFilterTests-" + Guid.NewGuid());
            Directory.CreateDirectory(this.tempDirectory);
            this.fileSummarizer = new FileSummarizer(NullLogger<FileSummarizer>.Instance, new LogSummaryService());
        }

        public void Dispose()
        {
            Directory.Delete(this.tempDirectory, recursive: true);
        }

        [Fact]
        public void SummarizesLinesUnitsAndFileSize()
        {
            var input = this.WriteInputFile(
                TestLogLines.BeginLog,
                TestLogLines.UnitAddedPlayer,
                TestLogLines.UnitAddedHostile,
                TestLogLines.BeginCombat,
                TestLogLines.CombatEventPlayerHitsHostile,
                TestLogLines.EndCombat,
                TestLogLines.EndLog);

            var summary = this.fileSummarizer.SummarizeFile(input);

            Assert.Equal(7, summary.TotalLines);
            Assert.Equal(new FileInfo(input).Length, summary.FileSizeBytes);
            Assert.Equal(1, summary.GetUnitCount(UnitTypes.Player));
            Assert.Equal(1, summary.GetUnitCount(UnitTypes.MonsterHostile));
            Assert.Equal(1, summary.FightCount);
            Assert.Equal(60_000, summary.CombatTimeMs);
        }

        [Fact]
        public async Task AsyncVariant_ProducesTheSameResult()
        {
            var input = this.WriteInputFile(
                TestLogLines.BeginLog,
                TestLogLines.UnitAddedPlayer,
                TestLogLines.CombatEventPlayerHitsHostile);

            var syncSummary = this.fileSummarizer.SummarizeFile(input);
            var asyncSummary = await this.fileSummarizer.SummarizeFileAsync(input, CancellationToken.None);

            Assert.Equal(syncSummary.TotalLines, asyncSummary.TotalLines);
            Assert.Equal(syncSummary.FileSizeBytes, asyncSummary.FileSizeBytes);
            Assert.Equal(syncSummary.GetTotalUnitCount(), asyncSummary.GetTotalUnitCount());
            Assert.Equal(syncSummary.LinesByRecordType, asyncSummary.LinesByRecordType);
        }

        [Fact]
        public void SecondRun_DoesNotLeakCountsFromTheFirstRun()
        {
            var firstInput = this.WriteInputFile(
                TestLogLines.BeginLog,
                TestLogLines.UnitAddedPlayer,
                TestLogLines.UnitAddedHostile);
            this.fileSummarizer.SummarizeFile(firstInput);

            var secondInput = this.WriteInputFile(TestLogLines.BeginLog);
            var summary = this.fileSummarizer.SummarizeFile(secondInput);

            Assert.Equal(1, summary.TotalLines);
            Assert.Equal(0, summary.GetTotalUnitCount());
        }

        private string WriteInputFile(params string[] lines)
        {
            var path = Path.Combine(this.tempDirectory, Guid.NewGuid() + "-input.log");
            File.WriteAllLines(path, lines);
            return path;
        }
    }
}
