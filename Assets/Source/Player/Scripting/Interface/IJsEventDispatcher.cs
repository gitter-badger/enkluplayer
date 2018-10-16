using System;
using Jint;
using Jint.Native;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Describes an object that can dispatch events.
    /// </summary>
    public interface IJsEventDispatcher
    {
        /// <summary>
        /// Adds a handler for an event type.
        /// </summary>
        /// <param name="engine">The Jint engine.</param>
        /// <param name="type">The type of event.</param>
        /// <param name="fn">The function to handle the event.</param>
        void on(Engine engine, string type, Func<JsValue, JsValue[], JsValue> fn);

        /// <summary>
        /// Removes all handlers for a type.
        /// </summary>
        /// <param name="type">The type of event.</param>
        void off(string type);

        /// <summary>
        /// Removes a specific event handler.
        /// </summary>
        /// <param name="engine">The Jint engine.</param>
        /// <param name="type">The type.</param>
        /// <param name="fn">The handler.</param>
        void off(Engine engine, string type, Func<JsValue, JsValue[], JsValue> fn);
    }
}