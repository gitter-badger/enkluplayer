using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.IUX;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.GetAssets;
using Body = CreateAR.Trellis.Messages.GetAppScripts.Body;
using Response = CreateAR.Trellis.Messages.GetScene.Response;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    public class AppSceneListCacheData
    {
        public string[] Scenes;
    }

    /// <summary>
    /// Loads app data.
    /// </summary>
    public class AppDataLoader : IAppDataLoader
    {
        /// <summary>
        /// Json.
        /// </summary>
        private readonly JsonSerializer _json = new JsonSerializer();

        /// <summary>
        /// API.
        /// </summary>
        private readonly ApiController _api;

        /// <summary>
        /// Transport implementation.
        /// </summary>
        private readonly IElementTxnTransport _transport;

        /// <summary>
        /// Messages.
        /// </summary>
        private readonly IMessageRouter _messages;
        
        /// <summary>
        /// Files.
        /// </summary>
        private readonly IFileManager _files;

        /// <summary>
        /// Lookup from sceneId -> scene loads.
        /// </summary>
        private readonly Dictionary<string, IAsyncToken<HttpResponse<Response>>> _sceneLoads = new Dictionary<string, IAsyncToken<HttpResponse<Response>>>();

        /// <summary>
        /// Data for each loaded scene.
        /// </summary>
        private readonly Dictionary<string, ElementDescription> _sceneData = new Dictionary<string, ElementDescription>();

        /// <inheritdoc />
        public string[] Scenes
        {
            get
            {
                return _sceneData.Keys.ToArray();
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public AppDataLoader(
            ApiController api,
            IElementTxnTransport transport,
            IMessageRouter messages,
            IFileManager files)
        {
            _api = api;
            _transport = transport;
            _messages = messages;
            _files = files;
        }

        /// <inheritdoc />
        public ElementDescription Scene(string sceneId)
        {
            ElementDescription description;
            if (_sceneData.TryGetValue(sceneId, out description))
            {
                return description;
            }

            return null;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Load(string appId)
        {
            return Async.Map(
                Async.All(
                    LoadPrerequisites(appId),
                    LoadScenes(appId)),
                _ => Void.Instance);
        }

        /// <inheritdoc />
        public void Unload()
        {
            // stop loads
            foreach (var pair in _sceneLoads)
            {
                pair.Value.Abort();
            }
            _sceneLoads.Clear();
            
            // unload data
            _sceneData.Clear();
        }

        /// <summary>
        /// Loads prerequisites for an app.
        /// </summary>
        /// <param name="appId">The id of the app.</param>
        /// <returns></returns>
        private IAsyncToken<Void> LoadPrerequisites(string appId)
        {
            return Async.Map(
                Async.All(
                    GetAssets(appId),
                    GetScripts(appId)),
                _ => Void.Instance);
        }

        /// <summary>
        /// Loads all of an app's scenes.
        /// </summary>
        /// <param name="appId">The id of the app.</param>
        /// <returns></returns>
        private IAsyncToken<Void> LoadScenes(string appId)
        {
            var token = new AsyncToken<Void>();

            // callback called after scene list is retrieved
            Action<string[], bool> load = (scenes, offline) =>
            {
                if (offline)
                {
                    Log.Info(this, "Loading scenes from disk.");
                }
                else
                {
                    Log.Info(this, "Loading scenes from network.");
                }

                // load each scene
                Async
                    .All(scenes
                        .Select(scene =>
                        {
                            if (offline)
                            {
                                return LoadSceneFromDisk(appId, scene)
                                    .OnSuccess(description => _sceneData[appId] = description);
                            }

                            return LoadSceneFromNetwork(appId, scene)
                                    .OnSuccess(description => _sceneData[appId] = description);
                        })
                        .ToArray())
                    .OnSuccess(_ =>
                    {
                        Log.Info(this,
                            "Successfully loaded {0} scenes.",
                            scenes.Length);

                        token.Succeed(Void.Instance);
                    })
                    .OnFailure(token.Fail);
            };

            var uri = string.Format("appdata://{0}/scenelist", appId);

            // get app
            _transport
                .GetApp(appId)
                .OnSuccess(response =>
                {
                    Log.Info(this, "Loaded scene list from network.");

                    _files
                        .Set(uri, new AppSceneListCacheData
                        {
                            Scenes = response.Body.Scenes
                        })
                        .OnFailure(exception => Log.Error(this, "Could not save scene list to disk : {0}.", exception));

                    load(response.Body.Scenes, false);
                })
                .OnFailure(exception =>
                {
                    Log.Info(this, "Could not get app from network : {0}.", exception);

                    _files
                        .Get<AppSceneListCacheData>(uri)
                        .OnSuccess(file =>
                        {
                            Log.Info(this, "Loaded scene list from disk.");

                            load(file.Data.Scenes, true);
                        })
                        .OnFailure(token.Fail);
                });

            return token;
        }
        
        /// <summary>
        /// Retrieves AssetData.
        /// </summary>
        /// <returns></returns>
        private IAsyncToken<Void> GetAssets(string appId)
        {
            var token = new AsyncToken<Void>();

            // Start loads from both disk and network at the same time.
            // If both fail, the token fails. Whichever one succeeds first 
            // succeeds the token. Network is treated as the authority, so it
            // will cancel disk and reapply assets if necessary.

            // fail token if both disk and network retrieval fails
            Exception disk = null, network = null;
            Action checkForFailure = () =>
            {
                if (null != disk && null != network)
                {
                    Log.Info(this, "Both disk and network asset loads have failed.");
                    token.Fail(network);
                }
            };

            // load from disk
            var diskLoad = GetAssetsFromDisk(appId)
                .OnSuccess(@event =>
                {
                    _messages.Publish(
                        MessageTypes.RECV_ASSET_LIST,
                        @event);

                    token.Succeed(Void.Instance);
                })
                .OnFailure(exception =>
                {
                    Log.Info(this, "Could not load assets from disk : {0}.", exception);

                    disk = exception;

                    checkForFailure();
                });

            // load from network
            GetAssetsFromNetwork(appId)
                .OnSuccess(assets =>
                {
                    var @event = new AssetListEvent
                    {
                        Assets = assets
                    };

                    Log.Info(this, "Assets recevied from network. Writing to disk.");

                    // write to disk for next time
                    _files
                        .Set(string.Format("appdata://{0}/assets", appId), @event)
                        .OnFailure(exception => Log.Error(this, "Could not write assets data to disk : {0}.", exception));

                    // cancel disk load
                    diskLoad.Abort();

                    _messages.Publish(
                        MessageTypes.RECV_ASSET_LIST,
                        @event);

                    token.Succeed(Void.Instance);
                })
                .OnFailure(exception =>
                {
                    Log.Info(this, "Could not load assets from network : {0}.", exception);

                    network = exception;
                    checkForFailure();
                });

            return token;
        }

        /// <summary>
        /// Retrieves assets from disk.
        /// </summary>
        /// <param name="appId">Id of the app.</param>
        /// <returns></returns>
        private IAsyncToken<AssetListEvent> GetAssetsFromDisk(string appId)
        {
            return Async.Map(
                _files.Get<AssetListEvent>(string.Format("appdata://{0}/assets", appId)),
                file => file.Data);
        }

        /// <summary>
        /// Retrieve assets from the network.
        /// </summary>
        /// <param name="appId">The id of the app.</param>
        /// <returns></returns>
        private IAsyncToken<AssetData[]> GetAssetsFromNetwork(string appId)
        {
            var token = new AsyncToken<AssetData[]>();

            _api
                .Assets
                .GetAssets(appId)
                .OnSuccess(response =>
                {
                    if (response.Payload.Success)
                    {
                        token.Succeed(response.Payload.Body.Assets.Select(ToAssetData).ToArray());
                    }
                    else
                    {
                        token.Fail(new Exception(response.Payload.Error));
                    }
                })
                .OnFailure(exception =>
                {
                    token.Fail(new Exception(string.Format("Could not get assets from network: {0}.", exception)));
                });

            return token;
        }

        /// <summary>
        /// Retrieves ScriptData.
        /// </summary>
        /// <returns></returns>
        private IAsyncToken<Void> GetScripts(string appId)
        {
            var token = new AsyncToken<Void>();

            // Start loads from both disk and network at the same time.
            // If both fail, the token fails. Whichever one succeeds first 
            // succeeds the token. Network is treated as the authority, so it
            // will cancel disk and reapply assets if necessary.

            // fail token if both disk and network retrieval fails
            Exception disk = null, network = null;
            Action checkForFailure = () =>
            {
                if (null != disk && null != network)
                {
                    Log.Info(this, "Both disk and network script loads have failed.");
                    token.Fail(network);
                }
            };

            // load from disk
            var diskLoad = GetScriptsFromDisk(appId)
                .OnSuccess(@event =>
                {
                    _messages.Publish(
                        MessageTypes.RECV_SCRIPT_LIST,
                        @event);

                    token.Succeed(Void.Instance);
                })
                .OnFailure(exception =>
                {
                    Log.Info(this, "Could not load scripts from disk : {0}.", exception);

                    disk = exception;

                    checkForFailure();
                });

            // load from network
            GetScriptsFromNetwork(appId)
                .OnSuccess(scripts =>
                {
                    var @event = new ScriptListEvent
                    {
                        Scripts = scripts
                    };

                    // write to disk for next time
                    _files
                        .Set(string.Format("appdata://{0}/scripts", appId), @event)
                        .OnFailure(exception => Log.Error(this, "Could not write scripts data to disk : {0}.", exception));

                    // cancel disk load
                    diskLoad.Abort();

                    _messages.Publish(
                        MessageTypes.RECV_SCRIPT_LIST,
                        @event);

                    token.Succeed(Void.Instance);
                })
                .OnFailure(exception =>
                {
                    Log.Info(this, "Could not load scripts from network : {0}.", exception);

                    network = exception;
                    checkForFailure();
                });

            return token;
        }

        /// <summary>
        /// Retrieves scripts from disk.
        /// </summary>
        /// <param name="appId">Id of the app.</param>
        /// <returns></returns>
        private IAsyncToken<ScriptListEvent> GetScriptsFromDisk(string appId)
        {
            return Async.Map(
                _files.Get<ScriptListEvent>(string.Format("appdata://{0}/scripts", appId)),
                file => file.Data);
        }

        /// <summary>
        /// Retrieve scripts from the network.
        /// </summary>
        /// <param name="appId">The id of the app.</param>
        /// <returns></returns>
        private IAsyncToken<ScriptData[]> GetScriptsFromNetwork(string appId)
        {
            var token = new AsyncToken<ScriptData[]>();

            _api
                .Scripts
                .GetAppScripts(appId)
                .OnSuccess(response =>
                {
                    if (response.Payload.Success)
                    {
                        token.Succeed(response.Payload.Body.Select(ToScriptData).ToArray());
                    }
                    else
                    {
                        token.Fail(new Exception(response.Payload.Error));
                    }
                })
                .OnFailure(exception =>
                {
                    token.Fail(new Exception(string.Format("Could not get scripts from network: {0}.", exception)));
                });

            return token;
        }

        /// <summary>
        /// Loads a scene by id.
        /// </summary>
        /// <param name="appId">Id of the app.</param>
        /// <param name="sceneId">The id of the scene.</param>
        /// <returns></returns>
        private IAsyncToken<ElementDescription> LoadSceneFromNetwork(
            string appId,
            string sceneId)
        {
            var token = new AsyncToken<ElementDescription>();

            _sceneLoads[sceneId] = _api
                .Scenes
                .GetScene(appId, sceneId)
                .OnSuccess(response =>
                {
                    object obj;
                    try
                    {
                        var bytes = Encoding.UTF8.GetBytes(response.Payload.Body.Elements);
                        _json.Deserialize(
                            typeof(ElementData),
                            ref bytes,
                            out obj);
                    }
                    catch (Exception exception)
                    {
                        token.Fail(exception);

                        return;
                    }

                    var description = new ElementDescription
                    {
                        Root = new ElementRef
                        {
                            Id = "root"
                        },
                        Elements = new[]
                        {
                            (ElementData) obj
                        }
                    };

                    _files
                        .Set(
                            string.Format("appdata://{0}/Scene/{1}", appId, sceneId),
                            description)
                        .OnFailure(exception => Log.Error(this, "Could not write scene to disk : {0}.", exception));

                    token.Succeed(description);
                })
                .OnFailure(token.Fail);

            return token;
        }

        /// <summary>
        /// Loads a scene by id.
        /// </summary>
        /// <param name="appId">Id of the app.</param>
        /// <param name="sceneId">The id of the scene.</param>
        /// <returns></returns>
        private IAsyncToken<ElementDescription> LoadSceneFromDisk(
            string appId,
            string sceneId)
        {
            return Async.Map(
                _files.Get<ElementDescription>(string.Format("appdata://{0}/Scene/{1}", appId, sceneId)),
                file => file.Data);
        }

        /// <summary>
        /// Creates AssetData from response body.
        /// </summary>
        /// <param name="data">Asset data.</param>
        /// <returns></returns>
        private static AssetData ToAssetData(Asset data)
        {
            var stats = new AssetStatsData();
            if (null != data.Stats)
            {
                stats.TriCount = (int)data.Stats.TriCount;
                stats.VertCount = (int)data.Stats.VertCount;

                if (null != data.Stats.Bounds)
                {
                    stats.Bounds = new AssetStatsBoundsData
                    {
                        Min = new Vec3(
                            (float)data.Stats.Bounds.Min.X,
                            (float)data.Stats.Bounds.Min.Y,
                            (float)data.Stats.Bounds.Min.Z),
                        Max = new Vec3(
                            (float)data.Stats.Bounds.Max.X,
                            (float)data.Stats.Bounds.Max.Y,
                            (float)data.Stats.Bounds.Max.Z)
                    };
                }
            }

            return new AssetData
            {
                Guid = data.Id,
                AssetName = data.Name,
                Crc = data.Crc,
                CreatedAt = data.CreatedAt,
                Owner = data.Owner,
                Description = data.Description,
                Type = data.Type,
                UpdatedAt = data.UpdatedAt,
                Tags = data.Tags,
                Stats = stats,
                Version = (int)data.Version,
                Uri = data.Uri,
                UriThumb = data.UriThumb
            };
        }

        /// <summary>
        /// Creates ScriptData from response body.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <returns></returns>
        private static ScriptData ToScriptData(Body data)
        {
            return new ScriptData
            {
                Id = data.Id,
                Name = data.Name,
                Description = data.Description,
                Crc = data.Crc,
                CreatedAt = data.CreatedAt,
                UpdatedAt = data.UpdatedAt,
                Version = (int)data.Version,
                Uri = data.Uri,
                Owner = data.Owner,
                TagString = data.Tags
            };
        }
    }
}