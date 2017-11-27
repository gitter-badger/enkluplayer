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
            IScriptRequireResolver resolver,
            IScriptManager scripts)
            : base(options =>
            {
                options.AllowClr();
                options.CatchClrExceptions(exception =>
                {
                    throw exception;
                });
            })
        {
            SetValue("log", new JsLogWrapper(context));
            SetValue("require", new Func<string, JsValue>(
                value => resolver.Resolve(scripts, this, value)));
            SetValue("time", new TimeJsApi());
        }
    }
}