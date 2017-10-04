using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using Jint;
using Jint.Unity;
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
        /// Hosts!
        /// </summary>
        private readonly List<MonoBehaviourScriptingHost> _hosts = new List<MonoBehaviourScriptingHost>();
        
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

            UnloadScripts();
            UnloadContent();

            Data = null;

            return new AsyncToken<Scene>(this);
        }

        /// <summary>
        /// Starts up a scene, post load.
        /// </summary>
        public void Startup()
        {
            // start scripts
            for (int i = 0, len = _hosts.Count; i < len; i++)
            {
                _hosts[i].Enter();
            }
        }

        /// <summary>
        /// Tears down a scene.
        /// </summary>
        public void Teardown()
        {
            // stop scripts
            for (int i = 0, len = _hosts.Count; i < len; i++)
            {
                _hosts[i].Exit();
            }
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

            Log.Info(this, "Loading {0} scripts.", len);

            var loads = new IAsyncToken<SpireScript>[len];
            for (var i = 0; i < len; i++)
            {
                var scriptId = scriptIds[i];
                var script = _scripts.Create(scriptId, Data.Id);
                loads[i] = script.OnReady;

                Log.Debug(this, "Loading {0}.", script.Data);
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
            var len = contentIds.Count;
            var loads = new IAsyncToken<Content>[len];

            Log.Info(this, "Loading {0} piece of Content.", len);

            for (var i = 0; i < len; ++i)
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
        /// Unloads scripts.
        /// </summary>
        private void UnloadScripts()
        {
            _scripts.ReleaseAll(Data.Id);

            // destroy scripts
            for (int i = 0, len = _hosts.Count; i < len; i++)
            {
                var host = _hosts[i];

                Destroy(host.gameObject);
            }
            _hosts.Clear();
        }

        /// <summary>
        /// Called when all scripts have been loaded, before we resolve the
        /// external token.
        /// </summary>
        /// <param name="scripts">All preloaded scripts.</param>
        private void ProcessScripts(SpireScript[] scripts)
        {
            for (int i = 0, len = scripts.Length; i < len; i++)
            {
                var script = scripts[i];
                var obj = new GameObject(script.Data.Id);
                obj.transform.SetParent(transform);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;

                var host = obj.AddComponent<MonoBehaviourScriptingHost>();
                host.Initialize(
                    // TODO: Reuse engines or share them.
                    new UnityScriptingHost(null, null), 
                    script);

                _hosts.Add(host);
            }
        }
    }
}