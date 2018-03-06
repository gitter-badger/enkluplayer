using System;
using System.Linq;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class UnityPlatformConfigManager : InjectableMonoBehaviour
    {
        public UnityPlatformConfig[] Configs;

        [Inject]
        public MainCamera Camera { get; set; }

        [Inject]
        public GridRenderer GridRenderer { get; set; }

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

    [Serializable]
    public class UnityPlatformConfig
    {
        public RuntimePlatform[] Platforms;

        public GridConfig Grid;
        public CameraConfig Camera;

        public bool IsActive()
        {
            return Platforms.Contains(UnityEngine.Application.platform);
        }
    }

    [Serializable]
    public class CameraConfig
    {
        public Color BackgroundColor;
    }
}