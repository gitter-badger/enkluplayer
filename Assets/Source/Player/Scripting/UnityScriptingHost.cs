using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.Scripting;
using Enklu.Data;
using Jint;
using Jint.Native;

namespace CreateAR.EnkluPlayer
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
            IScriptManager scripts,
            Action<Options> engineOptions)
            : base(engineOptions)
        {
            SetValue("log", new LogJsApi(context));
            SetValue("require", new Func<string, JsValue>(
                value => resolver.Resolve(scripts, this, value)));

            // common apis
            SetValue("v", Vec3Methods.Instance);
            SetValue("vec3", new Func<float, float, float, Vec3>(Vec3Methods.create));
            SetValue("vec2", new Func<float, float, Vec2>((x, y) => new Vec2(x, y)));
            SetValue("q", QuatMethods.Instance);
            SetValue("quat", new Func<float, float, float, float, Quat>(QuatMethods.create));
            SetValue("c", Col4Methods.Instance);
            SetValue("col", new Func<float, float, float, float, Col4>(Col4Methods.create));
            SetValue("time", TimeJsApi.Instance);
        }
    }
}