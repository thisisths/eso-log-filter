namespace EsoLogFilter.Tests
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using EsoLogFilter.Core.Exceptions;
    using EsoLogFilter.Core.Model.Objects;
    using EsoLogFilter.Core.Services;
    using EsoLogFilter.Infrastructure.File.Services;

    using Microsoft.Extensions.Logging.Abstractions;

    using Xunit;

    public class FileHandlerTests : IDisposable
    {
        private static readonly UnitTypes[] PlayersAndPets = { UnitTypes.Player, UnitTypes.MonsterNpcAlly };

        private readonly string tempDirectory;
        private readonly FileHandler fileHandler;

        public FileHandlerTests()
        {
            this.tempDirectory = Path.Combine(Path.GetTempPath(), "EsoLogFilterTests-" + Guid.NewGuid());
            Directory.CreateDirectory(this.tempDirectory);
            this.fileHandler = new FileHandler(NullLogger<FileHandler>.Instance, new FilterByUnitTypeService());
        }

        public void Dispose()
        {
            Directory.Delete(this.tempDirectory, recursive: true);
        }

        [Fact]
        public void DefaultMode_DropsUnitLinesButKeepsAllEvents()
        {
            var input = this.WriteInputFile(
                TestLogLines.BeginLog,
                TestLogLines.UnitAddedPlayer,
                TestLogLines.UnitAddedHostile,
                TestLogLines.CombatEventPlayerHitsHostile,
                TestLogLines.CombatEventHostileHitsPlayer,
                TestLogLines.UnitRemovedHostile,
                TestLogLines.EndLog);
            var output = this.GetOutputPath();

            this.fileHandler.FilterFileByUnitType(input, PlayersAndPets, output, filterCombatEvents: false);

            var result = File.ReadAllLines(output);
            Assert.Equal(
                new[]
                {
                    TestLogLines.BeginLog,
                    TestLogLines.UnitAddedPlayer,
                    TestLogLines.CombatEventPlayerHitsHostile,
                    TestLogLines.CombatEventHostileHitsPlayer,
                    TestLogLines.EndLog,
                },
                result);
        }

        [Fact]
        public void FilterEventsMode_AlsoDropsEventsTargetingDroppedUnits()
        {
            var input = this.WriteInputFile(
                TestLogLines.BeginLog,
                TestLogLines.UnitAddedPlayer,
                TestLogLines.UnitAddedHostile,
                TestLogLines.CombatEventPlayerHitsHostile,
                TestLogLines.CombatEventHostileHitsPlayer,
                TestLogLines.BeginCastHostile,
                TestLogLines.EndCastHostile,
                TestLogLines.EffectChangedSelfHostile,
                TestLogLines.UnitRemovedHostile,
                TestLogLines.EndLog);
            var output = this.GetOutputPath();

            this.fileHandler.FilterFileByUnitType(input, PlayersAndPets, output, filterCombatEvents: true);

            var result = File.ReadAllLines(output);
            Assert.Equal(
                new[]
                {
                    TestLogLines.BeginLog,
                    TestLogLines.UnitAddedPlayer,
                    TestLogLines.CombatEventHostileHitsPlayer,
                    TestLogLines.EndLog,
                },
                result);
        }

        [Fact]
        public void UnknownRecordTypes_ArePassedThrough()
        {
            var input = this.WriteInputFile(TestLogLines.BeginLog, TestLogLines.UnknownRecordType, TestLogLines.EndLog);
            var output = this.GetOutputPath();

            this.fileHandler.FilterFileByUnitType(input, PlayersAndPets, output, filterCombatEvents: true);

            Assert.Contains(TestLogLines.UnknownRecordType, File.ReadAllLines(output));
        }

        [Fact]
        public void ExistingOutputFile_IsTruncatedNotAppendedTo()
        {
            var input = this.WriteInputFile(TestLogLines.BeginLog);
            var output = this.GetOutputPath();
            File.WriteAllText(output, new string('x', 10_000));

            this.fileHandler.FilterFileByUnitType(input, PlayersAndPets, output, filterCombatEvents: false);

            Assert.Equal(new[] { TestLogLines.BeginLog }, File.ReadAllLines(output));
        }

        [Fact]
        public void SecondRun_DoesNotLeakUnitIdsFromFirstRun()
        {
            var firstInput = this.WriteInputFile(TestLogLines.BeginLog, TestLogLines.UnitAddedPlayer);
            var firstOutput = this.GetOutputPath();
            this.fileHandler.FilterFileByUnitType(firstInput, PlayersAndPets, firstOutput, filterCombatEvents: false);

            // The second log never adds unit 1, so its UNIT_REMOVED must be dropped
            // even though the first run kept that id.
            var secondInput = this.WriteInputFile(TestLogLines.BeginLog, TestLogLines.UnitRemovedPlayer);
            var secondOutput = this.GetOutputPath();
            this.fileHandler.FilterFileByUnitType(secondInput, PlayersAndPets, secondOutput, filterCombatEvents: false);

            Assert.Equal(new[] { TestLogLines.BeginLog }, File.ReadAllLines(secondOutput));
        }

        [Fact]
        public async Task AsyncVariant_ProducesTheSameOutput()
        {
            var input = this.WriteInputFile(
                TestLogLines.BeginLog,
                TestLogLines.UnitAddedPlayer,
                TestLogLines.UnitAddedHostile,
                TestLogLines.CombatEventPlayerHitsHostile);
            var syncOutput = this.GetOutputPath();
            var asyncOutput = this.GetOutputPath();

            this.fileHandler.FilterFileByUnitType(input, PlayersAndPets, syncOutput, filterCombatEvents: true);
            await this.fileHandler.FilterFileByUnitTypeAsync(input, PlayersAndPets, asyncOutput, filterCombatEvents: true, progress: null, CancellationToken.None);

            Assert.Equal(File.ReadAllLines(syncOutput), File.ReadAllLines(asyncOutput));
        }

        [Fact]
        public async Task ReportsProgressEndingAtOneHundredPercent()
        {
            var input = this.WriteInputFile(TestLogLines.BeginLog, TestLogLines.UnitAddedPlayer);
            var output = this.GetOutputPath();
            var progress = new TestProgress();

            await this.fileHandler.FilterFileByUnitTypeAsync(input, PlayersAndPets, output, filterCombatEvents: false, progress, CancellationToken.None);

            Assert.NotEmpty(progress.Reports);
            Assert.Equal(1.0, progress.Reports[^1]);
            Assert.All(progress.Reports, fraction => Assert.InRange(fraction, 0.0, 1.0));
        }

        [Fact]
        public void ReadOnlySourceFile_CanBeFiltered()
        {
            var input = this.WriteInputFile(TestLogLines.BeginLog);
            var output = this.GetOutputPath();
            File.SetAttributes(input, FileAttributes.ReadOnly);

            try
            {
                this.fileHandler.FilterFileByUnitType(input, PlayersAndPets, output, filterCombatEvents: false);
            }
            finally
            {
                // A read-only file would break the recursive cleanup in Dispose.
                File.SetAttributes(input, FileAttributes.Normal);
            }

            Assert.Equal(new[] { TestLogLines.BeginLog }, File.ReadAllLines(output));
        }

        [Fact]
        public void LockedSourceFile_ThrowsFileInUseExceptionForTheSource()
        {
            if (!OperatingSystem.IsWindows())
            {
                return; // POSIX file systems do not enforce mandatory locks.
            }

            var input = this.WriteInputFile(TestLogLines.BeginLog);
            var output = this.GetOutputPath();

            // Simulates the game still writing the encounter log.
            using (new FileStream(input, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                var exception = Assert.Throws<FileInUseException>(
                    () => this.fileHandler.FilterFileByUnitType(input, PlayersAndPets, output, filterCombatEvents: false));

                Assert.Equal(input, exception.FilePath);
                Assert.Contains("source file", exception.Message);
                Assert.Contains("/encounterlog", exception.Message);
            }
        }

        [Fact]
        public void LockedTargetFile_ThrowsFileInUseExceptionForTheTarget()
        {
            if (!OperatingSystem.IsWindows())
            {
                return; // POSIX file systems do not enforce mandatory locks.
            }

            var input = this.WriteInputFile(TestLogLines.BeginLog);
            var output = this.GetOutputPath();

            using (new FileStream(output, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                var exception = Assert.Throws<FileInUseException>(
                    () => this.fileHandler.FilterFileByUnitType(input, PlayersAndPets, output, filterCombatEvents: false));

                Assert.Equal(output, exception.FilePath);
                Assert.Contains("target file", exception.Message);
            }
        }

        private string WriteInputFile(params string[] lines)
        {
            var path = Path.Combine(this.tempDirectory, Guid.NewGuid() + "-input.log");
            File.WriteAllLines(path, lines);
            return path;
        }

        private string GetOutputPath()
        {
            return Path.Combine(this.tempDirectory, Guid.NewGuid() + "-output.log");
        }
    }
}
