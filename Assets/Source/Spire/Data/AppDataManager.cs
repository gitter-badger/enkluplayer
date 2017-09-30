using System;
using System.Collections.Generic;
using System.Linq;
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
        /// Manages assets.
        /// </summary>
        private readonly IAssetManager _assets;

        /// <summary>
        /// Containing app.
        /// </summary>
        private AppData _app;

        /// <summary>
        /// Type to list of data.
        /// </summary>
        private readonly Dictionary<Type, List<StaticData>> _dataByType = new Dictionary<Type, List<StaticData>>();

        /// <summary>
        /// Creates a new AppDataManager.
        /// </summary>
        /// <param name="files">For getting/setting files.</param>
        /// <param name="assets">For getting assets.</param>
        public AppDataManager(
            IFileManager files,
            IAssetManager assets)
        {
            _files = files;
            _assets = assets;
        }

        /// <inheritdoc cref="IAppDataManager"/>
        public T Get<T>(string id) where T : StaticData
        {
            var list = List<T>();
            foreach (var element in list)
            {
                if (element.Id == id)
                {
                    return (T) element;
                }
            }

            return null;
        }

        /// <inheritdoc cref="IAppDataManager"/>
        public T[] GetAll<T>() where T : StaticData
        {
            return List<T>().Cast<T>().ToArray();
        }

        /// <inheritdoc cref="IAppDataManager"/>
        public IAsyncToken<Void> Load(string name)
        {
            var token = new AsyncToken<Void>();

            Async
                .All(
                    LoadAssets(name),
                    LoadApp(name))
                .OnSuccess(_ => token.Succeed(Void.Instance))
                .OnFailure(token.Fail);

            return token;
        }

        /// <inheritdoc cref="IAppDataManager"/>
        public IAsyncToken<Void> Unload()
        {
            return new AsyncToken<Void>(new NotImplementedException());
        }

        /// <summary>
        /// Loads AssetInfoManifest for app into <c>IAssetManager</c>.
        /// </summary>
        /// <param name="name">Name of the app.</param>
        /// <returns></returns>
        private IAsyncToken<Void> LoadAssets(string name)
        {
            var token = new AsyncToken<Void>();

            _files
                .Get<AssetInfoManifest>(FileProtocols.APP + name + "/Assets")
                .OnSuccess(file =>
                {
                    var assets = file.Data.Assets;

                    Log.Info(this,
                        "Loading {0} assets into manifest.",
                        assets.Length);

                    _assets.Manifest.Add(assets);

                    token.Succeed(Void.Instance);
                })
                .OnFailure(exception =>
                {
                    Log.Error(this,
                        "Could not load AssetInfoManifest : {0}.",
                        exception);

                    token.Fail(exception);
                });

            return token;
        }

        /// <summary>
        /// Loads all app data.
        /// </summary>
        /// <param name="name">The name of the app.</param>
        /// <returns></returns>
        private IAsyncToken<Void> LoadApp(string name)
        {
            var token = new AsyncToken<Void>();
            
            // get app data
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
            var list = List<SceneData>();

            var uri = FileProtocols.APP + app.Name + "/SceneData/" + id;
            _files
                .Get<SceneData>(uri)
                .OnSuccess(file =>
                {
                    var scene = file.Data;

                    list.Add(scene);

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
            var list = List<ContentData>();

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
                    .OnSuccess(file => list.Add(file.Data));
            }

            // listen for all of them
            Async
                .All(tokens)
                .OnSuccess(_ => token.Succeed(Void.Instance))
                .OnFailure(token.Fail);

            return token;
        }

        /// <summary>
        /// Retrieves list for type.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <returns></returns>
        private List<StaticData> List<T>()
        {
            List<StaticData> list;
            if (!_dataByType.TryGetValue(typeof(T), out list))
            {
                list = _dataByType[typeof(T)] = new List<StaticData>();
            }

            return list;
        }
    }
}