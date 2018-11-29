using CreateAR.EnkluPlayer.Util;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Js API for creating tweens.
    /// </summary>
    [JsInterface("tween")]
    public class TweenManagerJsApi
    {
        /// <summary>
        /// Float type.
        /// </summary>
        public const string FLOAT = "float";

        /// <summary>
        /// Manages tweens.
        /// </summary>
        private readonly ITweenManager _tweens;

        /// <summary>
        /// Constructor.
        /// </summary>
        public TweenManagerJsApi(ITweenManager tweens)
        {
            _tweens = tweens;
        }

        /// <summary>
        /// Creates a tween for a number.
        /// </summary>
        public TweenJs number(ElementJs element, string prop)
        {
            return new TweenJs(_tweens, element.Element, prop, FLOAT);
        }
    }
}