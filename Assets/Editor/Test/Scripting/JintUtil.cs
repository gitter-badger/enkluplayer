using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Jint;
using Jint.Native;

namespace CreateAR.SpirePlayer.Test.Scripting
{
    public static class EngineExtensions
    {
        /// <summary>
        /// Execute a script against the Engine instance.
        /// </summary>
        /// <param name="engine">The Engine instance to Query</param>
        /// <param name="program">The script to execute</param>
        /// <returns>Execution result</returns>
        public static JsValue Run(this Engine engine, string program)
        {
            return engine.GetValue(engine.Execute(program).GetCompletionValue());
        }

        /// <summary>
        /// Execute a script against the Engine instance.
        /// </summary>
        /// <typeparam name="T">Type of return value</typeparam>
        /// <param name="engine">The Engine instance to Query</param>
        /// <param name="program">The script to execute</param>
        /// <returns>Execution result value</returns>
        public static T Run<T>(this Engine engine, string program)
        {
            return engine.Run(program).To<T>();
        }
    }
}
