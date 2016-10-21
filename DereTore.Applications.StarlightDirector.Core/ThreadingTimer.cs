using System;
using System.Diagnostics;
using System.Threading;
using DereTore.Interop;

namespace DereTore.Applications.StarlightDirector {
    public sealed class ThreadingTimer : DisposableBase {

        public ThreadingTimer(double interval) {
            _lock = new object();
            NativeMethods.timeBeginPeriod(TimerPeriod);
            Interval = interval;
            _stopwatch = new Stopwatch();
        }

        public event EventHandler<EventArgs> Elapsed;

        public void Start() {
            lock (_lock) {
                if (_thread != null) {
                    return;
                }
                _thread = new Thread(ThreadProc) {
                    IsBackground = true
                };
                _continue = true;
                _thread.Start();
                Monitor.Wait(_lock);
            }
        }

        public void Stop() {
            lock (_lock) {
                if (_thread == null) {
                    return;
                }
                try {
                    _continue = false;
                    _thread.Join();
                } finally {
                    _thread = null;
                }
            }
        }

        public double Interval { get; }

        protected override void Dispose(bool explicitDisposing) {
            Stop();
            if (explicitDisposing) {
            }
            NativeMethods.timeEndPeriod(TimerPeriod);
        }

        private void ThreadProc() {
            lock (_lock) {
                Monitor.Pulse(_lock);
            }
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

        private readonly object _lock;
        private Thread _thread;
        private volatile bool _continue;
        private readonly Stopwatch _stopwatch;

        private static readonly uint TimerPeriod = 1;

    }
}
