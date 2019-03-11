using System;

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
    
    public interface TweenConfig
    {
        /// <summary>
        /// All profiles.
        /// </summary>
        TweenProfile[] Profiles { get; }

        /// <summary>
        /// Retrieves the duration in seconds of a particular tween, or -1 if
        /// no profile could be found.
        /// </summary>
        /// <param name="type">The type of tween.</param>
        /// <returns></returns>
        float DurationSeconds(TweenType type);
    }
}