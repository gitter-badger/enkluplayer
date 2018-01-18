#if UNITY_EDITOR
using System;
using CreateAR.Commons.Unity.Http;
using UnityEngine;

namespace CreateAR.SpirePlayer.Test
{
    /// <summary>
    /// For testing the world scan pipeline.
    /// </summary>
    public class WorldScanSimTest : InjectableMonoBehaviour
    {
        [Inject]
        public IBootstrapper Bootstrapper { get; set; }

        [Inject]
        public IHttpService Http { get; set; }

        private WorldScanPipeline _pipeline;

        private DateTime _last = DateTime.MinValue;

        public int FrequencyMs = 1000;

        public bool Run = false;

        private void Start()
        {
            _pipeline = new WorldScanPipeline(
                Bootstrapper,
                Http,
                new WorldScanPipelineConfiguration());
        }
        
        private void Update()
        {
            if (Run)
            {
                _pipeline.Start("Sim");
            }
            else
            {
                _pipeline.Stop();
            }

            var now = DateTime.Now;
            if (Run && now.Subtract(_last).TotalMilliseconds > FrequencyMs)
            {
                _last = now;

                _pipeline.Scan(gameObject);
            }
        }
    }
}
#endif