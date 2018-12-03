using System.Collections.Generic;
using CreateAR.EnkluPlayer.IUX;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// An interface to Unity's Material system.
    /// Properties of an Element's shared material can be modified.
    /// </summary>
    public class MaterialJsApi
    {
        /// <summary>
        /// Schema to back shader properties.
        /// </summary>
        private readonly ElementSchema _schema;
        
        /// <summary>
        /// Underlying Unity Renderer to modify.
        /// </summary>
        private readonly IRenderer _renderer;

        /// <summary>
        /// Backing schema props
        /// </summary>
        private readonly List<ElementSchemaProp<float>> _propsFloat;
        private readonly List<ElementSchemaProp<int>> _propsInt;
        private readonly List<ElementSchemaProp<Vec3>> _propsVec3;
        private readonly List<ElementSchemaProp<Col4>> _propsCol4;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="renderer"></param>
        public MaterialJsApi(ElementSchema schema, IRenderer renderer)
        {
            _schema = schema;
            _renderer = renderer;

            // TODO: Unity 2018.2 upgrade - Look at Material.GetTexturePropertyNames for setting defaults?!
            _propsFloat = new List<ElementSchemaProp<float>>();
            _propsInt = new List<ElementSchemaProp<int>>();
            _propsVec3 = new List<ElementSchemaProp<Vec3>>();
            _propsCol4 = new List<ElementSchemaProp<Col4>>();
        }

        /// <summary>
        /// Deconstructor
        /// </summary>
        ~MaterialJsApi()
        {
            for (int i = 0, len = _propsFloat.Count; i < len; i++)
            {
                _propsFloat[i].OnChanged -= OnSchemaFloatChange;
            }
            
            for (int i = 0, len = _propsInt.Count; i < len; i++)
            {
                _propsInt[i].OnChanged -= OnSchemaIntChange;
            }
            
            for (int i = 0, len = _propsVec3.Count; i < len; i++)
            {
                _propsVec3[i].OnChanged -= OnSchemaVectorChange;
            }
            
            for (int i = 0, len = _propsCol4.Count; i < len; i++)
            {
                _propsCol4[i].OnChanged -= OnSchemaColorChange;
            }
        }

        /// <summary>
        /// Sets a float material property.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="value"></param>
        public void setFloat(string param, float value)
        {
            var prop = GetSchemaProp(_propsFloat, param);
            if (prop == null)
            {
                prop = _schema.GetOwn(ToSchemaName(param), value);
                _propsFloat.Add(prop);
                
                prop.OnChanged += OnSchemaFloatChange;
                OnSchemaFloatChange(prop, value, value);
            }
            else
            {
                prop.Value = value;
            }
        }

        /// <summary>
        /// Sets an integer material property.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="value"></param>
        public void setInt(string param, int value)
        {
            var prop = GetSchemaProp(_propsInt, param);
            if (prop == null)
            {
                prop = _schema.GetOwn(ToSchemaName(param), value);
                _propsInt.Add(prop);
                
                prop.OnChanged += OnSchemaIntChange;
                OnSchemaIntChange(prop, value, value);
            }
            else
            {
                prop.Value = value;
            }
        }

        /// <summary>
        /// Sets a Vector material property with a Vec3.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="value"></param>
        public void setVector(string param, Vec3 value)
        {
            var prop = GetSchemaProp(_propsVec3, param);
            if (prop == null)
            {
                prop = _schema.GetOwn(ToSchemaName(param), value);
                _propsVec3.Add(prop);
                
                prop.OnChanged += OnSchemaVectorChange;
                OnSchemaVectorChange(prop, value, value);
            }
            else
            {
                prop.Value = value;
            }
        }

        /// <summary>
        /// Sets a Vector material property with a Vec3 & alpha.
        /// TODO: Add in a Vec4 and use that instead.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="value"></param>
        public void setColor(string param, Col4 value)
        {
            var prop = GetSchemaProp(_propsCol4, param);
            if (prop == null)
            {
                prop = _schema.GetOwn(ToSchemaName(param), value);
                _propsCol4.Add(prop);
                
                prop.OnChanged += OnSchemaColorChange;
                OnSchemaColorChange(prop, value, value);
            }
            else
            {
                prop.Value = value;
            }
        }
        
        /// <summary>
        /// Converts a schema name to a parameter name.
        /// </summary>
        /// <param name="schema"></param>
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
            return "material." + param;
        }

        /// <summary>
        /// Update the underlying material based on schema changes.
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="prev"></param>
        /// <param name="new"></param>
        private void OnSchemaFloatChange(ElementSchemaProp<float> prop, float prev, float @new)
        {
            _renderer.SharedMaterial.SetFloat(FromSchemaName(prop.Name), @new);
        }
        
        /// <summary>
        /// Update the underlying material based on schema changes.
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="prev"></param>
        /// <param name="new"></param>
        private void OnSchemaIntChange(ElementSchemaProp<int> prop, int prev, int @new)
        {
            _renderer.SharedMaterial.SetInt(FromSchemaName(prop.Name), @new);
        }
        
        /// <summary>
        /// Update the underlying material based on schema changes.
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="prev"></param>
        /// <param name="new"></param>
        private void OnSchemaVectorChange(ElementSchemaProp<Vec3> prop, Vec3 prev, Vec3 @new)
        {
            _renderer.SharedMaterial.SetVec3(FromSchemaName(prop.Name), @new);
        }
        
        /// <summary>
        /// Update the underlying material based on schema changes.
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="prev"></param>
        /// <param name="new"></param>
        private void OnSchemaColorChange(ElementSchemaProp<Col4> prop, Col4 prev, Col4 @new)
        {
            _renderer.SharedMaterial.SetCol4(FromSchemaName(prop.Name), @new);
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
            
            for (int i = 0, len = props.Count; i < len; i++)
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