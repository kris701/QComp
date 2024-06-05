using Microsoft.VisualStudio.Threading;
using QComp.Helpers;
using QComp.Models;
using QComp.UserControls;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace QComp
{
    public partial class QCompWindowControl : UserControl
    {
        private SavesManager _saveManager;
        private ComparisonManager _comparisoManager;
        private bool _abort = false;

        public QCompWindowControl()
        {
            InitializeComponent();
            _saveManager = new SavesManager();
            _comparisoManager = new ComparisonManager();
            _comparisoManager.OnRoundStarted += () => { RunningProgressBar.Value += 1; };
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await _saveManager.InitializeAsync();
            await RefreshComboboxAsync();
        }

        private async Task RefreshComboboxAsync()
        {
            CompareToCombobox.Items.Clear();
            var project = await DTE2Helper.GetActiveProjectAsync();
            if (project != null)
                foreach(var save in _saveManager.GetSavesForProject(project.Name))
                    CompareToCombobox.Items.Add(save);
        }

        private async void CompareButton_Click(object sender, RoutedEventArgs e)
        {
            if (CompareToCombobox.SelectedItem is SaveItem save)
            {
                _abort = false;
                RunningGrid.Visibility = Visibility.Visible;
                ControlsPanel.IsEnabled = false;
                ControlsPanel.Opacity = 0.3;
                var project = await DTE2Helper.GetActiveProjectAsync();
                var targetBinary = _saveManager.GetBinaryPath(project.Name, save.Name);
                var rounds = Int32.Parse(RoundsTextBox.Text);
                if (project != null && targetBinary != null)
                {
                    var currentBinary = Path.Combine(project.FullName, await DTE2Helper.GetOutputDirAsync(), $"{project.Name}.exe");
                    RunningProgressBar.Maximum = rounds;
                    RunningProgressBar.Value = 0;

                    var results = await _comparisoManager.CompareAsync(currentBinary, targetBinary.FullName, ArgumentsTextbox.Text, rounds);

                    if (!_abort)
                    {
                        ResultDataGrid.ItemsSource = null;
                        var list = new ObservableCollection<TableRow>();
                        list.Add(new TableRow() { Name = "Sum", Value1 = Math.Round(results.Sum(x => x.Value1),1), Value2 = Math.Round(results.Sum(x => x.Value2), 1) });
                        list.Add(new TableRow() { Name = "Avg", Value1 = Math.Round(results.Average(x => x.Value1), 1), Value2 = Math.Round(results.Average(x => x.Value2), 1) });
                        list.Add(new TableRow() { Name = "Min", Value1 = Math.Round(results.Min(x => x.Value1), 1), Value2 = Math.Round(results.Min(x => x.Value2), 1) });
                        list.Add(new TableRow() { Name = "Max", Value1 = Math.Round(results.Max(x => x.Value1), 1), Value2 = Math.Round(results.Max(x => x.Value2), 1) });
                        ResultDataGrid.ItemsSource = list;
                        var targetWidth = ResultDataGrid.ActualWidth / 3;
                        ResultDataGrid.Columns[0].Header = "";
                        ResultDataGrid.Columns[0].Width = targetWidth;
                        ResultDataGrid.Columns[1].Header = "Current";
                        ResultDataGrid.Columns[1].Width = targetWidth;
                        ResultDataGrid.Columns[2].Header = save.Name;
                        ResultDataGrid.Columns[2].Width = targetWidth;

                        PlotGrid.Children.Clear();
                        PlotGrid.Children.Add(new ScatterPlot(results.Select(x => x.Value1).ToList(), "Current", results.Select(x => x.Value2).ToList(), save.Name, PlotGrid.ActualWidth));
                    }
                }
                RunningGrid.Visibility = Visibility.Hidden;
                ControlsPanel.IsEnabled = true;
                ControlsPanel.Opacity = 1;
            }
        }

        private async void SaveNewButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveNewNameTextbox.Text != "")
            {
                var project = await DTE2Helper.GetActiveProjectAsync();
                if (project != null)
                {
                    var sourceContent = Path.Combine(project.FullName, await DTE2Helper.GetOutputDirAsync());
                    _saveManager.Save(sourceContent, project.Name, SaveNewNameTextbox.Text);
                }
                SaveNewNameTextbox.Text = "";
            }
            await RefreshComboboxAsync();
        }

        private static readonly Regex _regex = new Regex("[^0-9.-]+");
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private void RoundsTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _abort = true;
            _comparisoManager.Abort = true;
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void GitButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/kris701/QComp",
                UseShellExecute = true
            });
        }

        private async void RemoveBinaryButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is SaveItem save)
                await DeleteItemAsync(save);
        }

        private async Task DeleteItemAsync(SaveItem save)
        {
            var project = await DTE2Helper.GetActiveProjectAsync();
            if (project != null)
            {
                _saveManager.Delete(project.Name, save.Name);
                CompareToCombobox.Items.Remove(save);
            }
        }
    }
}
