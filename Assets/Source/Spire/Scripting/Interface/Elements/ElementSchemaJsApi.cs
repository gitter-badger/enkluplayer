using System;
using System.Collections.Generic;
using CreateAR.SpirePlayer.IUX;
using Jint;
using Jint.Native;

namespace CreateAR.SpirePlayer
{
    public class ElementSchemaJsApi
    {
        private class CallbackHelper
        {
            private class CallbackRecord
            {
                public string Key;
                public Type Type;
                public ICallable Callback;
                public Delegate Handler;
            }
            
            private readonly List<CallbackRecord> _callbacks = new List<CallbackRecord>();
            
            private readonly Engine _engine;
            private readonly ElementSchema _schema;

            public CallbackHelper(Engine engine, ElementSchema schema)
            {
                _engine = engine;
                _schema = schema;
            }

            public void Watch<T>(string key, ICallable callback)
            {
                _schema
                    .Get<T>(key)
                    .OnChanged += OnChanged<T>(key, callback);
            }
            
            public void WatchOnce<T>(string key, ICallable callback)
            {
                _schema
                    .Get<T>(key)
                    .OnChanged += OnChangedOnce<T>(callback);
            }
            
            public void Unwatch<T>(string key, ICallable callback)
            {
                _schema
                    .Get<T>(key)
                    .OnChanged -= OnChanged<T>(key, callback);
            }

            private Action<ElementSchemaProp<T>, T, T> OnChanged<T>(
                string key,
                ICallable callback)
            {
                var record = Record<T>(key, callback);
                if (null != record)
                {
                    return (Action<ElementSchemaProp<T>, T, T>) record.Handler;
                }
                
                Action<ElementSchemaProp<T>, T, T> handler = null;
                handler = (prop, prev, next) =>
                {
                    callback.Call(null, new JsValue[2]
                    {
                        JsValue.FromObject(_engine, prev),
                        JsValue.FromObject(_engine, next)
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
            
            private Action<ElementSchemaProp<T>, T, T> OnChangedOnce<T>(ICallable callback)
            {
                Action<ElementSchemaProp<T>, T, T> handler = null;
                handler = (prop, prev, next) =>
                {
                    prop.OnChanged -= handler;
                    
                    callback.Call(null, new JsValue[2]
                    {
                        JsValue.FromObject(_engine, prev),
                        JsValue.FromObject(_engine, next)
                    });
                };

                return handler;
            }
            
            private CallbackRecord Record<T>(string key, ICallable callback)
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
        
        private readonly ElementSchema _schema;

        private readonly CallbackHelper _callbackHelper;

        public ElementSchemaJsApi(Engine engine, ElementSchema schema)
        {
            _schema = schema;
            _callbackHelper = new CallbackHelper(engine, schema);
        }

        public float getNumber(string key)
        {
            return _schema.Get<float>(key).Value;
        }
        
        public void setNumber(string key, float value)
        {
            _schema.Set(key, value);
        }
        
        public string getString(string key)
        {
            return _schema.Get<string>(key).Value;
        }
        
        public void setString(string key, string value)
        {
            _schema.Set(key, value);
        }
        
        public bool getBool(string key)
        {
            return _schema.Get<bool>(key).Value;
        }
        
        public void setBool(string key, bool value)
        {
            _schema.Set(key, value);
        }
        
        public void watchString(string key, ICallable callback)
        {
            _callbackHelper.Watch<string>(key, callback);
        }
        
        public void watchStringOnce(string key, ICallable callback)
        {
            _callbackHelper.WatchOnce<string>(key, callback);
        }
        
        public void unwatchString(string key, ICallable callback)
        {
            _callbackHelper.Unwatch<string>(key, callback);
        }
        
        public void watchBool(string key, ICallable callback)
        {
            _callbackHelper.Watch<bool>(key, callback);
        }
        
        public void watchBoolOnce(string key, ICallable callback)
        {
            _callbackHelper.WatchOnce<bool>(key, callback);
        }
        
        public void unwatchBool(string key, ICallable callback)
        {
            _callbackHelper.Unwatch<bool>(key, callback);
        }
        
        public void watchNumber(string key, ICallable callback)
        {
            _callbackHelper.Watch<float>(key, callback);
        }
        
        public void watchNumberOnce(string key, ICallable callback)
        {
            _callbackHelper.WatchOnce<float>(key, callback);
        }
        
        public void unwatchFloat(string key, ICallable callback)
        {
            _callbackHelper.Unwatch<float>(key, callback);
        }
    }
}