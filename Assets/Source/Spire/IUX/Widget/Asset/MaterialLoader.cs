using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.Assets;
using LightJson;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Loads resources for a material.
    /// </summary>
    public class MaterialLoader
    {
        /// <summary>
        /// Application data.
        /// </summary>
        private readonly IAppDataManager _appData;

        /// <summary>
        /// Manages assets.
        /// </summary>
        private readonly IAssetManager _assets;
        
        /// <summary>
        /// Action to unwatch shaderAsset.
        /// </summary>
        private Action _unwatchShaderAsset;

        /// <summary>
        /// Actions for unwatching texture assets.
        /// </summary>
        private readonly List<Action> _unwatchTextureAssets = new List<Action>();

        /// <summary>
        /// Data to apply to <c>Material</c>.
        /// </summary>
        private MaterialData _materialData;

        /// <summary>
        /// The <c>Material</c> to apply.
        /// </summary>
        public Material Material { get; private set; }

        /// <summary>
        /// Called when the <c>Material</c> has been loaded.
        /// </summary>
        public event Action OnLoaded;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MaterialLoader(
            IAppDataManager appData,
            IAssetManager assets)
        {
            _appData = appData;
            _assets = assets;

            Material = new Material(Shader.Find("Unlit/Color"));
            Material.SetColor("_Color", Color.magenta);
        }

        /// <summary>
        /// Updates material.
        /// </summary>
        /// <param name="material">The material</param>
        public void Update(MaterialData material)
        {
            Teardown();
            Setup(material);
        }
        
        /// <summary>
        /// Sets up asset watching.
        /// </summary>
        /// <param name="material">Material data.</param>
        private void Setup(MaterialData material)
        {
            _materialData = material;
            
            SetupShader(material);
            SetupTextures(material);
        }

        /// <summary>
        /// Stops watching assets.
        /// </summary>
        private void Teardown()
        {
            _materialData = null;

            if (null != _unwatchShaderAsset)
            {
                _unwatchShaderAsset();
                _unwatchShaderAsset = null;
            }

            for (int i = 0, len = _unwatchTextureAssets.Count; i < len; i++)
            {
                _unwatchTextureAssets[i]();
            }

            _unwatchTextureAssets.Clear();
        }

        /// <summary>
        /// Sets up the shader.
        /// </summary>
        /// <param name="material">Data to use to setup material.</param>
        private void SetupShader(MaterialData material)
        {
            var shaderId = null == material
                ? string.Empty
                : material.ShaderId;

            var shaderData = _appData.Get<ShaderData>(shaderId);
            if (null != shaderData)
            {
                Log.Info(this, "Load shader {0}.", shaderData);

                var asset = _assets.Manifest.Asset(shaderData.Id);
                if (null == asset)
                {
                    // might be baked in
                    var shader = Shader.Find(shaderData.Name);
                    if (null != shader)
                    {
                        ShaderAsset_OnUpdate(shader);
                    }
                }
                else
                {
                    // watch for future updates
                    _unwatchShaderAsset = asset.Watch<Shader>(ShaderAsset_OnUpdate);

                    // might already be loaded
                    var shader = asset.As<Shader>();
                    if (null != shader)
                    {
                        ShaderAsset_OnUpdate(shader);
                    }

                    // reload updates automatically
                    asset.AutoReload = true;
                }
            }
            else
            {
                Log.Warning(this, "No shader found for id {0}.", material.ShaderId);
            }
        }

        /// <summary>
        /// Sets up textures.
        /// </summary>
        /// <param name="material">Data to use to setup textures.</param>
        private void SetupTextures(MaterialData material)
        {
            if (null == material)
            {
                return;
            }

            foreach (var pair in material.Textures)
            {
                var uniform = pair.Key;
                var assetId = pair.Value;
                var textureAsset = _assets.Manifest.Asset(assetId);
                if (null != textureAsset)
                {
                    Log.Info(this, "Load texture {0}.", assetId);

                    // watch for future updates
                    var handler = TextureAsset_OnUpdate(uniform);
                    var unwatch = textureAsset.Watch(handler);
                    _unwatchTextureAssets.Add(unwatch);

                    // might already be loaded
                    var texture = textureAsset.As<Texture2D>();
                    if (null != texture)
                    {
                        handler(texture);
                    }

                    // reload updates automatically
                    textureAsset.AutoReload = true;
                }
            }
        }

        /// <summary>
        /// Called when a texture has been updated.
        /// </summary>
        /// <param name="uniform">The uniform to watch.</param>
        /// <returns></returns>
        private Action<Texture2D> TextureAsset_OnUpdate(string uniform)
        {
            return texture =>
            {
                Log.Info(this, "Texture updated for [{0}].", uniform);

                Material.SetTexture(uniform, texture);
            };
        }

        /// <summary>
        /// Called when the shader has been updated.
        /// </summary>
        /// <param name="shader">The shader to apply to the <c>Material</c>.</param>
        private void ShaderAsset_OnUpdate(Shader shader)
        {
            Material.shader = shader;

            // TODO: set properties
            foreach (var prop in _materialData.Properties)
            {
                var name = prop.Key;
                var value = (JsonValue) prop.Value;

                TrySetColor(value, name);
            }

            if (null != OnLoaded)
            {
                OnLoaded();
            }
        }

        /// <summary>
        /// Tried to set a color.
        /// </summary>
        /// <param name="value"><c>JObject</c> value.</param>
        /// <param name="name">Name of the uniform.</param>
        private bool TrySetColor(JsonValue value, string name)
        {
            return false;
            /*JToken r, g, b, a;
            if (value.TryGetValue("r", out r))
            {
                if (value.TryGetValue("g", out g))
                {
                    if (value.TryGetValue("b", out b))
                    {
                        if (value.TryGetValue("a", out a))
                        {
                            Material.SetColor(
                                name,
                                new Color(
                                    r.Value<int>() / 255f,
                                    g.Value<int>() / 255f,
                                    b.Value<int>() / 255f,
                                    a.Value<int>() / 255f));
                            return true;
                        }
                    }
                }
            }

            return false;*/
        }
    }
}