using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Exposes a simple interface to Unity's Animator. JS Scripts can get/set
    /// parameters as needed to trigger different animation states. The current
    /// animation clip's name is available for querying, to check what
    /// animation is currently active.
    /// </summary>
    public class AnimatorJsApi
    {
        /// <summary>
        /// Schema to back animator properties.
        /// </summary>
        private readonly ElementSchema _schema;
        
        /// <summary>
        /// Underlying Unity Animator to wrap.
        /// </summary>
        private readonly Animator _animator;

        /// <summary>
        /// List of available parameter names.
        /// </summary>
        private readonly string[] _parameterNames;
        
        /// <summary>
        /// Backing schema props
        /// </summary>
        private readonly List<ElementSchemaProp<float>> _propsFloat = new List<ElementSchemaProp<float>>();
        private readonly List<ElementSchemaProp<int>> _propsInt = new List<ElementSchemaProp<int>>();
        private readonly List<ElementSchemaProp<bool>> _propsBool = new List<ElementSchemaProp<bool>>();

        /// <summary>
        /// List of available parameter names.
        /// </summary>
        public string[] parameterNames
        {
            get { return _parameterNames; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="animator"></param>
        public AnimatorJsApi(ElementSchema schema, Animator animator)
        {
            _schema = schema;
            _animator = animator;

            _parameterNames = new string[_animator.parameterCount];
            for (var i = 0; i < _animator.parameters.Length; i++)
            {
                _parameterNames[i] = _animator.parameters[i].name;
                
                var prop = _schema.GetOwn(ToSchemaName(_parameterNames[i]));

                if (prop != null)
                {
                    if (prop.Type == typeof(float))
                    {
                        var propFloat = (ElementSchemaProp<float>) prop;
                        
                        _propsFloat.Add(propFloat);
                        propFloat.OnChanged += OnSchemaFloatChange;
                    } 
                    else if (prop.Type == typeof(int))
                    {
                        var propInt = (ElementSchemaProp<int>) prop;
                        
                        _propsInt.Add(propInt);
                        propInt.OnChanged += OnSchemaIntChange;
                    }
                    else if (prop.Type == typeof(bool))
                    {
                        var propBool = (ElementSchemaProp<bool>) prop;
                        
                        _propsBool.Add(propBool);
                        propBool.OnChanged += OnSchemaBoolChange;
                    }
                }
            }
        }

        /// <summary>
        /// Deconstructor
        /// </summary>
        ~AnimatorJsApi()
        {
            for (var i = 0; i < _propsFloat.Count; i++)
            {
                _propsFloat[i].OnChanged -= OnSchemaFloatChange;
            }
            
            for (var i = 0; i < _propsInt.Count; i++)
            {
                _propsInt[i].OnChanged -= OnSchemaIntChange;
            }
            
            for (var i = 0; i < _propsBool.Count; i++)
            {
                _propsBool[i].OnChanged -= OnSchemaBoolChange;
            }
        }

        /// <summary>
        /// Gets the current value for a parameter.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool getBool(string name)
        {
            return _animator.GetBool(name);
        }

        /// <summary>
        /// Sets the value for a parameter.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void setBool(string name, bool value)
        {
            var prop = GetSchemaProp(_propsBool, name);
            if (prop == null)
            {
                prop = _schema.GetOwn(ToSchemaName(name), value);
                prop.OnChanged += OnSchemaBoolChange;
                
                _propsBool.Add(prop);
            }
            
            prop.Value = value;
        }

        /// <summary>
        /// Gets the current value for a parameter.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int getInteger(string name)
        {
            return _animator.GetInteger(name);
        }

        /// <summary>
        /// Sets the value for a parameter.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void setInteger(string name, int value)
        {
            var prop = GetSchemaProp(_propsInt, name);
            if (prop == null)
            {
                prop = _schema.GetOwn(ToSchemaName(name), value);
                prop.OnChanged += OnSchemaIntChange;
                
                _propsInt.Add(prop);
            }
            
            prop.Value = value;
        }

        /// <summary>
        /// Gets the current value for a parameter.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public float getFloat(string name)
        {
            return _animator.GetFloat(name);
        }

        /// <summary>
        /// Sets the value for a parameter.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void setFloat(string name, float value)
        {
            var prop = GetSchemaProp(_propsFloat, name);
            if (prop == null)
            {
                prop = _schema.GetOwn(ToSchemaName(name), value);
                prop.OnChanged += OnSchemaFloatChange;
                
                _propsFloat.Add(prop);
            }
            
            prop.Value = value;
        }

        /// <summary>
        /// Returns the <see cref="AnimationClip"/> name for the playing animation with the highest weight.
        /// </summary>
        /// <param name="layer">Optional - layer to check on.</param>
        /// <returns></returns>
        public string getCurrentClipName(int layer = 0)
        {
            return _animator.GetCurrentAnimatorClipInfo(layer)[0].clip.name;
        }

        /// <summary>
        /// Returns true if the <c>clipName</c> is playing with any non-zero weight.
        /// </summary>
        /// <param name="clipName">Clip to test.</param>
        /// <param name="layer">Optional - layer to check on.</param>
        /// <returns></returns>
        public bool isClipPlaying(string clipName, int layer = 0)
        {
            var clips = _animator.GetCurrentAnimatorClipInfo(layer);

            for (var i = 0; i < clips.Length; i++)
            {
                if (clips[i].clip.name == clipName)
                {
                    return true;
                }
            }

            return false;
        }
        
        /// <summary>
        /// Converts a schema name to a parameter name.
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        private string FromSchemaName(string schema)
        {
            return schema.Substring(schema.IndexOf('.') + 1);
        }

        /// <summary>
        /// Converts a parameter name to a schema name.
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        private string ToSchemaName(string param)
        {
            return "animator." + param;
        }

        /// <summary>
        /// Update the underlying animator based on schema changes.
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="prev"></param>
        /// <param name="new"></param>
        private void OnSchemaFloatChange(ElementSchemaProp<float> prop, float prev, float @new)
        {
            _animator.SetFloat(FromSchemaName(prop.Name), @new);
        }

        /// <summary>
        /// Update the underlying animator based on schema changes.
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="prev"></param>
        /// <param name="new"></param>
        private void OnSchemaIntChange(ElementSchemaProp<int> prop, int prev, int @new)
        {
            _animator.SetInteger(FromSchemaName(prop.Name), @new);
        }

        /// <summary>
        /// Update the underlying animator based on schema changes.
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="prev"></param>
        /// <param name="new"></param>
        private void OnSchemaBoolChange(ElementSchemaProp<bool> prop, bool prev, bool @new)
        {
            _animator.SetBool(FromSchemaName(prop.Name), @new);
        }

        /// <summary>
        /// Gets an ElementSchemaProp by name.
        /// </summary>
        /// <param name="props"></param>
        /// <param name="param"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private ElementSchemaProp<T> GetSchemaProp<T>(List<ElementSchemaProp<T>> props, string param)
        {
            var schemaName = ToSchemaName(param);
            
            for (var i = 0; i < props.Count; i++)
            {
                if (props[i].Name == schemaName)
                {
                    return props[i];
                }
            }

            return null;
        }
    }
}