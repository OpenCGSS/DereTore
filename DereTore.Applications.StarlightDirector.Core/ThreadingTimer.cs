using System;
using System.Diagnostics;
using System.Threading;

namespace DereTore.Applications.StarlightDirector {
    public sealed class ThreadingTimer : DisposableBase {

        public ThreadingTimer(double interval) {
            Interval = interval;
            _waitHandle = new ManualResetEvent(false);
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
            _waitHandle.Reset();
            Thread.MemoryBarrier();
            _continue = false;
            _waitHandle.WaitOne();
        }

        public double Interval { get; }

        protected override void Dispose(bool explicitDisposing) {
            Stop();
            if (explicitDisposing) {
                _waitHandle.Close();
            }
        }

        private void ThreadProc() {
            var stopwatch = _stopwatch;
            stopwatch.Start();
            while (_continue) {
                var timeDiff = stopwatch.ElapsedMilliseconds;
                if (timeDiff >= Interval) {
                    Elapsed?.Invoke(this, EventArgs.Empty);
                    stopwatch.Reset();
                    stopwatch.Start();
                }
            }
            stopwatch.Reset();
            _waitHandle.Set();
        }

        private Thread _thread;
        private bool _continue;
        private readonly Stopwatch _stopwatch;
        private readonly ManualResetEvent _waitHandle;

    }
}
