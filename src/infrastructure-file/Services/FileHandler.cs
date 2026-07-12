namespace EsoLogFilter.Infrastructure.File.Services
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using EsoLogFilter.Core.Model;
    using EsoLogFilter.Core.Model.Objects;
    using EsoLogFilter.Core.Services;

    using Microsoft.Extensions.Logging;

    public class FileHandler : IFileHandler
    {
        // Roughly every 2 % on a raid-night log; cheap enough to not matter.
        private const int ProgressReportLineInterval = 50_000;

        private readonly ILogger<FileHandler> logger;
        private readonly IFilterByUnitTypeService filterService;

        public FileHandler(ILogger<FileHandler> logger, IFilterByUnitTypeService filterService)
        {
            this.logger = logger;
            this.filterService = filterService;
        }

        public void FilterFileByUnitType(string inputFile, UnitTypes[] unitTypes, string outputFile, bool filterCombatEvents)
        {
            this.logger.LogInformation($"Start Filtering '{inputFile}' to '{outputFile}'. Filtered by unit type.");

            this.FilterCore(inputFile, unitTypes, outputFile, filterCombatEvents, null, CancellationToken.None);
        }

        public async Task FilterFileByUnitTypeAsync(string inputFile, UnitTypes[] unitTypes, string outputFile, bool filterCombatEvents, IProgress<double> progress, CancellationToken cancellationToken)
        {
            this.logger.LogInformation($"Start Filtering '{inputFile}' to '{outputFile}'. Filtered by unit type.");

            await Task.Run(() => this.FilterCore(inputFile, unitTypes, outputFile, filterCombatEvents, progress, cancellationToken), cancellationToken);
        }

        private void FilterCore(string inputFile, UnitTypes[] unitTypes, string outputFile, bool filterCombatEvents, IProgress<double> progress, CancellationToken cancellationToken)
        {
            this.filterService.Reset();

            FileStream inputFileStream = new FileStream(inputFile, FileMode.Open);
            FileStream outputFileStream = new FileStream(outputFile, FileMode.Create);
            using (StreamReader reader = new StreamReader(inputFileStream))
            using (StreamWriter writer = new StreamWriter(outputFileStream))
            {
                var fileLength = inputFileStream.Length;
                long lineCount = 0;
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var logEntry = new LogEntry(line);

                    if (this.ShouldWrite(logEntry, unitTypes, filterCombatEvents))
                    {
                        writer.WriteLine(logEntry.Line);
                    }

                    // The stream position is the bytes buffered from the file, so
                    // it slightly leads the current line — fine for a progress bar.
                    if (progress != null && ++lineCount % ProgressReportLineInterval == 0 && fileLength > 0)
                    {
                        progress.Report((double)inputFileStream.Position / fileLength);
                    }
                }
            }

            progress?.Report(1);
        }

        private bool ShouldWrite(LogEntry logEntry, UnitTypes[] unitTypes, bool filterCombatEvents)
        {
            switch (logEntry.LineType)
            {
                case LineTypes.UnitAdded:
                    return this.filterService.IsUnitInFilterAndAdd(logEntry, unitTypes);
                case LineTypes.UnitRemoved:
                    return this.filterService.ShouldAddUnitRemoved(logEntry);
                case LineTypes.UnitChanged:
                    return this.filterService.ShouldAddUnitChanged(logEntry);
                case LineTypes.PlayerInfo:
                    return this.filterService.ShouldAddPlayerInfo(logEntry);
                case LineTypes.HealthRegen:
                    return this.filterService.ShouldAddHealthRegen(logEntry);
                case LineTypes.BeginCast:
                    return !filterCombatEvents || this.filterService.ShouldAddBeginCast(logEntry);
                case LineTypes.EndCast:
                    return !filterCombatEvents || this.filterService.ShouldAddEndCast(logEntry);
                case LineTypes.EffectChanged:
                    return !filterCombatEvents || this.filterService.ShouldAddEffectChanged(logEntry);
                case LineTypes.CombatEvent:
                    return !filterCombatEvents || this.filterService.ShouldAddCombatEvent(logEntry);
                default:
                    // Structural lines (BEGIN_LOG, ZONE_CHANGED, ABILITY_INFO, ...) and record
                    // types this app does not know yet are always kept.
                    return true;
            }
        }
    }
}
