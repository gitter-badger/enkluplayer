﻿using System;
using System.Collections;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.SendEmail;
using UnityEngine;

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
        /// Dependencies.
        /// </summary>
        [Inject]
        public ApiController Api { get; set; }
        [Inject]
        public ApplicationConfig Config { get; set; }
        [Inject]
        public PerfMetricsCollector MetricsCollector { get; set; }

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

        [InjectElements("..btn-one")]
        public ButtonWidget BtnOneMinute { get; set; }

        [InjectElements("..btn-two")]
        public ButtonWidget BtnTwoMinutes { get; set; }

        [InjectElements("..btn-three")]
        public ButtonWidget BtnThreeMinutes { get; set; }

        [InjectElements("..txt-progress")]
        public TextWidget TxtProgress { get; set; }

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

            TxtProgress.LocalVisible = false;
            BtnOneMinute.OnActivated += _ => StartCoroutine(StartCapture(60));
            BtnTwoMinutes.OnActivated += _ => StartCoroutine(StartCapture(120));
            BtnThreeMinutes.OnActivated += _ => StartCoroutine(StartCapture(180));
        }

        /// <inheritdoc />
        private void Start()
        {
            _monitor = MetricsCollector.PerfMonitor;
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
            else if ("capture" == tabName)
            {
                UpdateCapture();
            }
        }

        /// <summary>
        /// Updates capture.
        /// </summary>
        private void UpdateCapture()
        {

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

        /// <summary>
        /// Starts the capture.
        /// </summary>
        /// <param name="secs">The number of seconds to capture.</param>
        /// <returns></returns>
        private IEnumerator StartCapture(int secs)
        {
            _monitor.StartCapture();

            BtnOneMinute.LocalVisible = BtnTwoMinutes.LocalVisible = BtnThreeMinutes.LocalVisible = false;
            TxtProgress.LocalVisible = true;
            TxtProgress.Label = "Capture in progress. Do not close window.";

            yield return new WaitForSecondsRealtime(secs);

            TxtProgress.Label = "Capture complete. Uploading...";

            var id = StringExtensions.RandomIdentifier(4);
            var dump = _monitor.StopCapture();

            // send it in
            Api
                .Utilities
                .SendEmail(new Request
                {
                    Body = dump,
                    EmailAddress = Config.Debug.DumpEmail,
                    Subject = string.Format("Performance Capture ({0}): {1}", id, DateTime.Now),
                    FirstName = ""
                })
                .OnSuccess(res =>
                {
                    if (res.Payload.Success)
                    {
                        TxtProgress.Label = string.Format("Capture '{0}' sent to '{1}'.", id, Config.Debug.DumpEmail);
                    }
                    else
                    {
                        TxtProgress.Label = string.Format("There was an error: {0}", res.Payload.Error);
                    }
                })
                .OnFailure(ex =>
                {
                    TxtProgress.Label = string.Format("There was an error: {0}", ex.Message);

                    Log.Error(this, "Could not send performance capture to {0} : {1}.",
                        Config.Debug.DumpEmail,
                        ex);
                });
        }
    }
}