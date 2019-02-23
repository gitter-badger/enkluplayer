using System;
using System.Collections.Generic;
using CreateAR.EnkluPlayer.IUX;
using Enklu.Data;
using Jint.Runtime;

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
        /// Tracks whether Setup has been called.
        /// </summary>
        private bool _setup;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="renderer"></param>
        public MaterialJsApi(ElementSchema schema, IRenderer renderer)
        {
            _schema = schema;
            _renderer = renderer;

            _propsFloat = new List<ElementSchemaProp<float>>();
            _propsInt = new List<ElementSchemaProp<int>>();
            _propsVec3 = new List<ElementSchemaProp<Vec3>>();
            _propsCol4 = new List<ElementSchemaProp<Col4>>();
        }

        /// <summary>
        /// Subscribes to schema.
        /// </summary>
        [DenyJsAccess]
        public void Setup()
        {
            if (_setup)
            {
                throw new Exception("MaterialJsApi already setup.");
            }
            
            // TODO: Unity 2018.2 upgrade - Look at Material.GetTexturePropertyNames for setting defaults?!
            
            _setup = true;
        }

        /// <summary>
        /// Unsubscribes from schema.
        /// </summary>
        [DenyJsAccess]
        public void Teardown()
        {
            if (!_setup)
            {
                throw new Exception("MaterialJsApi not setup.");
            }
            
            for (int i = 0, len = _propsFloat.Count; i < len; i++)
            {
                _propsFloat[i].OnChanged -= Prop_OnFloatChanged;
            }
            
            for (int i = 0, len = _propsInt.Count; i < len; i++)
            {
                _propsInt[i].OnChanged -= Prop_OnIntChanged;
            }
            
            for (int i = 0, len = _propsVec3.Count; i < len; i++)
            {
                _propsVec3[i].OnChanged -= Prop_OnVectorChanged;
            }
            
            for (int i = 0, len = _propsCol4.Count; i < len; i++)
            {
                _propsCol4[i].OnChanged -= Prop_OnColorChanged;
            }

            _setup = false;
        }

        /// <summary>
        /// Breaks the underlying shared material link, causing this material to be unique.
        /// </summary>
        public void makeUnique()
        {
            var mat = _renderer.Material;
        }

        /// <summary>
        /// Gets a float material property.
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public float getFloat(string param)
        {
            return _renderer.SharedMaterial.GetFloat(param);
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
                
                prop.OnChanged += Prop_OnFloatChanged;
                Prop_OnFloatChanged(prop, value, value);
            }
            else
            {
                prop.Value = value;
            }
        }

        /// <summary>
        /// Gets an integer material property.
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public int getInt(string param)
        {
            return _renderer.SharedMaterial.GetInt(param);
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
                
                prop.OnChanged += Prop_OnIntChanged;
                Prop_OnIntChanged(prop, value, value);
            }
            else
            {
                prop.Value = value;
            }
        }

        /// <summary>
        /// Gets a Vec3 for a Vector material property.
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public Vec3 getVector(string param)
        {
            return _renderer.SharedMaterial.GetVec3(param);
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
                
                prop.OnChanged += Prop_OnVectorChanged;
                Prop_OnVectorChanged(prop, value, value);
            }
            else
            {
                prop.Value = value;
            }
        }

        /// <summary>
        /// Gets a Col4 for a Vector material property.
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public Col4 getColor(string param)
        {
            return _renderer.SharedMaterial.GetCol4(param);
        }

        /// <summary>
        /// Sets a Vector material property with a Col4.
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
                
                prop.OnChanged += Prop_OnColorChanged;
                Prop_OnColorChanged(prop, value, value);
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
        private static string FromSchemaName(string schema)
        {
            return schema.Substring(schema.IndexOf('.') + 1);
        }

        /// <summary>
        /// Converts a parameter name to a schema name.
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        private static string ToSchemaName(string param)
        {
            return "material." + param;
        }

        /// <summary>
        /// Update the underlying material based on schema changes.
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="prev"></param>
        /// <param name="new"></param>
        private void Prop_OnFloatChanged(ElementSchemaProp<float> prop, float prev, float @new)
        {
            _renderer.SharedMaterial.SetFloat(FromSchemaName(prop.Name), @new);
        }
        
        /// <summary>
        /// Update the underlying material based on schema changes.
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="prev"></param>
        /// <param name="new"></param>
        private void Prop_OnIntChanged(ElementSchemaProp<int> prop, int prev, int @new)
        {
            _renderer.SharedMaterial.SetInt(FromSchemaName(prop.Name), @new);
        }
        
        /// <summary>
        /// Update the underlying material based on schema changes.
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="prev"></param>
        /// <param name="new"></param>
        private void Prop_OnVectorChanged(ElementSchemaProp<Vec3> prop, Vec3 prev, Vec3 @new)
        {
            _renderer.SharedMaterial.SetVec3(FromSchemaName(prop.Name), @new);
        }
        
        /// <summary>
        /// Update the underlying material based on schema changes.
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="prev"></param>
        /// <param name="new"></param>
        private void Prop_OnColorChanged(ElementSchemaProp<Col4> prop, Col4 prev, Col4 @new)
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