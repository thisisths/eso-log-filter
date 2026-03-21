namespace EsoLogFilter.Core.Services
{
    using EsoLogFilter.Core.Model.Objects;

    public interface IFileHandler
    {
        void FilterFileByUnitType(string inputFile, UnitTypes[] unitTypes, string outputFile);
    }
}
