namespace EsoLogFilter.Ui.Avalonia.Model
{
    /// <summary>
    /// One entry of the fight list. FightIndex -1 is the "Whole log" scope,
    /// otherwise it is the 1-based fight index from the summary.
    /// </summary>
    public class FightRow
    {
        public FightRow(string label, int fightIndex)
        {
            this.Label = label;
            this.FightIndex = fightIndex;
        }

        public string Label { get; }

        public int FightIndex { get; }
    }
}
