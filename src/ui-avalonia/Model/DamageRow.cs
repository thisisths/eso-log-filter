namespace EsoLogFilter.Ui.Avalonia.Model
{
    using global::Avalonia.Media;

    /// <summary>
    /// One preformatted row of the damage table. The bar widths are pixel values
    /// precomputed against the biggest total in the current scope.
    /// </summary>
    public class DamageRow
    {
        public DamageRow(string name, string total, string counted, string removed, double fillWidth, double restWidth, bool isTotal = false)
        {
            this.Name = name;
            this.Total = total;
            this.Counted = counted;
            this.Removed = removed;
            this.FillWidth = fillWidth;
            this.RestWidth = restWidth;
            this.Weight = isTotal ? FontWeight.Bold : FontWeight.Normal;
        }

        public string Name { get; }

        public string Total { get; }

        public string Counted { get; }

        public string Removed { get; }

        public double FillWidth { get; }

        public double RestWidth { get; }

        public FontWeight Weight { get; }
    }
}
