namespace EsoLogFilter.Ui.Avalonia.Model
{
    using global::Avalonia.Media;

    /// <summary>
    /// One preformatted row of the preview tables (unfiltered vs filtered vs delta).
    /// </summary>
    public class SummaryRow
    {
        public SummaryRow(string name, string unfiltered, string filtered, string delta, bool isTotal = false)
        {
            this.Name = name;
            this.Unfiltered = unfiltered;
            this.Filtered = filtered;
            this.Delta = delta;
            this.Weight = isTotal ? FontWeight.Bold : FontWeight.Normal;
        }

        public string Name { get; }

        public string Unfiltered { get; }

        public string Filtered { get; }

        public string Delta { get; }

        public FontWeight Weight { get; }
    }
}
