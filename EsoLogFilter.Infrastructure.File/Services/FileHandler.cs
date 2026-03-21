namespace EsoLogFilter.Infrastructure.File.Services
{
    using System;
    using System.IO;
    using EsoLogFilter.Core.Model;
    using EsoLogFilter.Core.Model.Objects;
    using EsoLogFilter.Core.Services;

    using Microsoft.Extensions.Logging;

    public class FileHandler : IFileHandler
    {
        private readonly ILogger<FileHandler> logger;
        private readonly IFilterByUnitTypeService filterService;

        public FileHandler(ILogger<FileHandler> logger, IFilterByUnitTypeService filterService)
        {
            this.logger = logger;
            this.filterService = filterService;
        }

        public void FilterFileByUnitType(string inputFile, UnitTypes[] unitTypes, string outputFile)
        {
            this.logger.LogInformation($"Start Filtering '{inputFile}' to '{outputFile}'. Filtered by unit type.");


            FileStream inputFileStream = new FileStream(inputFile, FileMode.Open);
            FileStream outputFileStream = new FileStream(outputFile, FileMode.OpenOrCreate);
            using (StreamReader reader = new StreamReader(inputFileStream))
            using (StreamWriter writer = new StreamWriter(outputFileStream))
            {
                int counter = 0;
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    var logEntry = new LogEntry(line);

                    switch (logEntry.LineType)
                    {
                        case LineTypes.BeginLog:
                        case LineTypes.ZoneChanged:
                        case LineTypes.BeginCombat:
                        case LineTypes.EndCombat:
                        case LineTypes.AbilityInfo:
                        case LineTypes.EffectInfo:
                        case LineTypes.MapChanged:
                        case LineTypes.EndLog:
                            writer.WriteLine(logEntry.Line);
                            break;
                        case LineTypes.UnitAdded:
                            this.HandleUnitAdded(logEntry, unitTypes, writer);
                            break;
                        case LineTypes.UnitRemoved:
                            this.HandleUnitRemoved(logEntry, writer);
                            break;
                        case LineTypes.UnitChanged:
                            this.HandleUnitChanged(logEntry, writer);
                            break;
                        case LineTypes.PlayerInfo:
                            this.HandlePlayerInfo(logEntry, writer);
                            break;
                        case LineTypes.BeginCast:
                            this.HandleBeginCast(logEntry, writer);
                            break;
                        case LineTypes.EndCast:
                            this.HandleEndCast(logEntry, writer);
                            break;
                        case LineTypes.EffectChanged:
                            this.HandleEffectChanged(logEntry, writer);
                            break;
                        case LineTypes.CombatEvent:
                            this.HandleCombatEvent(logEntry, writer);
                            break;
                        case LineTypes.HealthRegen:
                            this.HandleHealthRegen(logEntry, writer);
                            break;
                        default:
                            throw new Exception("Line type unknown");
                    }

                    counter++;
                }
            }
        }

        private void HandleUnitAdded(LogEntry logEntry, UnitTypes[] unitTypes, StreamWriter writer)
        {
            if (this.filterService.IsUnitInFilterAndAdd(logEntry, unitTypes))
            {
                writer.WriteLine(logEntry.Line);
            }
        }

        private void HandleUnitRemoved(LogEntry logEntry, StreamWriter writer)
        {
            if (this.filterService.ShouldAddUnitRemoved(logEntry))
            {
                writer.WriteLine(logEntry.Line);
            }
        }

        private void HandleUnitChanged(LogEntry logEntry, StreamWriter writer)
        {
            if (this.filterService.ShouldAddUnitChanged(logEntry))
            {
                writer.WriteLine(logEntry.Line);
            }
        }

        private void HandlePlayerInfo(LogEntry logEntry, StreamWriter writer)
        {
            if (this.filterService.ShouldAddPlayerInfo(logEntry))
            {
                writer.WriteLine(logEntry.Line);
            }
        }

        private void HandleBeginCast(LogEntry logEntry, StreamWriter writer)
        {
            if (this.filterService.ShouldAddBeginCast(logEntry))
            {
                writer.WriteLine(logEntry.Line);
            }
        }

        private void HandleEndCast(LogEntry logEntry, StreamWriter writer)
        {
            if (this.filterService.ShouldAddEndCast(logEntry))
            {
                writer.WriteLine(logEntry.Line);
            }
        }

        private void HandleEffectChanged(LogEntry logEntry, StreamWriter writer)
        {
            if (this.filterService.ShouldAddEffectChanged(logEntry))
            {
                writer.WriteLine(logEntry.Line);
            }
        }

        private void HandleCombatEvent(LogEntry logEntry, StreamWriter writer)
        {
            if (this.filterService.ShouldAddCombatEvent(logEntry))
            {
                writer.WriteLine(logEntry.Line);
            }
        }

        private void HandleHealthRegen(LogEntry logEntry, StreamWriter writer)
        {
            if (this.filterService.ShouldAddHealthRegen(logEntry))
            {
                writer.WriteLine(logEntry.Line);
            }
        }
    }
}
