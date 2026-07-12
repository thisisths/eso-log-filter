namespace EsoLogFilter.Core.Services
{
    using System.Threading;
    using System.Threading.Tasks;
    using EsoLogFilter.Core.Model.Analysis;

    public interface IFileSummarizer
    {
        LogSummary SummarizeFile(string inputFile);

        Task<LogSummary> SummarizeFileAsync(string inputFile, CancellationToken cancellationToken);
    }
}
