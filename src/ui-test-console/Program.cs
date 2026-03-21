namespace EsoLogFilter.Ui.TestConsole
{
    using EsoLogFilter.Core;
    using EsoLogFilter.Core.Model.Objects;
    using EsoLogFilter.Core.Services;
    using EsoLogFilter.Infrastructure.File;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    class Program
    {
        static void Main(string[] args)
        {
            //setup our DI
            var serviceCollection = new ServiceCollection()
                .AddCore()
                .AddInfrastructureFile()
                .AddLogging(cfg => cfg.AddConsole()) ;

            var serviceProvider = serviceCollection
                .BuildServiceProvider();

            var logger = serviceProvider.GetService<ILoggerFactory>()
                .CreateLogger<Program>();
            logger.LogDebug("Starting application");

            var fileHandler = serviceProvider.GetService<IFileHandler>();
            fileHandler.FilterFileByUnitType("D:\\OneDrive\\Dokumente\\Elder Scrolls Online\\live\\Logs\\Encounter.log", new[] { UnitTypes.Player, UnitTypes.MonsterNpcAlly }, "D:\\OneDrive\\Dokumente\\Elder Scrolls Online\\live\\Logs\\FilteredLog.log");

            logger.LogDebug("All done!");
        }
    }
}
