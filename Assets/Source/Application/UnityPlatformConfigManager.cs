using System;
using System.Linq;
using UnityEngine;

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

        /// <summary>
        /// Retrieves the current config.
        /// </summary>
        /// <returns></returns>
        public UnityPlatformConfig GetConfig()
        {
            for (var i = 0; i < Configs.Length; i++)
            {
                var config = Configs[i];
                if (config.IsActive())
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

            // apply all

            // grid
            GridRenderer.Config = config.Grid;
            
            // camera
            Camera.GetComponent<Camera>().backgroundColor = config.Camera.BackgroundColor;
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

        /// <summary>
        /// Config for the grid.
        /// </summary>
        public GridConfig Grid;

        /// <summary>
        /// Config for the camera.
        /// </summary>
        public CameraConfig Camera;

        /// <summary>
        /// True iff active.
        /// </summary>
        /// <returns></returns>
        public bool IsActive()
        {
            return Platforms.Contains(UnityEngine.Application.platform);
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
    }
}