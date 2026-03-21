namespace EsoLogFilter.Ui.Avalonia
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using global::Avalonia.Controls;
    using global::Avalonia.Interactivity;
    using global::Avalonia.Platform.Storage;
    using EsoLogFilter.Core.Model.Objects;
    using EsoLogFilter.Core.Services;
    using EsoLogFilter.Ui.Avalonia.Helper;

    public partial class MainWindow : Window
    {
        private readonly IFileHandler fileHandler;
        private CancellationTokenSource cancellationTokenSource;

        public MainWindow(IFileHandler fileHandler)
        {
            InitializeComponent();
            this.fileHandler = fileHandler;
        }

        private async void btnSourceFile_Click(object sender, RoutedEventArgs e)
        {
            var files = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select Source File",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("LogFiles") { Patterns = new[] { "*.log" } }
                }
            });

            if (files.Count > 0)
            {
                this.tbSourceFile.Text = files[0].Path.LocalPath;
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

            var unitTypes = this.GetUnitTipes();

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

            this.cancellationTokenSource = new CancellationTokenSource();
            try
            {
                await this.fileHandler.FilterFileByUnitTypeAsync(sourceFile, unitTypes, outFile, this.cancellationTokenSource.Token);
                this.lblError.Text = "Finished!";
            }
            catch (OperationCanceledException)
            {
                this.lblError.Text = "Cancelled.";
            }
            catch
            {
                this.lblError.Text = "A unexpected error has occured!";
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

        private void SetRunningState(bool isRunning)
        {
            this.btnRun.IsVisible = !isRunning;
            this.pnlLoading.IsVisible = isRunning;
            this.btnSourceFile.IsEnabled = !isRunning;
            this.btnTargetFile.IsEnabled = !isRunning;
        }

        private UnitTypes[] GetUnitTipes()
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