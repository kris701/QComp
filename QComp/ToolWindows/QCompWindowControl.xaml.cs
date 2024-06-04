using Microsoft.VisualStudio.Threading;
using QComp.Helpers;
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
        public static string SaveLocation = "QComp";

        private Process? _currentProcess;
        private bool _abort = false;

        public QCompWindowControl()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await RefreshComboboxAsync();
        }

        private async Task RefreshComboboxAsync()
        {
            CompareToCombobox.Items.Clear();
            var solution = await DTE2Helper.GetSolutionPathAsync();
            var project = await DTE2Helper.GetActiveProjectAsync();
            if (solution != null && project != null)
            {
                var targetSaves = Path.Combine(solution.FullName, ".vs", project.Name, SaveLocation);
                if (!Directory.Exists(targetSaves))
                    Directory.CreateDirectory(targetSaves);

                foreach (var folder in new DirectoryInfo(targetSaves).GetDirectories())
                {
                    CompareToCombobox.Items.Add($"{folder.Name}");
                }
            }
        }

        private async void CompareButton_Click(object sender, RoutedEventArgs e)
        {
            if (CompareToCombobox.Text != "")
            {
                _abort = false;
                RunningGrid.Visibility = Visibility.Visible;
                ControlsPanel.IsEnabled = false;
                ControlsPanel.Opacity = 0.3;
                ResultTextBlock.Text = "";
                var solution = await DTE2Helper.GetSolutionPathAsync();
                var project = await DTE2Helper.GetActiveProjectAsync();
                var rounds = Int32.Parse(RoundsTextBox.Text);
                if (solution != null && project != null)
                {
                    var currentBinary = Path.Combine(project.FullName, await DTE2Helper.GetOutputDirAsync(), $"{project.Name}.exe");
                    var targetBinary = Path.Combine(solution.FullName, ".vs", project.Name, SaveLocation, CompareToCombobox.Text, $"{project.Name}.exe");
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
                        exitTarget = await ExecuteBinaryAsync(targetBinary, ArgumentsTextbox.Text);
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
                await CopyBinariesToCacheAsync(SaveNewNameTextbox.Text);
                SaveNewNameTextbox.Text = "";
            }
            await RefreshComboboxAsync();
        }

        private async Task CopyBinariesToCacheAsync(string name)
        {
            var solution = await DTE2Helper.GetSolutionPathAsync();
            var project = await DTE2Helper.GetActiveProjectAsync();
            if (solution != null && project != null)
            {
                var targetSaves = Path.Combine(solution.FullName, ".vs", project.Name, SaveLocation);
                var newSaveDir = Path.Combine(targetSaves, name);
                Directory.CreateDirectory(newSaveDir);

                var sourceBinaries = Path.Combine(project.FullName, await DTE2Helper.GetOutputDirAsync());

                FileHelper.Copy(sourceBinaries, newSaveDir);
            }
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