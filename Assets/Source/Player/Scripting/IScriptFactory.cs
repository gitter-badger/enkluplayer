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
        VineScript Vine(Element element, EnkluScript script);

        /// <summary>
        /// Creates & Initializes a BehaviorScript or creates one.
        /// </summary>
        /// <returns></returns>
        BehaviorScript Behavior(
            Element element,
            IElementJsCache jsCache,  
            UnityScriptingHost host,
            EnkluScript script);
    }
}