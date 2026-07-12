namespace EsoLogFilter.Core.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EsoLogFilter.Core.Model.Objects;

    public interface IFileHandler
    {
        void FilterFileByUnitType(string inputFile, UnitTypes[] unitTypes, string outputFile, bool filterCombatEvents);

        Task FilterFileByUnitTypeAsync(string inputFile, UnitTypes[] unitTypes, string outputFile, bool filterCombatEvents, IProgress<double> progress, CancellationToken cancellationToken);
    }
}
