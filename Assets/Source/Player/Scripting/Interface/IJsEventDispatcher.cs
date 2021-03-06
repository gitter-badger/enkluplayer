﻿using Enklu.Orchid;

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
        /// <param name="type">The type of event.</param>
        /// <param name="fn">The function to handle the event.</param>
        void on(string type, IJsCallback fn);

        /// <summary>
        /// Removes all handlers for a type.
        /// </summary>
        /// <param name="type">The type of event.</param>
        void off(string type);

        /// <summary>
        /// Removes a specific event handler.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="fn">The handler.</param>
        void off(string type, IJsCallback fn);

        /// <summary>
        /// Dispatches an event.
        /// </summary>
        /// <param name="eventType">The type of event.</param>
        void dispatch(string eventType);

        /// <summary>
        /// Dispatches an event.
        /// </summary>
        /// <param name="eventType">The type of event.</param>
        /// <param name="evt">The event.</param>
        void dispatch(string eventType, object evt);
    }
}