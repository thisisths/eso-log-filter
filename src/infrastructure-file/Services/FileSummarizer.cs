namespace EsoLogFilter.Infrastructure.File.Services
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using EsoLogFilter.Core.Model;
    using EsoLogFilter.Core.Model.Analysis;
    using EsoLogFilter.Core.Services;

    using Microsoft.Extensions.Logging;

    public class FileSummarizer : IFileSummarizer
    {
        // Roughly every 2 % on a raid-night log; cheap enough to not matter.
        private const int ProgressReportLineInterval = 50_000;

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

            return this.SummarizeCore(inputFile, null, CancellationToken.None);
        }

        public async Task<LogSummary> SummarizeFileAsync(string inputFile, IProgress<double> progress, CancellationToken cancellationToken)
        {
            this.logger.LogInformation($"Start summarizing '{inputFile}'.");

            return await Task.Run(() => this.SummarizeCore(inputFile, progress, cancellationToken), cancellationToken);
        }

        private LogSummary SummarizeCore(string inputFile, IProgress<double> progress, CancellationToken cancellationToken)
        {
            this.summaryService.Reset();

            // Read-only access, so a file that is still being written or is
            // archived read-only can be summarized too.
            FileStream inputFileStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using (StreamReader reader = new StreamReader(inputFileStream))
            {
                var fileLength = inputFileStream.Length;
                long lineCount = 0;
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    this.summaryService.ProcessLine(new LogEntry(line));

                    // The stream position is the bytes buffered from the file, so
                    // it slightly leads the current line — fine for a progress bar.
                    if (progress != null && ++lineCount % ProgressReportLineInterval == 0 && fileLength > 0)
                    {
                        progress.Report((double)inputFileStream.Position / fileLength);
                    }
                }
            }

            progress?.Report(1);

            var summary = this.summaryService.GetSummary();
            summary.FileSizeBytes = new FileInfo(inputFile).Length;
            return summary;
        }
    }
}
