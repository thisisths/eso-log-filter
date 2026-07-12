namespace EsoLogFilter.Infrastructure.File.Services
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using EsoLogFilter.Core.Model;
    using EsoLogFilter.Core.Model.Analysis;
    using EsoLogFilter.Core.Services;

    using Microsoft.Extensions.Logging;

    public class FileSummarizer : IFileSummarizer
    {
        private readonly ILogger<FileSummarizer> logger;
        private readonly ILogSummaryService summaryService;

        public FileSummarizer(ILogger<FileSummarizer> logger, ILogSummaryService summaryService)
        {
            this.logger = logger;
            this.summaryService = summaryService;
        }

        public LogSummary SummarizeFile(string inputFile)
        {
            this.logger.LogInformation($"Start summarizing '{inputFile}'.");

            return this.SummarizeCore(inputFile, CancellationToken.None);
        }

        public async Task<LogSummary> SummarizeFileAsync(string inputFile, CancellationToken cancellationToken)
        {
            this.logger.LogInformation($"Start summarizing '{inputFile}'.");

            return await Task.Run(() => this.SummarizeCore(inputFile, cancellationToken), cancellationToken);
        }

        private LogSummary SummarizeCore(string inputFile, CancellationToken cancellationToken)
        {
            this.summaryService.Reset();

            // Read-only access, so a file that is still being written or is
            // archived read-only can be summarized too.
            FileStream inputFileStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using (StreamReader reader = new StreamReader(inputFileStream))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    this.summaryService.ProcessLine(new LogEntry(line));
                }
            }

            var summary = this.summaryService.GetSummary();
            summary.FileSizeBytes = new FileInfo(inputFile).Length;
            return summary;
        }
    }
}
