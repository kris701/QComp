using Microsoft.VisualStudio.Threading;
using QComp.Helpers;
using QComp.Models;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace QComp
{
    public partial class QCompWindowControl : UserControl
    {
        private SavesManager _saveManager;
        private Process? _currentProcess;
        private bool _abort = false;

        public QCompWindowControl()
        {
            InitializeComponent();
            _saveManager = new SavesManager();
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
                ResultTextBlock.Text = "";
                var project = await DTE2Helper.GetActiveProjectAsync();
                var targetBinary = _saveManager.GetBinaryPath(project.Name, save.Name);
                var rounds = Int32.Parse(RoundsTextBox.Text);
                if (project != null && targetBinary != null)
                {
                    var currentBinary = Path.Combine(project.FullName, await DTE2Helper.GetOutputDirAsync(), $"{project.Name}.exe");
                    RunningProgressBar.Maximum = rounds * 2;
                    RunningProgressBar.Value = 0;

                    var watch = new Stopwatch();
                    watch.Start();
                    var exitCurrent = -1;
                    for (int i = 0; i < rounds && !_abort; i++)
                    {
                        exitCurrent = await ExecuteBinaryAsync(currentBinary, ArgumentsTextbox.Text);
                        RunningProgressBar.Value += 1;
                    }
                    watch.Stop();
                    var timeCurrent = watch.ElapsedMilliseconds;

                    watch = new Stopwatch();
                    watch.Start();
                    var exitTarget = -1;
                    for (int i = 0; i < rounds && !_abort; i++) 
                    { 
                        exitTarget = await ExecuteBinaryAsync(targetBinary.FullName, ArgumentsTextbox.Text);
                        RunningProgressBar.Value += 1;
                    }
                    watch.Stop();
                    var timeTarget = watch.ElapsedMilliseconds;

                    if (!_abort)
                    {
                        ResultTextBlock.Text += $"Current exit code: {exitCurrent}{Environment.NewLine}";
                        ResultTextBlock.Text += $"Target exit code:  {exitTarget}{Environment.NewLine}";
                        ResultTextBlock.Text += $"Current Time:      {timeCurrent}ms{Environment.NewLine}";
                        ResultTextBlock.Text += $"Target Time:       {timeTarget}ms{Environment.NewLine}";
                    }
                }
                RunningGrid.Visibility = Visibility.Hidden;
                ControlsPanel.IsEnabled = true;
                ControlsPanel.Opacity = 1;
            }
        }

        private async Task<int> ExecuteBinaryAsync(string file, string arguments)
        {
            if (_abort)
                return -1;
            _currentProcess = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = file,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                }
            };

            _currentProcess.Start();
            await _currentProcess.WaitForExitAsync();
            return _currentProcess.ExitCode;
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
            if (_currentProcess != null && !_currentProcess.HasExited)
                _currentProcess?.Kill();
        }
    }
}
