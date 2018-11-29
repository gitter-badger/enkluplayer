namespace CreateAR.EnkluPlayer.Util
{
    /// <summary>
    /// Data for a tween.
    /// </summary>
    public class TweenData
    {
        /// <summary>
        /// The prop to tween.
        /// </summary>
        public string Prop;
        
        /// <summary>
        /// To parameter.
        /// </summary>
        public object To;

        /// <summary>
        /// True iff the from should be used.
        /// </summary>
        public bool CustomFrom;

        /// <summary>
        /// From parameter.
        /// </summary>
        public object From;

        /// <summary>
        /// Easing equation.
        /// </summary>
        public TweenEasingType Easing = TweenEasingType.Linear;

        /// <summary>
        /// Duration of tween.
        /// </summary>
        public float DurationSec = 1f;

        /// <summary>
        /// Delay in seconds before the tween should start.
        /// </summary>
        public float DelaySec = 0f;
    }
}