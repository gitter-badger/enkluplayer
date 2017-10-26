﻿using System;
using System.Collections.Generic;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Default implementation of <c>IAppDataManager</c>.
    /// </summary>
    public class AppDataManager : IAdminAppDataManager
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

        /// <inheritdoc cref="IAppDataManager"/>
        public event Action OnLoaded;

        /// <inheritdoc cref="IAppDataManager"/>
        public event Action OnUnloaded;

        /// <inheritdoc cref="IAppDataManager"/>
        public event Action<StaticData> OnRemoved;

        /// <inheritdoc cref="IAppDataManager"/>
        public event Action<StaticData> OnUpdated;

        /// <inheritdoc cref="IAppDataManager"/>
        public string LoadedApp { get; private set; }

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
            var list = GetList<T>();
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
            return GetList<T>().Cast<T>().ToArray();
        }

        /// <inheritdoc cref="IAppDataManager"/>
        public T GetByName<T>(string name) where T : StaticData
        {
            var list = GetList<T>();
            for (int i = 0, len = list.Count; i < len; i++)
            {
                if (list[i].Name == name)
                {
                    return (T) list[i];
                }
            }

            return null;
        }

        /// <inheritdoc cref="IAppDataManager"/>
        public IAsyncToken<Void> Load(string id)
        {
            var token = new AsyncToken<Void>();

            Async
                .All(
                    LoadAssetManifest(id),
                    LoadApp(id))
                .OnSuccess(_ =>
                {
                    LoadedApp = id;

                    if (null != OnLoaded)
                    {
                        OnLoaded();
                    }

                    token.Succeed(Void.Instance);
                })
                .OnFailure(token.Fail);

            return token;
        }

        /// <inheritdoc cref="IAppDataManager"/>
        public IAsyncToken<Void> Unload()
        {
            if (null != OnUnloaded)
            {
                OnUnloaded();
            }

            return new AsyncToken<Void>(new NotImplementedException());
        }

        /// <inheritdoc cref="IAdminAppDataManager"/>
        public void Set<T>(params T[] data) where T : StaticData
        {
            var list = GetList<T>();
            list.Clear();

            list.AddRange(data);
        }

        /// <inheritdoc cref="IAdminAppDataManager"/>
        public void Add<T>(params T[] data) where T : StaticData
        {
            GetList<T>().AddRange(data);
        }

        /// <inheritdoc cref="IAdminAppDataManager"/>
        public void Remove<T>(params T[] data) where T : StaticData
        {
            var list = GetList<T>();
            for (var i = data.Length - 1; i >= 0; i--)
            {
                var datum = data[i];
                list.Remove(datum);

                if (null != OnRemoved)
                {
                    OnRemoved(datum);
                }
            }
        }

        /// <inheritdoc cref="IAdminAppDataManager"/>
        public void Update<T>(params T[] data) where T : StaticData
        {
            var list = GetList<T>();
            for (int i = 0, ilen = data.Length; i < ilen; i++)
            {
                var instance = data[i];

                var found = false;
                for (int j = 0, jlen = list.Count; j < jlen; j++)
                {
                    var existing = list[j];
                    if (instance.Id == existing.Id)
                    {
                        found = true;

                        list[j] = instance;

                        if (null != OnUpdated)
                        {
                            OnUpdated(instance);
                        }
                        break;
                    }
                }

                if (!found)
                {
                    Log.Error(this,
                        "Received update for unknown StaticData : {0}.",
                        instance);
                }
            }
        }

        /// <summary>
        /// Loads AssetDataManifest for app into <c>IAssetManager</c>.
        /// </summary>
        /// <param name="name">Name of the app.</param>
        /// <returns></returns>
        private IAsyncToken<Void> LoadAssetManifest(string name)
        {
            var token = new AsyncToken<Void>();

            _files
                .Get<AssetDataManifest>(FileProtocols.APP + name + "/Assets")
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
                        "Could not load AssetDataManifest : {0}.",
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
            var list = GetList<SceneData>();

            var uri = FileProtocols.APP + app.Name + "/SceneData/" + id;
            _files
                .Get<SceneData>(uri)
                .OnSuccess(file =>
                {
                    var scene = file.Data;

                    list.Add(scene);

                    // load all scripts + content before succeeding token
                    Async
                        .All(
                            LoadData<ScriptData>(app, scene.Scripts),
                            LoadData<ContentData>(app, scene.Content))
                        .OnSuccess(_ => token.Succeed(Void.Instance))
                        .OnFailure(token.Fail);
                })
                .OnFailure(token.Fail);

            return token;
        }
        /// <summary>
        /// Loads all scripts for a scene.
        /// </summary>
        /// <param name="app">The container app.</param>
        /// <param name="ids">The ids of data to load.</param>
        /// <returns></returns>
        private IAsyncToken<Void> LoadData<T>(AppData app, List<string> ids)
            where T : StaticData
        {
            var token = new AsyncToken<Void>();
            var list = GetList<T>();

            // load and save each piece
            var len = ids.Count;
            var tokens = new IAsyncToken<File<T>>[len];

            for (var i = 0; i < len; i++)
            {
                var id = ids[i];
                var uri = string.Format(
                    "{0}/{1}/{2}",
                    FileProtocols.APP + app.Name,
                    typeof(T).Name,
                    id);

                tokens[i] = _files
                    .Get<T>(uri)
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
        private List<StaticData> GetList<T>()
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