using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CreateAR.Commons.Unity.Async;
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
    /// <summary>
    /// Cached data with scene list.
    /// 
    /// TODO: Move to user profile object.
    /// </summary>
    public class AppSceneListCacheData
    {
        /// <summary>
        /// List of Scenes.
        /// </summary>
        public string[] Scenes;
    }

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
            IMessageRouter messages,
            IFileManager files)
        {
            _api = api;
            _messages = messages;
            
            _helper = new HttpRequestCacher(files);
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
            
            // get app
            _helper.Request(
                    HttpRequestCacher.LoadBehavior.NetworkFirst,
                    "appdata://" + appId + "/scenelist",
                    () => _api.Apps.GetApp(appId))
                .OnSuccess(response =>
                {
                    Log.Info(this, "Retrieved scene list.");

                    // load each scene
                    Async
                        .All(response
                            .Body
                            .Scenes
                            .Select(scene => LoadScene(appId, scene).OnSuccess(description => _sceneData[appId] = description))
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
                    () => _api.Assets.GetAssets(appId))
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
                    () => _api.Scripts.GetAppScripts(appId))
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
                    () => _api.Scenes.GetScene(appId, sceneId))
                .OnSuccess(response =>
                {
                    object obj;
                    try
                    {
                        var bytes = Encoding.UTF8.GetBytes(response.Body.Elements);
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