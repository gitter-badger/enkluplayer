using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.Spire
{
    /// <summary>
    /// Default implementation of <c>IAppDataManager</c>.
    /// </summary>
    public class AppDataManager : IAppDataManager
    {
        /// <summary>
        /// For getting/setting files.
        /// </summary>
        private readonly IFileManager _files;

        /// <summary>
        /// Containing app.
        /// </summary>
        private AppData _app;

        /// <summary>
        /// List of all scenes.
        /// </summary>
        private readonly List<SceneData> _scenes = new List<SceneData>();

        /// <summary>
        /// List of all content.
        /// </summary>
        private readonly List<ContentData> _content = new List<ContentData>();

        /// <summary>
        /// Creates a new AppDataManager.
        /// </summary>
        /// <param name="files">For getting/setting files.</param>
        public AppDataManager(IFileManager files)
        {
            _files = files;
        }

        /// <inheritdoc cref="IAppDataManager"/>
        public T Get<T>(string id)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc cref="IAppDataManager"/>
        public T[] GetAll<T>()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc cref="IAppDataManager"/>
        public IAsyncToken<Void> Load(string name)
        {
            var token = new AsyncToken<Void>();

            // first, get app data
            _files
                .Get<AppData>(FileProtocols.APP + name + "/App")
                .OnSuccess(file =>
                {
                    _app = file.Data;
                    
                    // now get accompanying scenes
                    LoadScenes(_app)
                        .OnSuccess(_ =>
                        {
                            Log.Info(this, "Loaded scenes.");

                            token.Succeed(Void.Instance);
                        })
                        .OnFailure(token.Fail);
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not load AppData for {0} : {1}.",
                        name,
                        exception);

                    token.Fail(exception);
                });

            return token;
        }

        /// <summary>
        /// Loads all scenes in an app.
        /// </summary>
        /// <param name="app">Container app.</param>
        /// <returns></returns>
        private IAsyncToken<Void> LoadScenes(AppData app)
        {
            var token = new AsyncToken<Void>();

            var scenes = app.Scenes;
            var len = scenes.Count;

            // load and save
            var tokens = new IAsyncToken<Void>[len];
            for (var i = 0; i < len; i++)
            {
                tokens[i] = LoadScene(app, scenes[i]);
            }

            // wait for all of them
            Async
                .All(tokens)
                .OnSuccess(_ => token.Succeed(Void.Instance))
                .OnFailure(token.Fail);

            return token;
        }

        /// <summary>
        /// Loads everything within a scene.
        /// </summary>
        /// <param name="app">The container app.</param>
        /// <param name="id">Id of the scene to load.</param>
        /// <returns></returns>
        private IAsyncToken<Void> LoadScene(AppData app, string id)
        {
            var token = new AsyncToken<Void>();

            var uri = FileProtocols.APP + app.Name + "/SceneData/" + id;
            _files
                .Get<SceneData>(uri)
                .OnSuccess(file =>
                {
                    var scene = file.Data;

                    _scenes.Add(scene);

                    // load all content before succeeding token
                    LoadContent(app, scene)
                        .OnSuccess(token.Succeed)
                        .OnFailure(token.Fail);
                })
                .OnFailure(token.Fail);

            return token;
        }

        /// <summary>
        /// Loads all content for a scene.
        /// </summary>
        /// <param name="app">The container app.</param>
        /// <param name="scene">The scene to load content for.</param>
        /// <returns></returns>
        private IAsyncToken<Void> LoadContent(AppData app, SceneData scene)
        {
            var token = new AsyncToken<Void>();

            // load and save each piece
            var content = scene.Content;
            var len = content.Count;
            var tokens = new IAsyncToken<File<ContentData>>[len];
            for (var i = 0; i < len; i++)
            {
                var id = content[i];
                var uri = FileProtocols.APP + app.Name + "/ContentData/" + id;

                tokens[i] = _files
                    .Get<ContentData>(uri)
                    .OnSuccess(file =>
                    {
                        _content.Add(file.Data);
                    });
            }

            // listen for all of them
            Async
                .All(tokens)
                .OnSuccess(_ => token.Succeed(Void.Instance))
                .OnFailure(token.Fail);

            return token;
        }
    }
}