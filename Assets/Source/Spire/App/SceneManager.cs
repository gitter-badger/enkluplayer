using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.Spire
{
    /// <summary>
    /// Creates, destroys, and queries scenes.
    /// </summary>
    public class SceneManager : InjectableMonoBehaviour, ISceneManager
    {
        /// <summary>
        /// Scenes currently in hierarchy.
        /// </summary>
        public readonly List<Scene> ActiveScenes = new List<Scene>();

        /// <summary>
        /// Dependencies.
        /// </summary>
        [Inject]
        public IAppDataManager AppData { get; set; }
        [Inject]
        public IContentManager Content { get; set; }
        [Inject]
        public IScriptManager Scripts { get; set; }

        /// <summary>
        /// Useful ToString.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(
                "[SceneManager ActiveScenes={0}]",
                ActiveScenes.Count);
        }

        /// <summary>
        /// Searches for an active scene.
        /// </summary>
        /// <param name="id">Unique id of the scene.</param>
        /// <returns></returns>
        public Scene Find(string id)
        {
            for (int i = 0, count = ActiveScenes.Count; i < count; ++i)
            {
                var activeScene = ActiveScenes[i];
                if (activeScene != null
                    && activeScene.Data.Id == id)
                {
                    return activeScene;
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a new scene.
        /// </summary>
        /// <param name="id">Unique id of the scene.</param>
        /// <returns></returns>
        public IAsyncToken<Scene> Load(string id)
        {
            Log.Debug(this, "Request Scene with id {0}.", id);

            var existing = Find(id);
            if (null != existing)
            {
                var error = string.Format(
                    "Scene already loaded : {0}.",
                    existing.Data);
                Log.Warning(this, error);

                return new AsyncToken<Scene>(new Exception(error));
            }
            
            // find SceneData
            var data = AppData.Get<SceneData>(id);
            if (null == data)
            {
                var error = string.Format("Could not find scene with id {0}.", id);
                Log.Warning(this, error);

                return new AsyncToken<Scene>(new Exception(error));
            }

            // create GameObject parented to this
            var newSceneGameObject = new GameObject(id);
            newSceneGameObject.transform.SetParent(transform);
            newSceneGameObject.transform.localPosition = Vector3.zero;

            // initialize scene
            var newScene = newSceneGameObject.AddComponent<Scene>();
            newScene.Initialize(Scripts, Content);
            ActiveScenes.Add(newScene);

            return newScene.Load(data);
        }

        /// <summary>
        /// Destroys a scene.
        /// </summary>
        /// <param name="id">Unique id of the scene.</param>
        public IAsyncToken<Void> Unload(string id)
        {
            var scene = Find(id);
            if (null == scene)
            {
                return new AsyncToken<Void>(new Exception("Scene not found."));
            }

            // unload + destroy
            var token = new AsyncToken<Void>();

            scene
                .Unload()
                .OnFinally(_ =>
                {
                    ActiveScenes.Remove(scene);

                    Destroy(scene.gameObject);

                    token.Succeed(Void.Instance);
                });

            return token;
        }
    }
}