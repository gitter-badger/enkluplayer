using System;
using UnityEngine;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Consistent set of tween durations based around intended user-experience.
    /// </summary>
    public enum TweenType
    {
        Instant,
        Responsive,
        Deliberate,
        Pronounced

        // !!!
        // NOTE: Before adding a new TweenType to the enum, consider the
        //       goals of the user experience you are trying to achieve,
        //       and why those goals do not fit the existing tween types.
        // !!!
    }
    
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
    
    [Serializable]
    public class TweenConfig
    {
        /// <summary>
        /// Backing Unity serialized field.
        /// </summary>
        [SerializeField]
        private TweenProfile[] _profiles;
        
        /// <summary>
        /// All profiles.
        /// </summary>
        public TweenProfile[] Profiles { get { return _profiles; } }

        /// <summary>
        /// Retrieves the duration in seconds of a particular tween, or -1 if
        /// no profile could be found.
        /// </summary>
        /// <param name="type">The type of tween.</param>
        /// <returns></returns>
        public float DurationSeconds(TweenType type)
        {
            if (Profiles != null)
            {
                for (int i = 0, len = Profiles.Length; i < len; i++)
                {
                    var profile = Profiles[i];
                    if (profile.Type == type)
                    {
                        return profile.DurationSeconds;
                    }
                }
            }

            return -1f;
        }
    }
}