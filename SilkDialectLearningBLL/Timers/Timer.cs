using System;
using System.Threading;
using System.Threading.Tasks;

namespace SilkDialectLearningBLL.Timers
{
    internal delegate void TimerCallback(object state);
    internal sealed class PclTimer : CancellationTokenSource, IDisposable
    {
        internal PclTimer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
        {
            //Contract.Assert(period == TimeSpan.FromMilliseconds(-1), "This stub implementation only supports dueTime.");
            Task.Delay(dueTime, Token).ContinueWith((t, s) =>
            {
                var tuple = (Tuple<TimerCallback, object>)s;
                tuple.Item1(tuple.Item2);
            }, Tuple.Create(callback, state), CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion,
                TaskScheduler.Default);
        }

        public new void Dispose() { base.Cancel(); }
    }

    public class Timer
    {
        private PclTimer pcltimer;
        private TimeSpan interval;

        public Timer(double interval)
        {
            Interval = interval;
        }

        /// <summary>
        /// Interval between signals in milliseconds.
        /// </summary>
        public double Interval
        {
            get 
            { 
                return interval.TotalMilliseconds; 
            }
            set 
            { 
                interval = TimeSpan.FromMilliseconds(value); 
            }
        }

        /// <summary>
        /// True if PCLTimer is running, false if not.
        /// </summary>
        public bool Enabled
        {
            get { return null != pcltimer; }
            set 
            {
                if (value) 
                    Start(); 
                else 
                    Stop(); 
            }
        }

        /// <summary>
        /// Occurs when the specified time has elapsed and the PCLTimer is enabled.
        /// </summary>
        public event EventHandler Elapsed;

        /// <summary>
        /// Starts the PCLTimer.
        /// </summary>
        public void Start()
        {
            if (interval.TotalMilliseconds == 0)
                throw new InvalidOperationException("Set Elapsed property before calling PCLTimer.Start().");
            pcltimer = new PclTimer(OnElapsed, null, interval, interval);
        }

        /// <summary>
        /// Stops the PCLTimer.
        /// </summary>
        public void Stop()
        {
            pcltimer.Dispose();
        }

        /// <summary>
        /// Releases all resources.
        /// </summary>
        public void Dispose()
        {
            pcltimer.Dispose();
        }

        /// <summary>
        /// Invokes Elapsed event.
        /// </summary>
        /// <param name="state"></param>
        private void OnElapsed(object state)
        {
            if (null != pcltimer && null != Elapsed)
                Elapsed(this, EventArgs.Empty);
        }
    }
}
