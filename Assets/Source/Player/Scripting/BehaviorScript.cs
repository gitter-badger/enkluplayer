using CreateAR.EnkluPlayer.IUX;
using Jint;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// This object is able to run a JS script on an Element similar to a MonoBehaviour.
    /// </summary>
    public abstract class BehaviorScript : Script
    {
        /// <summary>
        /// Initializes the host.
        /// </summary>
        /// <param name="jsCache">Js cache.</param>
        /// <param name="factory">Creates elements.</param>
        /// <param name="engine">JS Engine.</param>
        /// <param name="script">The script to execute.</param>
        /// <param name="widget">The widget.</param>
        public abstract void Initialize(
            IElementJsCache jsCache,
            IElementJsFactory factory,
            Engine engine,
            EnkluScript script,
            Widget widget);
    }
}