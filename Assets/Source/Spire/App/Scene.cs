using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.Spire
{
    /// <summary>
    /// Manages scene content + scripts.
    /// </summary>
    public class Scene : MonoBehaviour
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private IScriptManager _scripts;
        private IContentManager _contentManager;
        
        /// <summary>
        /// Data for this <c>Scene</c>.
        /// </summary>
        public SceneData Data { get; private set; }
        
        /// <summary>
        /// Useful ToString.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[Scene Data={0}]", Data);
        }

        /// <summary>
        /// Initializes the scene separately from loading, in case we which to
        /// pool.
        /// </summary>
        /// <param name="scripts">Manages scripts.</param>
        /// <param name="content">Finds content.</param>
        public void Initialize(
            IScriptManager scripts,
            IContentManager content)
        {
            _scripts = scripts;
            _contentManager = content;
        }

        /// <summary>
        /// Loads Scene assets.
        /// </summary>
        /// <param name="data">Data for this scene.</param>
        /// <returns></returns>
        public IAsyncToken<Scene> Load(SceneData data)
        {
            if (null != Data)
            {
                throw new Exception("Scene already loaded.");
            }

            Data = data;

            Log.Info(this, "Loading scene {0}.", Data);

            return Async.Map(
                Async.All(
                    Async.Map(LoadContent(Data.Content), _ => Void.Instance),
                    Async.Map(LoadScripts(Data.Scripts).OnSuccess(ProcessScripts), _ => Void.Instance)),
                _ => this);
        }

        /// <summary>
        /// Unloads scene.
        /// </summary>
        /// <returns></returns>
        public IAsyncToken<Scene> Unload()
        {
            Log.Info(this, "Unloading scene {0}.", Data);

            UnloadContent();

            Data = null;

            return new AsyncToken<Scene>(this);
        }

        /// <summary>
        /// Starts up a scene, post load.
        /// </summary>
        public void Startup()
        {
            
        }

        /// <summary>
        /// Tears down a scene.
        /// </summary>
        public void Teardown()
        {
            
        }

        /// <summary>
        /// Loads all <c>SpireScript</c>.
        /// </summary>
        /// <param name="scriptIds">Ids of <c>ScriptData</c> to load.</param>
        /// <returns></returns>
        private IAsyncToken<SpireScript[]> LoadScripts(
            List<string> scriptIds)
        {
            var len = scriptIds.Count;
            var loads = new IAsyncToken<SpireScript>[len];
            for (var i = 0; i < len; i++)
            {
                var scriptId = scriptIds[i];
                var script = _scripts.Create(scriptId, Data.Id);
                loads[i] = script.OnReady;

                Log.Debug(this, "Loading {0}.", script);
            }

            return Async.All(loads);
        }

        /// <summary>
        /// Loads all <c>Content</c>.
        /// </summary>
        /// <param name="contentIds">Ids of <c>Content</c> to load.</param>
        /// <returns></returns>
        private IAsyncToken<Content[]> LoadContent(List<string> contentIds)
        {
            var loads = new IAsyncToken<Content>[contentIds.Count];

            for (int i = 0, count = contentIds.Count; i < count; ++i)
            {
                var contentId = contentIds[i];
                var content = _contentManager.Request(contentId, Data.Id);
                loads[i] = content.OnReady;

                Log.Debug(this, "Loading {0}.", content);
            }

            return Async.All(loads);
        }

        /// <summary>
        /// Unloads content.
        /// </summary>
        private void UnloadContent()
        {
            // release tagged content
            _contentManager.ReleaseAll(Data.Id);
        }

        /// <summary>
        /// Called when all scripts have been loaded, before we resolve the
        /// external token.
        /// </summary>
        /// <param name="scripts">All preloaded scripts.</param>
        private void ProcessScripts(SpireScript[] scripts)
        {

        }
    }
}