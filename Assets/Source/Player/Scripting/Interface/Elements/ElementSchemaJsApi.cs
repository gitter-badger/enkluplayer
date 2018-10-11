using System;
using System.Collections.Generic;
using CreateAR.EnkluPlayer.IUX;
using Jint;
using Jint.Native;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// JS Api for element schema.
    /// </summary>
    public class ElementSchemaJsApi
    {
        /// <summary>
        /// Helps track and call callbacks.
        /// </summary>
        private class CallbackHelper
        {
            /// <summary>
            /// Tracks a callback.
            /// </summary>
            private class CallbackRecord
            {
                /// <summary>
                /// The schema prop key.
                /// </summary>
                public string Key;
                
                /// <summary>
                /// The type associated with this record.
                /// </summary>
                public Type Type;
                
                /// <summary>
                /// The callback.
                /// </summary>
                public Func<JsValue, JsValue[], JsValue> Callback;
                
                /// <summary>
                /// Handler to call.
                /// </summary>
                public Delegate Handler;
            }
            
            /// <summary>
            /// Lookups from type to callbacks.
            /// </summary>
            private readonly List<CallbackRecord> _callbacks = new List<CallbackRecord>();
            
            /// <summary>
            /// The schema to provide an API for.
            /// </summary>
            private readonly ElementSchema _schema;

            /// <summary>
            /// Constructor.
            /// </summary>
            public CallbackHelper(ElementSchema schema)
            {
                _schema = schema;
            }

            /// <summary>
            /// Watches a value.
            /// </summary>
            /// <param name="engine">The associated engine.</param>
            /// <param name="key">The string key.</param>
            /// <param name="callback">The JS callback to call.</param>
            public void Watch<T>(Engine engine, string key, Func<JsValue, JsValue[], JsValue> callback)
            {
                _schema
                    .Get<T>(key)
                    .OnChanged += OnChanged<T>(engine, key, callback);
            }

            /// <summary>
            /// Adds a watcher that is only called once.
            /// </summary>
            /// <param name="engine">The associated engine.</param>
            /// <param name="key">The string key.</param>
            /// <param name="callback">The JS callback to call.</param>
            public void WatchOnce<T>(Engine engine, string key, Func<JsValue, JsValue[], JsValue> callback)
            {
                _schema
                    .Get<T>(key)
                    .OnChanged += OnChangedOnce<T>(engine, callback);
            }

            /// <summary>
            /// Removes a watcher.
            /// </summary>
            /// <param name="engine">The associated engine.</param>
            /// <param name="key">The string key.</param>
            /// <param name="callback">The JS callback.</param>
            public void Unwatch<T>(Engine engine, string key, Func<JsValue, JsValue[], JsValue> callback)
            {
                _schema
                    .Get<T>(key)
                    .OnChanged -= OnChanged<T>(engine, key, callback);
            }

            /// <summary>
            /// Called when a prop has changed.
            /// </summary>
            /// <param name="engine">The associated engine.</param>
            /// <param name="key">The string key.</param>
            /// <param name="callback">The JS callback.</param>
            private Action<ElementSchemaProp<T>, T, T> OnChanged<T>(
                Engine engine,
                string key,
                Func<JsValue, JsValue[], JsValue> callback)
            {
                var record = Record<T>(key, callback);
                if (null != record)
                {
                    return (Action<ElementSchemaProp<T>, T, T>) record.Handler;
                }
                
                Action<ElementSchemaProp<T>, T, T> handler = null;
                handler = (prop, prev, next) =>
                {
                    callback(null, new []
                    {
                        JsValue.FromObject(engine, prev),
                        JsValue.FromObject(engine, next)
                    });
                };
                
                _callbacks.Add(new CallbackRecord
                {
                    Key = key,
                    Type = typeof(T),
                    Callback = callback,
                    Handler = handler
                });

                return handler;
            }

            /// <summary>
            /// Called when a prop has changed once.
            /// </summary>
            /// <param name="engine">The associated engine.</param>
            /// <param name="callback">The callback.</param>
            private Action<ElementSchemaProp<T>, T, T> OnChangedOnce<T>(
                Engine engine,
                Func<JsValue, JsValue[], JsValue> callback)
            {
                Action<ElementSchemaProp<T>, T, T> handler = null;
                handler = (prop, prev, next) =>
                {
                    prop.OnChanged -= handler;
                    
                    callback(null, new []
                    {
                        JsValue.FromObject(engine, prev),
                        JsValue.FromObject(engine, next)
                    });
                };

                return handler;
            }
            
            /// <summary>
            /// Retrieves a record.
            /// </summary>
            /// <param name="key">The string key.</param>
            /// <param name="callback">The callback.</param>
            private CallbackRecord Record<T>(string key, Func<JsValue, JsValue[], JsValue> callback)
            {
                for (int i = 0, len = _callbacks.Count; i < len; i++)
                {
                    var record = _callbacks[i];
                    if (record.Callback == callback
                        && record.Type == typeof(T)
                        && record.Key == key)
                    {
                        return record;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// The schema.
        /// </summary>
        private readonly ElementSchema _schema;

        /// <summary>
        /// Tracks callbacks.
        /// </summary>
        private readonly CallbackHelper _callbackHelper;

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="schema">The schema to wrap.</param>
        public ElementSchemaJsApi(ElementSchema schema)
        {
            _schema = schema;
            _callbackHelper = new CallbackHelper(schema);
        }

        /// <summary>
        /// Retrieves the value of a number prop.
        /// </summary>
        /// <param name="key">The string key.</param>
        public float getNumber(string key)
        {
            return _schema.Get<float>(key).Value;
        }

        /// <summary>
        /// Retrieves the value of an Element's own number prop, with a customizeable default.
        /// </summary>
        /// <param name="key">The string key.</param>
        /// <param name="default">default(float) if unprovided.</param>
        /// <returns></returns>
        public float getOwnNumber(string key, float @default = default(float))
        {
            return _schema.GetOwn(key, @default).Value;
        }
        
        /// <summary>
        /// Sets the value of a number prop.
        /// </summary>
        /// <param name="key">The string key.</param>
        /// <param name="value">The value at which to set it.</param>
        public void setNumber(string key, float value)
        {
            _schema.Set(key, value);
        }
        
        /// <summary>
        /// Retrieves the value of a string prop.
        /// </summary>
        /// <param name="key">The string key.</param>
        /// <returns></returns>
        public string getString(string key)
        {
            return _schema.Get<string>(key).Value;
        }

        /// <summary>
        /// Retrieves the value of an Element's own string prop, with a customizeable default.
        /// </summary>
        /// <param name="key">The string key.</param>
        /// <param name="default">default(string) if unprovided.</param>
        /// <returns></returns>
        public string getOwnString(string key, string @default = default(string))
        {
            return _schema.GetOwn(key, @default).Value;
        }
        
        /// <summary>
        /// Sets the value of a string prop.
        /// </summary>
        /// <param name="key">The string key.</param>
        /// <param name="value">The string value.</param>
        public void setString(string key, string value)
        {
            _schema.Set(key, value);
        }
        
        /// <summary>
        /// Retrieves the value of a bool prop.
        /// </summary>
        /// <param name="key">The string key.</param>
        /// <returns></returns>
        public bool getBool(string key)
        {
            return _schema.Get<bool>(key).Value;
        }
        
        /// <summary>
        /// Sets the value of a bool prop.
        /// </summary>
        /// <param name="key">The string key.</param>
        /// <param name="value">The value.</param>
        public void setBool(string key, bool value)
        {
            _schema.Set(key, value);
        }

        /// <summary>
        /// Retrieves the value of an Element's own bool prop, with a customizeable default.
        /// </summary>
        /// <param name="key">The string key.</param>
        /// <param name="default">default(bool) if unprovided.</param>
        /// <returns></returns>
        public bool getOwnBool(string key, bool @default = default(bool))
        {
            return _schema.GetOwn(key, @default).Value;
        }
        
        /// <summary>
        /// Adds a watcher to a prop.
        /// </summary>
        public void watchString(Engine engine, string key, Func<JsValue, JsValue[], JsValue> callback)
        {
            _callbackHelper.Watch<string>(engine, key, callback);
        }
        
        /// <summary>
        /// Adds a one time watcher to a prop.
        /// </summary>
        public void watchStringOnce(Engine engine, string key, Func<JsValue, JsValue[], JsValue> callback)
        {
            _callbackHelper.WatchOnce<string>(engine, key, callback);
        }
        
        /// <summary>
        /// Removes a watcher from a prop.
        /// </summary>
        public void unwatchString(Engine engine, string key, Func<JsValue, JsValue[], JsValue> callback)
        {
            _callbackHelper.Unwatch<string>(engine, key, callback);
        }
        
        /// <summary>
        /// Adds a watcher to a prop.
        /// </summary>
        public void watchBool(Engine engine, string key, Func<JsValue, JsValue[], JsValue> callback)
        {
            _callbackHelper.Watch<bool>(engine, key, callback);
        }
        
        /// <summary>
        /// Adds a one time watcher to a prop.
        /// </summary>
        public void watchBoolOnce(Engine engine, string key, Func<JsValue, JsValue[], JsValue> callback)
        {
            _callbackHelper.WatchOnce<bool>(engine, key, callback);
        }
        
        /// <summary>
        /// Removes a watcher from a prop.
        /// </summary>
        public void unwatchBool(Engine engine, string key, Func<JsValue, JsValue[], JsValue> callback)
        {
            _callbackHelper.Unwatch<bool>(engine, key, callback);
        }

        /// <summary>
        /// Adds a watcher to a prop.
        /// </summary>
        public void watchNumber(Engine engine, string key, Func<JsValue, JsValue[], JsValue> callback)
        {
            _callbackHelper.Watch<float>(engine, key, callback);
        }
        
        /// <summary>
        /// Adds a watcher to a prop.
        /// </summary>
        public void watchNumberOnce(Engine engine, string key, Func<JsValue, JsValue[], JsValue> callback)
        {
            _callbackHelper.WatchOnce<float>(engine, key, callback);
        }
        
        /// <summary>
        /// Removes a watcher from a prop.
        /// </summary>
        public void unwatchNumber(Engine engine, string key, Func<JsValue, JsValue[], JsValue> callback)
        {
            _callbackHelper.Unwatch<float>(engine, key, callback);
        }
    }
}