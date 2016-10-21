using System;
using System.Diagnostics;
using System.Threading;
using DereTore.Applications.StarlightDirector.Core.Interop;

namespace DereTore.Applications.StarlightDirector {
    public sealed class ThreadingTimer : DisposableBase {
        public readonly uint MMTimerPeriod = 1;

        public ThreadingTimer(double interval) {
            NativeMethods.timeBeginPeriod(MMTimerPeriod);
            Interval = interval;
            _stopwatch = new Stopwatch();
        }

        public event EventHandler<EventArgs> Elapsed;

        public void Start() {
            if (_stopwatch.IsRunning) {
                return;
            }
            _thread = new Thread(ThreadProc) {
                IsBackground = true
            };
            _continue = true;
            _thread.Start();
        }

        public void Stop() {
            if (!_stopwatch.IsRunning) {
                return;
            }
            _continue = false;
            _thread.Join();
        }

        public double Interval { get; }

        protected override void Dispose(bool explicitDisposing) {
            Stop();
            if (explicitDisposing) {
            }
            NativeMethods.timeEndPeriod(MMTimerPeriod);
        }

        private void ThreadProc() {
            var stopwatch = _stopwatch;
            stopwatch.Start();
            while (_continue) {
                var timeDiff = stopwatch.ElapsedMilliseconds;
                if (timeDiff >= Interval) {
                    stopwatch.Restart();
                    Elapsed?.Invoke(this, EventArgs.Empty);
                } else {
                    Thread.Sleep(1);
                }
            }
            stopwatch.Reset();
        }

        private Thread _thread;
        private bool _continue;
        private readonly Stopwatch _stopwatch;

    }
}
