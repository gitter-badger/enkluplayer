using System;
using UnityEngine;

namespace CreateAR.SpirePlayer.Test
{
    public class WorldScanSimTest : MonoBehaviour
    {
        private readonly WorldScanPipeline _pipeline = new WorldScanPipeline(new WorldScanPipelineConfiguration());

        private DateTime _last = DateTime.MinValue;

        public int FrequencyMs = 1000;

        public bool Run = false;
        
        private void Update()
        {
            if (Run)
            {
                _pipeline.Start();
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
