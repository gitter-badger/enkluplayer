using System;
using System.Linq;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Provides configuration based on platform.
    /// </summary>
    public class UnityPlatformConfigManager : InjectableMonoBehaviour
    {
        /// <summary>
        /// The configs.
        /// </summary>
        public UnityPlatformConfig[] Configs;

        /// <summary>
        /// Dependencies.
        /// </summary>
        [Inject]
        public MainCamera Camera { get; set; }
        [Inject]
        public GridRenderer GridRenderer { get; set; }
        [Inject]
        public ApplicationConfig Config { get; set; }
        
        /// <summary>
        /// Retrieves the current config.
        /// </summary>
        /// <returns></returns>
        public UnityPlatformConfig GetConfig()
        {
            if (UnityEngine.Application.isEditor)
            {
                for (var i = 0; i < Configs.Length; i++)
                {
                    var config = Configs[i];
                    if (config.Debug)
                    {
                        return config;
                    }
                }
            }

            for (var i = 0; i < Configs.Length; i++)
            {
                var config = Configs[i];
                if (config.IsActive(Config.ParsedPlatform))
                {
                    return config;
                }
            }

            return null;
        }

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

            var config = GetConfig();
            if (null == config)
            {
                throw new Exception(string.Format(
                    "No UnityPlatformConfig for {0}.",
                    UnityEngine.Application.platform));
            }
            else
            {
                Log.Info(this, "Platform config found : {0}.", config);
            }

            // apply all

            // grid
            {
                GridRenderer.Config = config.Grid;
            }

            // camera
            {
                var cam = Camera.GetComponent<Camera>();
                cam.backgroundColor = config.Camera.BackgroundColor;
                cam.transform.position = config.Camera.StartingPosition;
                cam.transform.LookAt(Vector3.zero);

                var debugRenderer = FindObjectOfType<DebugRendererMonoBehaviour>();
                debugRenderer.Enabled = config.Camera.DebugRenderingEnabled;
                debugRenderer.Filter = config.Camera.DebugRenderingFilter;
            }
            
            // lighting
            {
                if (!config.Lighting.DefaultDirectionalLight)
                {
                    var directional = GameObject.Find("Directional Light");
                    if (null == directional)
                    {
                        Log.Error(this, "Could not find default directional light!");
                    }
                    else
                    {
                        directional.SetActive(false);
                    }
                }

                if (!config.Lighting.DefaultAmbientLight)
                {
                    RenderSettings.ambientIntensity = 0;
                    RenderSettings.ambientLight = Color.black;
                    RenderSettings.ambientGroundColor = Color.black;
                    RenderSettings.ambientSkyColor = Color.black;
                    RenderSettings.skybox = null;
                }
            }
        }
    }

    /// <summary>
    /// Configuration for a set of platforms.
    /// </summary>
    [Serializable]
    public class UnityPlatformConfig
    {
        /// <summary>
        /// The platforms to match.
        /// </summary>
        public RuntimePlatform[] Platforms;

        [Tooltip("If true, then the editor will use this config. Only applies in editor.")]
        public bool Debug;

        /// <summary>
        /// Config for the grid.
        /// </summary>
        public GridConfig Grid;

        /// <summary>
        /// Config for the camera.
        /// </summary>
        public CameraConfig Camera;

        /// <summary>
        /// Config for lighting.
        /// </summary>
        public LightingConfig Lighting;

        /// <summary>
        /// True iff active.
        /// </summary>
        /// <returns></returns>
        public bool IsActive(RuntimePlatform platform)
        {
            return Platforms.Contains(platform);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("[UnityPlatformConfig Platforms={0}]",
                string.Join(", ", Platforms.Select(p => p.ToString()).ToArray()));
        }
    }

    /// <summary>
    /// Config for camera.
    /// </summary>
    [Serializable]
    public class CameraConfig
    {
        [Tooltip("Clear color.")]
        public Color BackgroundColor;

        [Tooltip("Starting position of camera.")]
        public Vector3 StartingPosition;

        [Tooltip("True iff debug rendering is enabled.")]
        public bool DebugRenderingEnabled;

        [Tooltip("Filter for debug renderer.")]
        public string DebugRenderingFilter;
    }

    /// <summary>
    /// Config for lighting.
    /// </summary>
    [Serializable]
    public class LightingConfig
    {
        [Tooltip("True iff the default direction light should be used.")]
        public bool DefaultDirectionalLight = true;

        [Tooltip("True iff the default ambient lighting settings should be used.")]
        public bool DefaultAmbientLight = true;
    }
}