using System.Diagnostics;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Monitors performance metrics.
    /// </summary>
    public class PerfMonitor : MonoBehaviour
    {
        /// <summary>
        /// Simple object for frame time data.
        /// </summary>
        public class FrameTimeData
        {
            /// <summary>
            /// Average MS per frame.
            /// </summary>
            public long AverageMs;

            /// <summary>
            /// Min MS per frame.
            /// </summary>
            public long MinMs;

            /// <summary>
            /// Max MS per frame.
            /// </summary>
            public long MaxMs;

            /// <summary>
            /// Converts frame time to FPS.
            /// </summary>
            /// <param name="ms">Frame time.</param>
            /// <returns></returns>
            public static int FrameTimeToFps(long ms)
            {
                return Mathf.RoundToInt(1000f / ms);
            }
        }

        /// <summary>
        /// Simple object for memory information.
        /// </summary>
        public class MemoryData
        {
            /// <summary>
            /// Total bytes of memory.
            /// </summary>
            public long Total;

            /// <summary>
            /// Allocated bytes.
            /// </summary>
            public long Allocated;

            /// <summary>
            /// Bytes that Mono has allocated.
            /// </summary>
            public long Mono;

            /// <summary>
            /// Bytes of GPU memory.
            /// </summary>
            public long Gpu;

            /// <summary>
            /// Bytes allocated to graphics driver.
            /// </summary>
            public long GraphicsDriver;
        }

        /// <summary>
        /// Number of samples to take.
        /// </summary>
        private const int NUM_SAMPLES = 1024;

        /// <summary>
        /// Frame time data.
        /// </summary>
        public readonly FrameTimeData FrameTime = new FrameTimeData();

        /// <summary>
        /// Memory data.
        /// </summary>
        public readonly MemoryData Memory = new MemoryData();
        
        /// <summary>
        /// Buffer in which to store frame times.
        /// </summary>
        private readonly long[] _frames = new long[NUM_SAMPLES];

        /// <summary>
        /// Index into the frame time buffer.
        /// </summary>
        private int _frameIndex;

        /// <summary>
        /// True iff the frameIndex has already wrapped around.
        /// </summary>
        private bool _wrap;
        
        /// <summary>
        /// Records capture.
        /// </summary>
        private StringBuilder _captureBuilder;

        /// <summary>
        /// Stopwatch used to time.
        /// </summary>
        private readonly Stopwatch _watch = new Stopwatch();

        /// <summary>
        /// True iff the monitor is currently capturing.
        /// </summary>
        public bool IsCapturing { get; private set; }

        /// <summary>
        /// Starts recording a capture.
        /// </summary>
        public void StartCapture()
        {
            if (IsCapturing)
            {
                return;
            }

            IsCapturing = true;

            _captureBuilder = new StringBuilder();

            AppendToCapture();
        }

        /// <summary>
        /// Stops recording a capture.
        /// </summary>
        public string StopCapture()
        {
            if (!IsCapturing)
            {
                return string.Empty;
            }

            IsCapturing = false;

            return _captureBuilder.ToString();
        }

        /// <summary>
        /// Appends a record to the capture.
        /// </summary>
        private void AppendToCapture()
        {
            _captureBuilder.AppendFormat("{0:0.0},{1:0.0},{2:0.0},{3:0},{4:0},{5:0},{6:0},{7:0}\n",
                FrameTime.MinMs, FrameTime.MaxMs, FrameTime.AverageMs,
                Memory.Allocated, Memory.Mono, Memory.Total, Memory.GraphicsDriver, Memory.Gpu);
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
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

            if (IsCapturing)
            {
                AppendToCapture();
            }
        }
    }
}