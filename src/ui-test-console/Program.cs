namespace EsoLogFilter.Ui.TestConsole
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EsoLogFilter.Core;
    using EsoLogFilter.Core.Model.Objects;
    using EsoLogFilter.Core.Services;
    using EsoLogFilter.Infrastructure.File;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    class Program
    {
        static async Task<int> Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: Ui.TestConsole <inputFile> <outputFile> [--filter-events] [--units=Player,MonsterNpcAlly]");
                Console.WriteLine($"Known units: {string.Join(", ", Enum.GetNames<UnitTypes>().Where(n => n != nameof(UnitTypes.Unknown)))}");
                return 1;
            }

            var inputFile = args[0];
            var outputFile = args[1];
            var filterCombatEvents = args.Contains("--filter-events");
            var unitTypes = ParseUnitTypes(args) ?? new[] { UnitTypes.Player, UnitTypes.MonsterNpcAlly };

            //setup our DI
            using var serviceProvider = new ServiceCollection()
                .AddCore()
                .AddInfrastructureFile()
                .AddLogging(cfg => cfg.AddConsole())
                .BuildServiceProvider();

            var logger = serviceProvider.GetService<ILoggerFactory>()
                .CreateLogger<Program>();
            logger.LogInformation($"Units: [{string.Join(", ", unitTypes)}], filterCombatEvents: {filterCombatEvents}");

            var fileHandler = serviceProvider.GetService<IFileHandler>();
            await fileHandler.FilterFileByUnitTypeAsync(inputFile, unitTypes, outputFile, filterCombatEvents, CancellationToken.None);

            logger.LogInformation("All done!");
            return 0;
        }

        private static UnitTypes[] ParseUnitTypes(string[] args)
        {
            var unitsArg = args.FirstOrDefault(a => a.StartsWith("--units="));

            if (unitsArg == null)
            {
                return null;
            }

            return unitsArg.Substring("--units=".Length)
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(name => Enum.Parse<UnitTypes>(name, ignoreCase: true))
                .ToArray();
        }
    }
}
