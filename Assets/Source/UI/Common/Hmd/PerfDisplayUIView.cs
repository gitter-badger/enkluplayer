using System;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// UI view with performance information.
    /// </summary>
    public class PerfDisplayUIView : MonoBehaviourIUXController
    {
        /// <summary>
        /// Underlying performance metrics object.
        /// </summary>
        private PerfMonitor _monitor;

        /// <summary>
        /// Injected controls.
        /// </summary>
        [InjectElements("..btn-close")]
        public ButtonWidget BtnClose { get; set; }

        [InjectElements("..slt-tab")]
        public SelectWidget SltTab { get; set; }

        [InjectElements("..tab-frame")]
        public ContainerWidget TabFrame { get; set; }

        [InjectElements("..tab-memory")]
        public ContainerWidget TabMemory { get; set; }

        [InjectElements("..tab-capture")]
        public ContainerWidget TabCapture { get; set; }

        [InjectElements("..txt-ave")]
        public TextWidget TxtFrameAve { get; set; }

        [InjectElements("..txt-min")]
        public TextWidget TxtFrameMin { get; set; }

        [InjectElements("..txt-max")]
        public TextWidget TxtFrameMax { get; set; }

        [InjectElements("..txt-total")]
        public TextWidget TxtMemTotal { get; set; }

        [InjectElements("..txt-allocated")]
        public TextWidget TxtMemAllocated { get; set; }

        [InjectElements("..txt-mono")]
        public TextWidget TxtMemMono { get; set; }

        [InjectElements("..txt-gpu")]
        public TextWidget TxtMemGpu { get; set; }

        [InjectElements("..txt-graphics")]
        public TextWidget TxtMemGraphics { get; set; }

        /// <summary>
        /// Called when the perf hud should be closed.
        /// </summary>
        public event Action OnClose;

        /// <inheritdoc />
        protected override void AfterElementsCreated()
        {
            base.AfterElementsCreated();

            BtnClose.OnActivated += _ =>
            {
                if (null != OnClose)
                {
                    OnClose();
                }
            };

            SltTab.OnValueChanged += _ =>
            {
                var selection = SltTab.Selection.Value;
                TabFrame.Schema.Set("visible", selection == "frame");
                TabMemory.Schema.Set("visible", selection == "memory");
                TabCapture.Schema.Set("visible", selection == "capture");
            };
        }

        /// <inheritdoc />
        private void Start()
        {
            _monitor = gameObject.AddComponent<PerfMonitor>();
        }
        
        /// <inheritdoc />
        private void Update()
        {
            if (null == SltTab)
            {
                return;
            }
            
            var tabName = SltTab.Selection.Value;
            if ("frame" == tabName)
            {
                UpdateFrame();
            }
            else if ("memory" == tabName)
            {
                UpdateMemory();
            }
        }

        /// <summary>
        /// Updates memory info.
        /// </summary>
        private void UpdateMemory()
        {
            // update memory
            TxtMemTotal.Label = string.Format("Total: {0:#0.0} MB",
                PerfMonitor.MemoryData.BytesToMb(_monitor.Memory.Total));
            TxtMemAllocated.Label = string.Format("Allocated: {0:#0.0} MB",
                PerfMonitor.MemoryData.BytesToMb(_monitor.Memory.Allocated));
            TxtMemMono.Label = string.Format("Mono: {0:#0.0} MB",
                PerfMonitor.MemoryData.BytesToMb(_monitor.Memory.Mono));
            TxtMemGpu.Label = string.Format("Gpu: {0:#0.0} MB",
                PerfMonitor.MemoryData.BytesToMb(_monitor.Memory.Gpu));
            TxtMemGraphics.Label = string.Format("Driver: {0:#0.0} MB",
                PerfMonitor.MemoryData.BytesToMb(_monitor.Memory.GraphicsDriver));
        }

        /// <summary>
        /// Updates frame info.
        /// </summary>
        private void UpdateFrame()
        {
            // update frame
            TxtFrameAve.Label = string.Format("Ave: {0} ms ({1} FPS)",
                _monitor.FrameTime.AverageMs,
                PerfMonitor.FrameTimeData.FrameTimeToFps(_monitor.FrameTime.AverageMs));
            TxtFrameMin.Label = string.Format("Min: {0} ms ({1} FPS)",
                _monitor.FrameTime.MinMs,
                PerfMonitor.FrameTimeData.FrameTimeToFps(_monitor.FrameTime.MinMs));
            TxtFrameMax.Label = string.Format("Max: {0} ms ({1} FPS)",
                _monitor.FrameTime.MaxMs,
                PerfMonitor.FrameTimeData.FrameTimeToFps(_monitor.FrameTime.MaxMs));
        }
    }
}