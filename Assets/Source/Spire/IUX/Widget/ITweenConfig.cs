namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Consistent set of tween durations based around intended user-experience.
    /// 
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

    public interface ITweenConfig
    {
        /// <summary>
        /// Retrieves the duration in seconds of a particular tween, or -1 if
        /// no profile could be found.
        /// </summary>
        /// <param name="type">The type of tween.</param>
        /// <returns></returns>
        float DurationSeconds(TweenType type);
    }
}
