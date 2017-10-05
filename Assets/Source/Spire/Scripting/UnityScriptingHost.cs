using System;
using Jint;
using Jint.Native;
using Jint.Unity;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Hosts scripts and provides a default Unity API.
    /// </summary>
    public class UnityScriptingHost : Engine
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public UnityScriptingHost(
            object context,
            IScriptManager scripts)
            : base(options => options.AllowClr())
        {
            SetValue("log", new JsLogWrapper(context));
            SetValue("scene", new UnitySceneManager());
            
            SetValue("require", new Func<string, JsValue>(
                new SpireScriptRequireResolver(scripts, this).Resolve));
        }
    }
}