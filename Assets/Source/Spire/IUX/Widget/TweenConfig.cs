using System;
using UnityEngine;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Maps a tween to a duration.
    /// </summary>
    [Serializable]
    public class TweenProfile
    {
        /// <summary>
        /// TweenType the profile corresponds to.
        /// </summary>
        public TweenType Type;

        /// <summary>
        /// Speed of transition, in seconds.
        /// </summary>
        public float DurationSeconds = 0.2f;
    }

    /// <summary>
    /// Contains configuration for each <c>TweenType</c>.
    /// </summary>
    public class TweenConfig : MonoBehaviour, ITweenConfig
    {
        /// <summary>
        /// All profiles.
        /// </summary>
        public TweenProfile[] Profiles;

        /// <summary>
        /// Retrieves the duration in seconds of a particular tween, or -1 if
        /// no profile could be found.
        /// </summary>
        /// <param name="type">The type of tween.</param>
        /// <returns></returns>
        public float DurationSeconds(TweenType type)
        {
            for (int i = 0, len = Profiles.Length; i < len; i++)
            {
                var profile = Profiles[i];
                if (profile.Type == type)
                {
                    return profile.DurationSeconds;
                }
            }

            return -1f;
        }
    }
}