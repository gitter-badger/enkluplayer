using System;
using CreateAR.SpirePlayer.Scripting;
using Jint;
using Jint.Native;

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
            SetValue("log", new LogJsApi(context));
            SetValue("require", new Func<string, JsValue>(
                value => resolver.Resolve(scripts, this, value)));

            // common apis
            SetValue("v", Vec3Methods.Instance);
            SetValue("vec3", new Func<float, float, float, Vec3>(Vec3Methods.create));
            SetValue("q", QuatMethods.Instance);
            SetValue("quat", new Func<float, float, float, float, Quat>(QuatMethods.create));
            SetValue("time", TimeJsApi.Instance);
        }
    }
}