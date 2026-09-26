using System;
using System.Diagnostics;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Lightweight diagnostics tool for measuring GC allocation bytes and execution time
    /// across simulation hot loops and match situation calculations.
    /// Pure C#, safe for headless benchmark testing and regression detection.
    /// </summary>
    public static class PerformanceProfiler
    {
        public readonly struct ProfileScope : IDisposable
        {
            private readonly string _tag;
            private readonly long _startAllocatedBytes;
            private readonly long _startTicks;
            private readonly Action<ProfileResult>? _onComplete;

            public ProfileScope(string tag, Action<ProfileResult>? onComplete = null)
            {
                _tag = tag;
                _onComplete = onComplete;
                _startAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();
                _startTicks = Stopwatch.GetTimestamp();
            }

            public void Dispose()
            {
                long endTicks = Stopwatch.GetTimestamp();
                long endAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

                double elapsedMs = (endTicks - _startTicks) * 1000.0 / Stopwatch.Frequency;
                long allocatedBytes = Math.Max(0, endAllocatedBytes - _startAllocatedBytes);

                _onComplete?.Invoke(new ProfileResult(_tag, elapsedMs, allocatedBytes));
            }
        }

        public readonly struct ProfileResult
        {
            public string Tag { get; }
            public double ElapsedMs { get; }
            public long AllocatedBytes { get; }

            public ProfileResult(string tag, double elapsedMs, long allocatedBytes)
            {
                Tag = tag;
                ElapsedMs = elapsedMs;
                AllocatedBytes = allocatedBytes;
            }

            public override string ToString() =>
                $"[{Tag}] Elapsed: {ElapsedMs:F2}ms | Allocated: {AllocatedBytes:N0} bytes";
        }

        public static ProfileScope Measure(string tag, Action<ProfileResult>? onComplete = null) =>
            new ProfileScope(tag, onComplete);

        public static ProfileResult Benchmark(string tag, int iterations, Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            // Warm up
            action();

            long startAllocated = GC.GetAllocatedBytesForCurrentThread();
            var sw = Stopwatch.StartNew();

            for (int i = 0; i < iterations; i++)
            {
                action();
            }

            sw.Stop();
            long endAllocated = GC.GetAllocatedBytesForCurrentThread();
            long totalAlloc = Math.Max(0, endAllocated - startAllocated);

            return new ProfileResult(tag, sw.Elapsed.TotalMilliseconds, totalAlloc);
        }
    }
}
