namespace EsoLogFilter.Ui.Avalonia.Helper
{
    using System;
    using System.IO;
    using System.Reflection;

    internal static class ErrorReporter
    {
        private const string FileName = "error.log";

        /// <summary>
        /// Appends the exception details to error.log next to the app (temp folder as
        /// fallback) and returns the full path, or null when no location was writable.
        /// Never throws — this runs inside catch blocks.
        /// </summary>
        public static string TryWriteErrorLog(Exception exception, string context)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var entry =
                $"=== {DateTime.Now:yyyy-MM-dd HH:mm:ss} (v{version}) ==={Environment.NewLine}" +
                $"Context: {context}{Environment.NewLine}" +
                exception + Environment.NewLine + Environment.NewLine;

            // The app folder may be read-only (e.g. Program Files, macOS /Applications).
            foreach (var directory in new[] { AppContext.BaseDirectory, Path.GetTempPath() })
            {
                try
                {
                    var path = Path.Combine(directory, FileName);
                    File.AppendAllText(path, entry);
                    return path;
                }
                catch
                {
                    // Try the next location.
                }
            }

            return null;
        }
    }
}
