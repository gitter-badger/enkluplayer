using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.GetPublishedAssets;
using Response = CreateAR.Trellis.Messages.GetPublishedScene.Response;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer
{
    /// <inheritdoc />
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
        /// Messages.
        /// </summary>
        private readonly IMessageRouter _messages;
        
        /// <summary>
        /// Lookup from sceneId -> scene loads.
        /// </summary>
        private readonly Dictionary<string, IAsyncToken<Response>> _sceneLoads = new Dictionary<string, IAsyncToken<Response>>();

        /// <summary>
        /// Data for each loaded scene.
        /// </summary>
        private readonly Dictionary<string, ElementDescription> _sceneData = new Dictionary<string, ElementDescription>();

        /// <summary>
        /// Http helper.
        /// </summary>
        private readonly HttpRequestCacher _helper;

        /// <inheritdoc />
        public string Name { get; private set; }

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
            HttpRequestCacher cache,
            IMessageRouter messages)
        {
            _api = api;
            _messages = messages;
            _helper = cache;
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
        public IAsyncToken<Void> Load(PlayAppConfig config)
        {
            var id = config.AppId;

            Log.Info(this, "Load App {0}.", id);

            return Async.Map(
                Async.All(
                    LoadPrerequisites(id),
                    LoadScenes(id)),
                _ => Void.Instance);
        }

        /// <inheritdoc />
        public void Unload()
        {
            foreach (var pair in _sceneLoads)
            {
                pair.Value.Abort();
            }

            _sceneLoads.Clear();
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
            
            // get app
            _helper
                .Request(
                    HttpRequestCacher.LoadBehavior.NetworkFirst,
                    "appdata://" + appId + "/scenelist",
                    () => _api.PublishedApps.GetPublishedApp(appId))
                .OnSuccess(response =>
                {
                    Log.Info(this, "Retrieved scene list:");
                    for (var i = 0; i < response.Body.Scenes.Length; i++)
                    {
                        Log.Info(this, "\t{0}", response.Body.Scenes[i]);
                    }

                    // set name
                    Name = response.Body.Name;

                    // load each scene
                    Async
                        .All(response
                            .Body
                            .Scenes
                            .Select(scene => LoadScene(appId, scene).OnSuccess(description => _sceneData[scene] = description))
                            .ToArray())
                        .OnSuccess(_ =>
                        {
                            Log.Info(this, "Successfully loaded app scenes.");

                            token.Succeed(Void.Instance);
                        })
                        .OnFailure(token.Fail);
                })
                .OnFailure(token.Fail);

            return token;
        }
        
        /// <summary>
        /// Retrieves AssetData.
        /// </summary>
        /// <returns></returns>
        private IAsyncToken<Void> GetAssets(string appId)
        {
            var token = new AsyncToken<Void>();
            
            // load from network
            _helper.Request(
                    HttpRequestCacher.LoadBehavior.NetworkFirst,
                    "appdata://" + appId + "/assets",
                    () => _api.PublishedApps.GetPublishedAssets(appId))
                .OnSuccess(response =>
                {
                    var assets = response.Body.Assets.Select(ToAssetData).ToArray();
                    var @event = new AssetListEvent
                    {
                        Assets = assets
                    };

                    Log.Info(this, "Assets retrieved.");
                    
                    _messages.Publish(
                        MessageTypes.RECV_ASSET_LIST,
                        @event);

                    token.Succeed(Void.Instance);
                })
                .OnFailure(token.Fail);

            return token;
        }
        
        /// <summary>
        /// Retrieves ScriptData.
        /// </summary>
        /// <returns></returns>
        private IAsyncToken<Void> GetScripts(string appId)
        {
            var token = new AsyncToken<Void>();

            // load from network
            _helper.Request(
                    HttpRequestCacher.LoadBehavior.NetworkFirst,
                    "appdata://" + appId + "/scripts",
                    () => _api.PublishedApps.GetPublishedAppScripts(appId))
                .OnSuccess(response =>
                {
                    var scripts = response.Body.Select(ToScriptData).ToArray();
                    var @event = new ScriptListEvent
                    {
                        Scripts = scripts
                    };

                    Log.Info(this, "Scripts retrieved.");
                    
                    _messages.Publish(
                        MessageTypes.RECV_SCRIPT_LIST,
                        @event);

                    token.Succeed(Void.Instance);
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
        private IAsyncToken<ElementDescription> LoadScene(
            string appId,
            string sceneId)
        {
            var token = new AsyncToken<ElementDescription>();

            _sceneLoads[sceneId] = _helper.Request(
                    HttpRequestCacher.LoadBehavior.NetworkFirst,
                    "appdata://" + appId + "/Scenes/" + sceneId,
                    () => _api.PublishedApps.GetPublishedScene(appId, sceneId))
                .OnSuccess(response =>
                {
                    object obj;
                    try
                    {
                        var bytes = Encoding.UTF8.GetBytes(response.Body.Elements);

                        Verbose("Loaded scene.\n{0}", response.Body.Elements);

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
                    
                    token.Succeed(description);
                })
                .OnFailure(token.Fail);

            return token;
        }

        /// <summary>
        /// Logs verbose messages.
        /// </summary>
        [Conditional("LOGGING_VERBOSE")]
        private void Verbose(string message, params object[] replacements)
        {
            Log.Info(this, message, replacements);
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
        private static ScriptData ToScriptData(Trellis.Messages.GetPublishedAppScripts.Body data)
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