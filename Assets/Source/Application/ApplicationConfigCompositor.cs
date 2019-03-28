using System;
using System.IO;
using System.Text;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Composites configuration from many sources and provides a consistent <c>ApplicationConfig</c> object.
    ///
    /// NOTE: Calling code must guarantee thread safety guarantees.
    /// </summary>
    public static class ApplicationConfigCompositor
    {
        /// <summary>
        /// URI to env prefs.
        /// </summary>
        private static readonly string _EnvUri = Path.Combine(
            UnityEngine.Application.persistentDataPath,
            "Config/env.prefs");

        /// <summary>
        /// Backing variable for property.
        /// </summary>
        private static ApplicationConfig _Config;

        /// <summary>
        /// The composited config.
        /// </summary>
        public static ApplicationConfig Config
        {
            get
            {
                if (
                    // always reload if in edit mode
                    UnityEngine.Application.isEditor && !UnityEngine.Application.isPlaying

                    // or if we haven't loaded it yet
                    || null == _Config)
                {
                    _Config = Load();
                }

                return _Config;
            }
        }

        /// <summary>
        /// Overwrites a piece of the config. 
        /// </summary>
        /// <param name="env">Environment data.</param>
        public static void Overwrite(EnvironmentData env)
        {
            // write to disk
            try
            {
                var dir = Path.GetDirectoryName(_EnvUri);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllText(_EnvUri, JsonConvert.SerializeObject(env));
            }
            catch (Exception ex)
            {
                Log.Error(env, "Could not write EnvironmentData to file : {0}", ex);
            }

            Apply(Config, env);
        }

        /// <summary>
        /// Reloads configs.
        /// </summary>
        public static ApplicationConfig Reload()
        {
            _Config = Load();

            return _Config;
        }

        /// <summary>
        /// Loads the config.
        /// </summary>
        /// <returns></returns>
        private static ApplicationConfig Load()
        {
            // load base
            var config = LoadConfig("ApplicationConfig");

            // load platform specific config
            var platform = LoadConfig(string.Format(
                "ApplicationConfig.{0}",
                UnityUtil.CurrentPlatform()));
            if (null != platform)
            {
                config.Override(platform);
            }

            // editor overrides
            if (UnityEngine.Application.isEditor)
            {
                var editor = LoadConfig("ApplicationConfig.Editor");
                if (null != editor)
                {
                    config.Override(editor);
                }
            }

            // load override
            var overrideConfig = LoadConfig("ApplicationConfig.Override");
            if (null != overrideConfig)
            {
                config.Override(overrideConfig);
            }

            // load custom overwrites
            try
            {
                var text = File.ReadAllText(_EnvUri);
                var env = JsonConvert.DeserializeObject<EnvironmentData>(text);

                Apply(config, env);
            }
            catch
            {
                //
            }

            return config;
        }

        /// <summary>
        /// Applies a piece of the config to the composited config.
        /// </summary>
        /// <param name="config">The config to apply to.</param>
        /// <param name="env">The piece of data to apply.</param>
        private static void Apply(ApplicationConfig config, EnvironmentData env)
        {
            // apply to config immediately
            var network = config.Network;

            var found = false;
            var environments = network.AllEnvironments;
            for (int i = 0, len = environments.Length; i < len; i++)
            {
                if (environments[i].Name == env.Name)
                {
                    found = true;
                    environments[i] = env;
                    break;
                }
            }

            if (!found)
            {
                network.AllEnvironments = network.AllEnvironments.Add(env);
            }

            network.Current = env.Name;
        }

        /// <summary>
        /// Loads a config at a path.
        /// </summary>
        /// <param name="path">The path to load the config from.</param>
        /// <returns></returns>
        private static ApplicationConfig LoadConfig(string path)
        {
            var text = string.Empty;
            if (UnityEngine.Application.isPlaying)
            {
                var configAsset = Resources.Load<TextAsset>(path);
                if (null == configAsset)
                {
                    return null;
                }

                text = configAsset.text;
            }
            else
            {
                text = File.ReadAllText(Path.Combine(
                    UnityEngine.Application.dataPath,
                    string.Format("Resources/{0}.json", path)));
            }

            var serializer = new JsonSerializer();
            var bytes = Encoding.UTF8.GetBytes(text);
            object app;
            serializer.Deserialize(typeof(ApplicationConfig), ref bytes, out app);
            
            return (ApplicationConfig) app;
        }
    }
}