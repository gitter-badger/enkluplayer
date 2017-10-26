using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Basic API for time.
    /// </summary>
    public class TimeJsApi
    {
        /// <summary>
        /// Retrieves the current time.
        /// </summary>
        /// <returns></returns>
        public float now()
        {
            return Time.time;
        }

        /// <summary>
        /// Retrieves the time since last update.
        /// </summary>
        /// <returns></returns>
        public float dt()
        {
            return Time.deltaTime;
        }
    }
}