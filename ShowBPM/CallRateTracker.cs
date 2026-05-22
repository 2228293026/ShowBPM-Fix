using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ShowBPM
{
    public class CallRateTracker
    {
        private readonly Queue<long> callTimestamps = new Queue<long>();
        private readonly object lockObject = new object();

        public void TrackCall()
        {
            long now = Stopwatch.GetTimestamp();
            lock (lockObject)
            {
                callTimestamps.Enqueue(now);
            }
        }

        public int GetCallsPerSecond()
        {
            long now = Stopwatch.GetTimestamp();
            long freq = Stopwatch.Frequency;
            lock (lockObject)
            {
                while (callTimestamps.Count > 0 && (now - callTimestamps.Peek()) / (double)freq > 1.0)
                {
                    callTimestamps.Dequeue();
                }
                return callTimestamps.Count;
            }
        }
    }
}
