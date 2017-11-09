using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.Assets;
using UnityEngine;

namespace CreateAR.SpirePlayer
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
        /// Updates the material properties.
        /// </summary>
        /// <param name="material">Data to update material with.</param>
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
            // shader
            var shaderData = _appData.Get<ShaderData>(material.ShaderId);
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
                    _unwatchShaderAsset = asset.Watch<Shader>(ShaderAsset_OnUpdate);

                    // triggers reload automatically
                    asset.AutoReload = true;
                }
            }
            else
            {
                Log.Warning(this, "No shader found for id {0}.", material.ShaderId);
            }

            // textures
            foreach (var pair in material.Textures)
            {
                var uniform = pair.Key;
                var assetId = pair.Value;
                var textureAsset = _assets.Manifest.Asset(assetId);
                if (null != textureAsset)
                {
                    Log.Info(this, "Load texture {0}.", assetId);

                    var unwatch = textureAsset.Watch(TextureAsset_OnUpdate(uniform));
                    _unwatchTextureAssets.Add(unwatch);
                    textureAsset.AutoReload = true;
                }
            }
        }

        /// <summary>
        /// Stops watching assets.
        /// </summary>
        private void Teardown()
        {
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
            Log.Info(this, "Shader updated.");

            Material.shader = shader;

            // TODO: set properties

            if (null != OnLoaded)
            {
                OnLoaded();
            }
        }
    }
}