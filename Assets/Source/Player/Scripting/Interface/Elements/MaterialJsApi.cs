using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// An interface to Unity's Material system.
    /// Properties of an Element's shared material can be modified.
    /// </summary>
    public class MaterialJsApi
    {
        public static TextureJsInterface TextureJsInterface;
        
        /// <summary>
        /// Underlying Unity Renderer to modify.
        /// </summary>
        private readonly Renderer _renderer;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="renderer"></param>
        public MaterialJsApi(Renderer renderer)
        {
            _renderer = renderer;
        }
        
        /// <summary>
        /// Returns the number of Materials on the Renderer.
        /// </summary>
        public int materialCount
        {
            get { return _renderer.sharedMaterials.Length; }
        }

        /// <summary>
        /// Sets a float material property.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="value"></param>
        public void setFloat(string param, float value)
        {
            _renderer.sharedMaterial.SetFloat(param, value);
        }

        /// <summary>
        /// Sets a float material property.
        /// </summary>
        /// <param name="material"></param>
        /// <param name="param"></param>
        /// <param name="value"></param>
        public void setFloat(int material, string param, float value)
        {
            _renderer.sharedMaterials[material].SetFloat(param, value);
        }

        /// <summary>
        /// Sets an integer material property.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="value"></param>
        public void setInt(string param, int value)
        {
            _renderer.sharedMaterial.SetInt(param, value);
        }

        /// <summary>
        /// Sets an integer material property.
        /// </summary>
        /// <param name="material"></param>
        /// <param name="param"></param>
        /// <param name="value"></param>
        public void setInt(int material, string param, int value)
        {
            _renderer.sharedMaterials[material].SetInt(param, value);
        }

        /// <summary>
        /// Sets a Vector material property with a Vec3.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="value"></param>
        public void setVector(string param, Vec3 value)
        {
            _renderer.sharedMaterial.SetVector(param, value.ToVector());
        }

        /// <summary>
        /// Sets a Vector material property with a Vec3.
        /// </summary>
        /// <param name="material"></param>
        /// <param name="param"></param>
        /// <param name="value"></param>
        public void setVector(int material, string param, Vec3 value)
        {
            _renderer.sharedMaterials[material].SetVector(param, value.ToVector());
        }

        /// <summary>
        /// Sets a Vector material property with a Vec3 & alpha.
        /// TODO: Add in a Vec4 and use that instead.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="value"></param>
        public void setVector(string param, Col4 value)
        {
            _renderer.sharedMaterial.SetVector(param, value.ToColor());
        }

        /// <summary>
        /// Sets a Vector material property with a Vec3 & alpha.
        /// </summary>
        /// <param name="material"></param>
        /// <param name="param"></param>
        /// <param name="value"></param>
        public void setVector(int material, string param, Col4 value)
        {
            _renderer.sharedMaterials[material].SetVector(param, value.ToColor());
        }

        /// <summary>
        /// Sets a lightmode pass enabled/disabled.
        /// </summary>
        /// <param name="pass"></param>
        /// <param name="enabled"></param>
        public void setLightModePass(string pass, bool enabled)
        {
            // Unity's docs are extremely horrible for SetShaderPassEnabled.
            // It doesn't actually set passes enabled/disabled by pass name like the code comments indicate.
            // Instead it works based on LightMode tags, which the example in the online docs doesn't match.
            _renderer.sharedMaterial.SetShaderPassEnabled(pass, enabled);
        }

        /// <summary>
        /// Sets a Texture material property by a <c>TextureJsInterface</c> id.
        /// </summary>
        /// <param name="texName"></param>
        /// <param name="texId"></param>
        public void setTexture(string texName, int texId)
        {
            Texture2D tex = TextureJsInterface.GetTexture(texId);
            if (!tex)
            {
                Log.Error(this, "Unknown texture ID");
                return;
            }
            
            _renderer.sharedMaterial.SetTexture(texName, tex);
        }

        /// <summary>
        /// Sets a Texture material property by a <c>TextureJsInterface</c> id.
        /// </summary>
        /// <param name="material"></param>
        /// <param name="texName"></param>
        /// <param name="texId"></param>
        public void setTexture(int material, string texName, int texId)
        {
            Texture2D tex = TextureJsInterface.GetTexture(texId);
            if (!tex)
            {
                Log.Error(this, "Unknown texture ID");
                return;
            }
            
            _renderer.sharedMaterials[material].SetTexture(texName, tex);
        }
    }
}