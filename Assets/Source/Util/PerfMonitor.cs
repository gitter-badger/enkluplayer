using System.Diagnostics;
using UnityEngine;
using UnityEngine.Profiling;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Monitors performance metrics.
    /// </summary>
    public class PerfMonitor : MonoBehaviour
    {
        public class FrameTimeData
        {
            public long AverageMs;
            public long MinMs;
            public long MaxMs;

            public static int FrameTimeToFps(long ms)
            {
                return Mathf.RoundToInt(1000f / ms);
            }
        }

        public class MemoryData
        {
            public long Total;
            public long Allocated;
            public long Mono;
            public long Gpu;
            public long GraphicsDriver;

            public static float BytesToMb(long bytes)
            {
                return (float) ((double) bytes / 1000000);
            }
        }

        private const int NUM_SAMPLES = 1024;

        public readonly FrameTimeData FrameTime = new FrameTimeData();
        public readonly MemoryData Memory = new MemoryData();
        
        private readonly long[] _frames = new long[NUM_SAMPLES];
        private int _frameIndex;
        private bool _wrap;

        private readonly Stopwatch _watch = new Stopwatch();

        private void Update()
        {
            // calculate frame time
            _watch.Stop();

            var dt = _watch.ElapsedMilliseconds;
            if (0 != dt)
            {
                _frames[_frameIndex] = dt;

                // advance
                if (_frameIndex == NUM_SAMPLES - 1)
                {
                    _wrap = true;
                    _frameIndex = 0;
                }
                else
                {
                    _frameIndex += 1;
                }
                
                // calculate
                var sum = 0L;
                var min = long.MaxValue;
                var max = long.MinValue;

                var len = _wrap ? NUM_SAMPLES : _frameIndex;
                for (var i = 0; i < len; i++)
                {
                    var value = _frames[i];
                    if (value < min)
                    {
                        min = value;
                    }

                    if (value > max)
                    {
                        max = value;
                    }

                    sum += value;
                }

                FrameTime.AverageMs = Mathf.RoundToInt((float) ((double) sum / len));
                FrameTime.MinMs = min;
                FrameTime.MaxMs = max;
            }

            // update memory
            Memory.Total = Profiler.GetTotalReservedMemoryLong();
            Memory.Allocated = Profiler.GetTotalAllocatedMemoryLong();
            Memory.Mono = System.GC.GetTotalMemory(false);
            Memory.Gpu = SystemInfo.graphicsMemorySize;
            Memory.GraphicsDriver = Profiler.GetAllocatedMemoryForGraphicsDriver();

            _watch.Reset();
            _watch.Start();
        }
    }
}