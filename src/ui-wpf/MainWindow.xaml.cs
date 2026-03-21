namespace EsoLogFilter.Ui.Wpf
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using EsoLogFilter.Core.Model.Objects;
    using EsoLogFilter.Core.Services;
    using EsoLogFilter.Ui.Wpf.Helper;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IFileHandler fileHandler;

        public MainWindow(IFileHandler fileHandler)
        {
            InitializeComponent();
            this.fileHandler = fileHandler;
        }

        private void btnSourceFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.DefaultExt = ".log";
            openFileDialog.Filter = "LogFiles (.log)|*.log";
            openFileDialog.CheckFileExists = true;
            var result = openFileDialog.ShowDialog();
            if (result.GetValueOrDefault())
            {
                this.tbSourceFile.Text = openFileDialog.FileName;
            }
        }

        private void btnTargetFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.SaveFileDialog();
            openFileDialog.FileName = "Encounter-filtered";
            openFileDialog.DefaultExt = ".log";
            openFileDialog.Filter = "LogFiles (.log)|*.log";
            var result = openFileDialog.ShowDialog();
            if (result.GetValueOrDefault())
            {
                this.tbTargetFile.Text = openFileDialog.FileName;
            }
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            this.lblError.Content = "Working!";
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
            if (string.IsNullOrWhiteSpace(sourceFile))
            {
                error.Add("Select a target file!");
            }

            if (!error.HasError)
            {
                try
                {
                    this.fileHandler.FilterFileByUnitType(sourceFile, unitTypes, outFile);
                }
                catch
                {
                    error.Add("A unexpected error has occured!");
                }
            }

            if (error.HasError)
            {
                this.lblError.Content = error.GetMessage();
            }
            else
            {
                this.lblError.Content = "Finished!";
            }
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
