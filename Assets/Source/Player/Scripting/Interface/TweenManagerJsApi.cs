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
        /// Types of props.
        /// </summary>
        public const string FLOAT = "float";
        public const string VEC3 = "vec3";
        public const string COL4 = "col4";

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

        /// <summary>
        /// Creates a tween for a vector.
        /// </summary>
        public TweenJs vec3(ElementJs element, string prop)
        {
            return new TweenJs(_tweens, element.Element, prop, VEC3);
        }

        /// <summary>
        /// Creates a tween for a color.
        /// </summary>
        public TweenJs col4(ElementJs element, string prop)
        {
            return new TweenJs(_tweens, element.Element, prop, COL4);
        }
    }
}