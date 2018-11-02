using CreateAR.EnkluPlayer.IUX;
using Jint;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Creates Vine and Behavior scripts on GameObjects.
    /// </summary>
    public interface IScriptFactory
    {
        /// <summary>
        /// Creates & Initializes a VineScript on a given GameObject.
        /// </summary>
        /// <returns></returns>
        VineScript Vine(Widget widget, EnkluScript script);

        /// <summary>
        /// Creates & Initializes a BehaviorScript or creates one.
        /// </summary>
        /// <returns></returns>
        BehaviorScript Behavior(
            Widget widget,
            IElementJsCache jsCache,  
            UnityScriptingHost host,
            EnkluScript script);
    }
}