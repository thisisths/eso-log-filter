namespace EsoLogFilter.Core.Services
{
    using EsoLogFilter.Core.Model;
    using EsoLogFilter.Core.Model.Analysis;

    public interface ILogSummaryService
    {
        void Reset();

        void ProcessLine(LogEntry logEntry);

        LogSummary GetSummary();
    }
}
