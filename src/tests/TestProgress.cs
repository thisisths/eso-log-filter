namespace EsoLogFilter.Tests
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Synchronous IProgress implementation — the built-in Progress&lt;T&gt; posts
    /// to a synchronization context, which makes test assertions racy.
    /// </summary>
    public class TestProgress : IProgress<double>
    {
        public List<double> Reports { get; } = new List<double>();

        public void Report(double value)
        {
            this.Reports.Add(value);
        }
    }
}
