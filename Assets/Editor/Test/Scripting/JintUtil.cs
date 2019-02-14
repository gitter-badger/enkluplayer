using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Jint;
using Jint.Native;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    public static class JintUtil
    {
        /// <summary>
        /// Static helper method for creating a <see cref="Engine"/> using default configuration.
        /// </summary>
        public static Engine NewEngine(bool enableDebug = false)
        {
            return new Engine(options =>
            {
                options.AllowClr();
                options.CatchClrExceptions(exception =>
                {
                    throw exception;
                });

                // Debugging Configuration
                options.DebugMode(enableDebug);
                options.AllowDebuggerStatement(enableDebug);
            });
        }
    }

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
