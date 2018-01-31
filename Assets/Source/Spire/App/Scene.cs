using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using Jint.Unity;
using UnityEngine;

using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Manages scene content + scripts.
    /// </summary>
    public class Scene : MonoBehaviour
    {
        /// <summary>
        /// Hosts!
        /// </summary>
        private readonly List<MonoBehaviourSpireScript> _scripts = new List<MonoBehaviourSpireScript>();

        /// <summary>
        /// Dependencies.
        /// </summary>
        private IScriptRequireResolver _resolver;
        private IScriptManager _scriptManager;
        private IContentManager _contentManager;

        /// <summary>
        /// Underlying host for all scripts in this scene.
        /// </summary>
        private UnityScriptingHost _host;

        /// <summary>
        /// Parent to all scripts.
        /// </summary>
        private GameObject _scriptParent;

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
        /// <param name="resolver">Resolves script require()s.</param>
        /// <param name="scripts">Manages scripts.</param>
        /// <param name="content">Finds content.</param>
        public void Initialize(
            IScriptRequireResolver resolver,
            IScriptManager scripts,
            IContentManager content)
        {
            _resolver = resolver;
            _scriptManager = scripts;
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

            _host = new UnityScriptingHost(this, _resolver, _scriptManager);

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

            _host = null;

            Data = null;

            return new AsyncToken<Scene>(this);
        }

        /// <summary>
        /// Starts up a scene, post load.
        /// </summary>
        public void Startup()
        {
            // start scripts
            for (int i = 0, len = _scripts.Count; i < len; i++)
            {
                var host = _scripts[i];
                if (host.Script.Data.AutoPlay)
                {
                    _scripts[i].Enter();
                }
            }
        }

        /// <summary>
        /// Tears down a scene.
        /// </summary>
        public void Teardown()
        {
            // stop scripts
            for (int i = 0, len = _scripts.Count; i < len; i++)
            {
                _scripts[i].Exit();
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
                var script = _scriptManager.Create(scriptId, Data.Id);
                loads[i] = Async.ToImmutable(script.OnReady);

                Log.Debug(this, "Loading {0}.", script.Data);
            }

            return Async.All(loads);
        }

        /// <summary>
        /// Loads all <c>Content</c>.
        /// </summary>
        /// <param name="contentIds">Ids of <c>Content</c> to load.</param>
        /// <returns></returns>
        private IAsyncToken<ContentWiget[]> LoadContent(List<string> contentIds)
        {
            var len = contentIds.Count;
            var loads = new IAsyncToken<ContentWiget>[len];

            Log.Info(this, "Loading {0} piece of Content.", len);

            for (var i = 0; i < len; ++i)
            {
                var contentId = contentIds[i];
                var content = _contentManager.Request(contentId, Data.Id);
                loads[i] = Async.ToImmutable(content.OnLoaded);

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
            _scriptManager.ReleaseAll(Data.Id);

            // destroy scripts
            for (int i = 0, len = _scripts.Count; i < len; i++)
            {
                var host = _scripts[i];

                Destroy(host.gameObject);
            }
            _scripts.Clear();

            // destroy parent
            Destroy(_scriptParent);
        }

        /// <summary>
        /// Called when all scripts have been loaded, before we resolve the
        /// external token.
        /// </summary>
        /// <param name="scripts">All preloaded scripts.</param>
        private void ProcessScripts(SpireScript[] scripts)
        {
            _scriptParent = new GameObject("Scripts");
            _scriptParent.transform.SetParent(transform);

            for (int i = 0, len = scripts.Length; i < len; i++)
            {
                var script = scripts[i];
                if (!script.Data.AutoPlay)
                {
                    continue;
                }

                var obj = new GameObject(script.Data.Id);
                obj.transform.SetParent(_scriptParent.transform);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;

                var host = obj.AddComponent<MonoBehaviourSpireScript>();
                host.Initialize(
                    _host,
                    script);

                _scripts.Add(host);
            }
        }
    }
}