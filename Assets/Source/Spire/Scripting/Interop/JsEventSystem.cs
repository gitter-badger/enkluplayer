namespace CreateAR.SpirePlayer.Scripting
{
    /// <summary>
    /// Base class for all JsInterfaces that provide events. This helper allows any derivation to automatically
    ///     receive an `events` property which a developer using the JsApi can work with, instead of raw strings.
    /// </summary>
    /// <typeparam name="T">Object with definitions for events to be provided to the JsApi</typeparam>
    public abstract class JsEventSystem<T> : InjectableMonoBehaviour where T : new()
    {
        /// <summary>
        /// Used by the JsApi. Contains all the events this JsEventSystem will be capable of emitting.
        /// </summary>
        public T events;

        public JsEventSystem()
        {
            events = new T();
        }
    }
}