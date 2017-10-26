﻿using System;
using Jint;
using Jint.Native;
using Jint.Unity;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class TimeJsApi
    {
        public float now()
        {
            return Time.time;
        }

        public float dt()
        {
            return Time.deltaTime;
        }
    }

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
            SetValue("scene", new UnitySceneManager());
            SetValue("require", new Func<string, JsValue>(
                value => resolver.Resolve(scripts, this, value)));
            SetValue("time", new TimeJsApi());
        }
    }
}