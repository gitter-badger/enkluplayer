using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.Spire
{
    /// <summary>
    /// Manages scene content + scripts.
    /// </summary>
    public class Scene : MonoBehaviour
    {
        /// <summary>
        /// Manages content.
        /// </summary>
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
            return String.Format("[Scene Data={0}]", Data);
        }

        /// <summary>
        /// Initializes the scene separately from loading, in case we which to
        /// pool.
        /// </summary>
        /// <param name="content">Finds content.</param>
        public void Initialize(IContentManager content)
        {
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

            Log.Info(this, "Loading scene {0}.", data);

            var token = new AsyncToken<Scene>();

            Data = data;

            LoadContent(Data.Content)
                .OnSuccess(content =>
                {
                    Log.Info(this, "Loaded all Scene content.");

                    token.Succeed(this);
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not load content : {0}.", exception);

                    token.Fail(exception);
                });

            return token;
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
        /// Loads all <c>Content</c>.
        /// </summary>
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
    }
}