using Microsoft.VisualStudio.Threading;
using QComp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace QComp
{
    public class ComparisonManager
    {
        public delegate void OnRoundHandler();
        public event OnRoundHandler? OnRoundStarted;

        public int Round { get; set; } = 0;
        private bool _abort = false;
        public bool Abort { 
            get => _abort; 
            set { 
                _abort = value; 
                if (_currentProcess != null && !_currentProcess.HasExited)
                    _currentProcess?.Kill();
            } 
        }

        private Process? _currentProcess;

        public async Task<List<ComparisonResult>> CompareAsync(string binary1, string binary2, string arguments, int rounds)
        {
            Round = 0;
            Abort = false;

            // Warmup
            await ExecuteBinaryAsync("cmd", "/c");

            var result = new List<ComparisonResult>();

            var watch = new Stopwatch();
            for(int i = 0; i < rounds && !Abort; i++)
            {
                var subResult = new ComparisonResult();
                subResult.Round = i;
                Round = i;
                watch.Restart();
                await ExecuteBinaryAsync(binary1, arguments);
                watch.Stop();
                subResult.Value1 = watch.ElapsedMilliseconds;
                watch.Restart();
                await ExecuteBinaryAsync(binary2, arguments);
                watch.Stop();
                subResult.Value2 = watch.ElapsedMilliseconds;

                result.Add(subResult);
                OnRoundStarted?.Invoke();
            }

            return result;
        }

        private async Task<int> ExecuteBinaryAsync(string file, string arguments)
        {
            if (Abort)
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
    }
}
