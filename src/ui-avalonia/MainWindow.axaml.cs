namespace EsoLogFilter.Ui.Avalonia
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using global::Avalonia.Controls;
    using global::Avalonia.Interactivity;
    using global::Avalonia.Media;
    using global::Avalonia.Platform.Storage;
    using EsoLogFilter.Core.Model.Analysis;
    using EsoLogFilter.Core.Model.Objects;
    using EsoLogFilter.Core.Services;
    using EsoLogFilter.Ui.Avalonia.Helper;
    using EsoLogFilter.Ui.Avalonia.Model;

    public partial class MainWindow : Window
    {
        private static readonly (UnitTypes Type, string Name)[] UnitCategories =
        {
            (UnitTypes.Player, "Players"),
            (UnitTypes.MonsterHostile, "Monster - Hostile"),
            (UnitTypes.MonsterNpcAlly, "Monster - Npc Ally (Pet)"),
            (UnitTypes.MonsterNpcEnemy, "Monster - Npc Enemy (Pet)"),
            (UnitTypes.MonsterFriendly, "Monster - Friendly"),
            (UnitTypes.MonsterNeutral, "Monster - Neutral"),
            (UnitTypes.Object, "Object"),
            (UnitTypes.SiegeWeapon, "Siege weapon"),
            (UnitTypes.Unknown, "Unknown"),
        };

        private const double DamageBarWidth = 120.0;

        private readonly IFileHandler fileHandler;
        private readonly IFileSummarizer fileSummarizer;
        private CancellationTokenSource cancellationTokenSource;
        private CancellationTokenSource analyzeCancellationTokenSource;
        private LogSummary previewUnfiltered;
        private LogSummary previewFiltered;

        public MainWindow(IFileHandler fileHandler, IFileSummarizer fileSummarizer)
        {
            InitializeComponent();
            this.fileHandler = fileHandler;
            this.fileSummarizer = fileSummarizer;
        }

        private async void btnSourceFile_Click(object sender, RoutedEventArgs e)
        {
            var file = await this.OpenLogFileAsync("Select Source File");

            if (file != null)
            {
                this.tbSourceFile.Text = file;
            }
        }

        private async void btnTargetFile_Click(object sender, RoutedEventArgs e)
        {
            var file = await this.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Select Target File",
                DefaultExtension = "log",
                SuggestedFileName = "Encounter-filtered",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("LogFiles") { Patterns = new[] { "*.log" } }
                }
            });

            if (file != null)
            {
                this.tbTargetFile.Text = file.Path.LocalPath;
            }
        }

        private async void btnRun_Click(object sender, RoutedEventArgs e)
        {
            var error = new Error();
            var sourceFile = this.tbSourceFile.Text;
            if (string.IsNullOrWhiteSpace(sourceFile))
            {
                error.Add("Select a source file!");
            }

            var unitTypes = this.GetUnitTypes();

            if (!unitTypes.Any())
            {
                error.Add("Select at least one unit type!");
            }

            var outFile = this.tbTargetFile.Text;
            if (string.IsNullOrWhiteSpace(outFile))
            {
                error.Add("Select a target file!");
            }

            if (error.HasError)
            {
                this.lblError.Text = error.GetMessage();
                return;
            }

            this.SetRunningState(true);

            var filterCombatEvents = this.cbFilterEvents.IsChecked.GetValueOrDefault();

            this.pbFilter.Value = 0;
            var progress = new Progress<double>(fraction => this.pbFilter.Value = fraction * 100);

            this.cancellationTokenSource = new CancellationTokenSource();
            try
            {
                await this.fileHandler.FilterFileByUnitTypeAsync(sourceFile, unitTypes, outFile, filterCombatEvents, progress, this.cancellationTokenSource.Token);
                this.lblError.Text = "Finished!";

                // Prefill the preview tab so the fresh pair can be analyzed directly.
                this.tbUnfilteredFile.Text = sourceFile;
                this.tbFilteredFile.Text = outFile;
            }
            catch (OperationCanceledException)
            {
                this.lblError.Text = "Cancelled.";
            }
            catch (Exception ex)
            {
                this.lblError.Text = ex.Message + "\r\n" + ex.StackTrace;
                //this.lblError.Text = "An unexpected error has occurred!";
            }
            finally
            {
                this.cancellationTokenSource.Dispose();
                this.cancellationTokenSource = null;
                this.SetRunningState(false);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.cancellationTokenSource?.Cancel();
        }

        private async void btnUnfilteredFile_Click(object sender, RoutedEventArgs e)
        {
            var file = await this.OpenLogFileAsync("Select Unfiltered Log");

            if (file != null)
            {
                this.tbUnfilteredFile.Text = file;
            }
        }

        private async void btnFilteredFile_Click(object sender, RoutedEventArgs e)
        {
            var file = await this.OpenLogFileAsync("Select Filtered Log");

            if (file != null)
            {
                this.tbFilteredFile.Text = file;
            }
        }

        private async void btnAnalyze_Click(object sender, RoutedEventArgs e)
        {
            var error = new Error();
            var unfilteredFile = this.tbUnfilteredFile.Text;
            if (string.IsNullOrWhiteSpace(unfilteredFile))
            {
                error.Add("Select an unfiltered file!");
            }

            var filteredFile = this.tbFilteredFile.Text;
            if (string.IsNullOrWhiteSpace(filteredFile))
            {
                error.Add("Select a filtered file!");
            }

            if (error.HasError)
            {
                this.lblPreviewError.Text = error.GetMessage();
                return;
            }

            this.SetAnalyzingState(true);
            this.lblPreviewError.Text = "";

            this.pbAnalyze.Value = 0;
            var unfilteredProgress = new Progress<double>(fraction => this.pbAnalyze.Value = fraction * 50);
            var filteredProgress = new Progress<double>(fraction => this.pbAnalyze.Value = 50 + (fraction * 50));

            this.analyzeCancellationTokenSource = new CancellationTokenSource();
            try
            {
                // Sequential on purpose: the summary service is stateful per pass.
                var unfiltered = await this.fileSummarizer.SummarizeFileAsync(unfilteredFile, unfilteredProgress, this.analyzeCancellationTokenSource.Token);
                var filtered = await this.fileSummarizer.SummarizeFileAsync(filteredFile, filteredProgress, this.analyzeCancellationTokenSource.Token);

                this.BuildPreview(unfiltered, filtered);
            }
            catch (OperationCanceledException)
            {
                this.lblPreviewError.Text = "Cancelled.";
            }
            catch (IOException ioException)
            {
                this.lblPreviewError.Text = ioException.Message;
            }
            catch
            {
                this.lblPreviewError.Text = "An unexpected error has occurred!";
            }
            finally
            {
                this.analyzeCancellationTokenSource.Dispose();
                this.analyzeCancellationTokenSource = null;
                this.SetAnalyzingState(false);
            }
        }

        private void btnAnalyzeCancel_Click(object sender, RoutedEventArgs e)
        {
            this.analyzeCancellationTokenSource?.Cancel();
        }

        private async System.Threading.Tasks.Task<string> OpenLogFileAsync(string title)
        {
            var files = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("LogFiles") { Patterns = new[] { "*.log" } }
                }
            });

            return files.Count > 0 ? files[0].Path.LocalPath : null;
        }

        private void SetRunningState(bool isRunning)
        {
            this.btnRun.IsVisible = !isRunning;
            this.pnlLoading.IsVisible = isRunning;
            this.btnSourceFile.IsEnabled = !isRunning;
            this.btnTargetFile.IsEnabled = !isRunning;

            // While filtering writes the target file, analyzing it would read a half-written log.
            this.btnAnalyze.IsEnabled = !isRunning;
        }

        private void SetAnalyzingState(bool isAnalyzing)
        {
            this.btnAnalyze.IsVisible = !isAnalyzing;
            this.pnlAnalyzing.IsVisible = isAnalyzing;
            this.btnUnfilteredFile.IsEnabled = !isAnalyzing;
            this.btnFilteredFile.IsEnabled = !isAnalyzing;
            this.btnRun.IsEnabled = !isAnalyzing;
        }

        private void BuildPreview(LogSummary unfiltered, LogSummary filtered)
        {
            var culture = CultureInfo.CurrentCulture;

            var sizeDelta = filtered.FileSizeBytes - unfiltered.FileSizeBytes;
            var sizePercent = unfiltered.FileSizeBytes != 0 ? sizeDelta * 100.0 / unfiltered.FileSizeBytes : 0.0;
            this.tileSizeValue.Text = $"{FormatMegabytes(unfiltered.FileSizeBytes)} → {FormatMegabytes(filtered.FileSizeBytes)} MB";
            this.tileSizeSub.Text = $"{FormatByteDelta(sizeDelta)} ({sizePercent.ToString("+0.00;-0.00;0.00", culture)} %)";

            var lineDelta = filtered.TotalLines - unfiltered.TotalLines;
            this.tileLinesValue.Text = $"{unfiltered.TotalLines.ToString("N0", culture)} → {filtered.TotalLines.ToString("N0", culture)}";
            this.tileLinesSub.Text = $"{lineDelta.ToString("+#,##0;-#,##0;±0", culture)} lines";

            var totalUnfilteredUnits = unfiltered.GetTotalUnitCount();
            var totalFilteredUnits = filtered.GetTotalUnitCount();
            var hiddenUnits = totalUnfilteredUnits - totalFilteredUnits;
            var hiddenPlayers = unfiltered.GetUnitCount(UnitTypes.Player) - filtered.GetUnitCount(UnitTypes.Player);
            this.tileUnitsValue.Text = $"{totalFilteredUnits.ToString("N0", culture)} kept";
            this.tileUnitsSub.Text = $"{hiddenUnits.ToString("N0", culture)} of {totalUnfilteredUnits.ToString("N0", culture)} hidden · {hiddenPlayers.ToString("N0", culture)} players hidden";
            this.tileUnitsSub.Foreground = hiddenPlayers > 0 ? Brushes.IndianRed : new SolidColorBrush(Color.Parse("#B0B0B0"));

            this.tileFightsValue.Text = unfiltered.FightCount.ToString("N0", culture);
            this.tileFightsSub.Text = unfiltered.FightCount > 0 ? FormatCombatTime(unfiltered.CombatTimeMs) : "no combat markers";

            this.icUnitRows.ItemsSource = BuildUnitRows(unfiltered, filtered, culture);
            this.icRecordRows.ItemsSource = BuildRecordRows(unfiltered, filtered, culture);

            this.previewUnfiltered = unfiltered;
            this.previewFiltered = filtered;
            this.lbFights.ItemsSource = BuildFightRows(unfiltered, culture);
            this.lbFights.SelectedIndex = 0;
            this.RebuildDamageRows();
        }

        private void lbFights_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.RebuildDamageRows();
        }

        private void RebuildDamageRows()
        {
            if (this.previewUnfiltered == null || !(this.lbFights.SelectedItem is FightRow fightRow))
            {
                return;
            }

            var unfilteredDamage = GetScopedDamage(this.previewUnfiltered, fightRow.FightIndex);
            var filteredDamage = GetScopedDamage(this.previewFiltered, fightRow.FightIndex);
            this.icDamageRows.ItemsSource = BuildDamageRows(unfilteredDamage, filteredDamage, CultureInfo.CurrentCulture);
        }

        private static List<FightRow> BuildFightRows(LogSummary unfiltered, CultureInfo culture)
        {
            var rows = new List<FightRow>
            {
                new FightRow($"Whole log · {unfiltered.FightCount.ToString("N0", culture)} fights", -1),
            };

            foreach (var fight in unfiltered.Fights)
            {
                rows.Add(new FightRow(BuildFightLabel(fight, unfiltered.EpochMs), fight.Index));
            }

            return rows;
        }

        private static string BuildFightLabel(FightSummary fight, long epochMs)
        {
            var start = fight.StartMs.HasValue && epochMs > 0
                ? DateTimeOffset.FromUnixTimeMilliseconds(epochMs + fight.StartMs.Value).ToLocalTime().ToString("HH:mm")
                : "--:--";

            var duration = fight.StartMs.HasValue && fight.EndMs.HasValue && fight.EndMs.Value >= fight.StartMs.Value
                ? TimeSpan.FromMilliseconds(fight.EndMs.Value - fight.StartMs.Value).ToString(@"m\:ss")
                : "?";

            // Every player counts — named or "(anonymous #n)" — but not the
            // NPC bucket or the anonymous fallback bucket.
            var players = fight.DamageBySourcePlayer.Keys.Count(
                key => key != LogSummary.NonPlayerSourcesKey && key != LogSummary.AnonymousPlayersKey);

            return $"#{fight.Index} · {start} · {duration} · {players} players";
        }

        private static Dictionary<string, long> GetScopedDamage(LogSummary summary, int fightIndex)
        {
            if (fightIndex < 0)
            {
                return summary.DamageBySourcePlayer;
            }

            // The fights of the two files are matched by index; BEGIN_COMBAT and
            // END_COMBAT lines are never filtered, so the lists line up. A pair
            // that does not (e.g. two unrelated files) falls back to no damage.
            var fight = summary.Fights.FirstOrDefault(f => f.Index == fightIndex);
            return fight != null ? fight.DamageBySourcePlayer : new Dictionary<string, long>();
        }

        private static List<DamageRow> BuildDamageRows(Dictionary<string, long> unfilteredDamage, Dictionary<string, long> filteredDamage, CultureInfo culture)
        {
            long GetValue(Dictionary<string, long> damageByKey, string key) => damageByKey.TryGetValue(key, out var value) ? value : 0;

            var keys = unfilteredDamage.Keys
                .Union(filteredDamage.Keys)
                .OrderByDescending(key => GetValue(unfilteredDamage, key))
                .ThenBy(key => key)
                .ToList();

            var maxTotal = keys.Select(key => GetValue(unfilteredDamage, key)).DefaultIfEmpty(0).Max();

            var rows = new List<DamageRow>();

            foreach (var key in keys)
            {
                var total = GetValue(unfilteredDamage, key);
                var counted = GetValue(filteredDamage, key);

                var barWidth = maxTotal > 0 ? DamageBarWidth * total / maxTotal : 0;
                var fillFraction = total > 0 ? Math.Clamp((double)counted / total, 0, 1) : 0;
                var fillWidth = barWidth * fillFraction;

                rows.Add(new DamageRow(
                    key,
                    FormatDamage(total, culture),
                    FormatDamage(counted, culture),
                    FormatDamage(total - counted, culture),
                    fillWidth,
                    barWidth - fillWidth));
            }

            var totalSum = keys.Sum(key => GetValue(unfilteredDamage, key));
            var countedSum = keys.Sum(key => GetValue(filteredDamage, key));
            rows.Add(new DamageRow(
                "Total",
                FormatDamage(totalSum, culture),
                FormatDamage(countedSum, culture),
                FormatDamage(totalSum - countedSum, culture),
                0,
                0,
                isTotal: true));

            return rows;
        }

        private static string FormatDamage(long value, CultureInfo culture)
        {
            if (value == 0)
            {
                return "—";
            }

            var absolute = Math.Abs(value);

            if (absolute >= 1_000_000)
            {
                return (value / 1_000_000.0).ToString("N1", culture) + " M";
            }

            if (absolute >= 10_000)
            {
                return (value / 1_000.0).ToString("N0", culture) + " k";
            }

            return value.ToString("N0", culture);
        }

        private static List<SummaryRow> BuildUnitRows(LogSummary unfiltered, LogSummary filtered, CultureInfo culture)
        {
            var rows = new List<SummaryRow>();

            foreach (var (type, name) in UnitCategories)
            {
                var unfilteredCount = unfiltered.GetUnitCount(type);
                var filteredCount = filtered.GetUnitCount(type);

                // Units of unknown type only appear after a game update; hide the noise row otherwise.
                if (type == UnitTypes.Unknown && unfilteredCount == 0 && filteredCount == 0)
                {
                    continue;
                }

                rows.Add(new SummaryRow(name, FormatCount(unfilteredCount, culture), FormatCount(filteredCount, culture), FormatCount(unfilteredCount - filteredCount, culture)));
            }

            var totalUnfiltered = unfiltered.GetTotalUnitCount();
            var totalFiltered = filtered.GetTotalUnitCount();
            rows.Add(new SummaryRow(
                "Total",
                totalUnfiltered.ToString("N0", culture),
                totalFiltered.ToString("N0", culture),
                (totalUnfiltered - totalFiltered).ToString("N0", culture),
                isTotal: true));

            return rows;
        }

        private static List<SummaryRow> BuildRecordRows(LogSummary unfiltered, LogSummary filtered, CultureInfo culture)
        {
            var rows = unfiltered.LinesByRecordType.Keys
                .Union(filtered.LinesByRecordType.Keys)
                .OrderByDescending(recordType => unfiltered.GetLineCount(recordType))
                .ThenBy(recordType => recordType)
                .Select(recordType =>
                {
                    var unfilteredCount = unfiltered.GetLineCount(recordType);
                    var filteredCount = filtered.GetLineCount(recordType);
                    return new SummaryRow(recordType, FormatCount(unfilteredCount, culture), FormatCount(filteredCount, culture), FormatCount(unfilteredCount - filteredCount, culture));
                })
                .ToList();

            rows.Add(new SummaryRow(
                "Total",
                unfiltered.TotalLines.ToString("N0", culture),
                filtered.TotalLines.ToString("N0", culture),
                (unfiltered.TotalLines - filtered.TotalLines).ToString("N0", culture),
                isTotal: true));

            return rows;
        }

        private static string FormatCount(long count, CultureInfo culture)
        {
            return count == 0 ? "—" : count.ToString("N0", culture);
        }

        private static string FormatMegabytes(long bytes)
        {
            return (bytes / 1_000_000.0).ToString("N1", CultureInfo.CurrentCulture);
        }

        private static string FormatByteDelta(long bytes)
        {
            var sign = bytes > 0 ? "+" : bytes < 0 ? "-" : "±";
            var absolute = Math.Abs(bytes);
            var magnitude = absolute >= 1_000_000
                ? (absolute / 1_000_000.0).ToString("N1", CultureInfo.CurrentCulture) + " MB"
                : (absolute / 1_000.0).ToString("N1", CultureInfo.CurrentCulture) + " KB";

            return sign + magnitude;
        }

        private static string FormatCombatTime(long combatTimeMs)
        {
            var time = TimeSpan.FromMilliseconds(combatTimeMs);

            return time.TotalHours >= 1
                ? $"{(int)time.TotalHours} h {time.Minutes} m in combat"
                : $"{time.Minutes} m {time.Seconds} s in combat";
        }

        private UnitTypes[] GetUnitTypes()
        {
            var unitTypes = new List<UnitTypes>();

            if (this.cbPlayer.IsChecked.GetValueOrDefault())
            {
                unitTypes.Add(UnitTypes.Player);
            }

            if (this.cbMonsterHostile.IsChecked.GetValueOrDefault())
            {
                unitTypes.Add(UnitTypes.MonsterHostile);
            }

            if (this.cbMonsterNpcAlly.IsChecked.GetValueOrDefault())
            {
                unitTypes.Add(UnitTypes.MonsterNpcAlly);
            }

            if (this.cbMonsterNpcEnemy.IsChecked.GetValueOrDefault())
            {
                unitTypes.Add(UnitTypes.MonsterNpcEnemy);
            }

            if (this.cbMonsterFriendly.IsChecked.GetValueOrDefault())
            {
                unitTypes.Add(UnitTypes.MonsterFriendly);
            }

            if (this.cbMonsterNeutral.IsChecked.GetValueOrDefault())
            {
                unitTypes.Add(UnitTypes.MonsterNeutral);
            }

            if (this.cbObject.IsChecked.GetValueOrDefault())
            {
                unitTypes.Add(UnitTypes.Object);
            }

            if (this.cbSiegeWeapon.IsChecked.GetValueOrDefault())
            {
                unitTypes.Add(UnitTypes.SiegeWeapon);
            }

            return unitTypes.ToArray();
        }
    }
}
