using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.GetPublishedAssets;
using UnityEngine;
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
        /// User prefs.
        /// </summary>
        private readonly UserPreferenceService _prefs;
        
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

        /// <summary>
        /// Network config.
        /// </summary>
        private readonly NetworkConfig _networkConfig;

        /// <summary>
        /// IBootstapper.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;

        /// <summary>
        /// Tokens used by the initial load behavior.
        /// </summary>
        private IAsyncToken<Trellis.Messages.GetPublishedAssets.Response> _assetToken;
        private IAsyncToken<Trellis.Messages.GetPublishedAppScripts.Response> _scriptToken;
        private IAsyncToken<Trellis.Messages.GetPublishedApp.Response> _sceneToken;
        private IAsyncToken<Void[]> _primaryLoadToken;

        /// <inheritdoc />
        public string Name { get; private set; }

        public static bool ForceUpdate;

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
            UserPreferenceService prefs,
            IMessageRouter messages,
            IBootstrapper bootstrapper,
            NetworkConfig networkConfig)
        {
            _api = api;
            _helper = cache;
            _prefs = prefs;
            _messages = messages;
            _bootstrapper = bootstrapper;
            _networkConfig = networkConfig;
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

            var token = new AsyncToken<Void>();
            
            // first, load user prefs
            _prefs
                .ForCurrentUser()
                .OnSuccess(sync =>
                {
                    var appPrefs = sync.Data.App(id);

                    var behavior = HttpRequestCacher.LoadBehavior.NetworkFirst;
                    if (!config.Edit && config.PeriodicUpdates)
                    {
                        Log.Info(this, "Periodic update requested.");

                        if (string.IsNullOrEmpty(appPrefs.LastUpdate))
                        {
                            Log.Info(this, "App has never been updatd, proceeding with network load.");
                        }
                        else
                        {
                            DateTime lastUpdate;
                            if (DateTime.TryParse(appPrefs.LastUpdate, out lastUpdate))
                            {
                                var delta = DateTime.Now.Subtract(lastUpdate).TotalMinutes;
                                if (delta < config.PeriodicUpdatesMinutes)
                                {
                                    Log.Info(this, "Periodic update has not expired. Loading app from disk.");

                                    behavior = HttpRequestCacher.LoadBehavior.DiskOnly;
                                }
                                else
                                {
                                    Log.Info(this, "Periodic update expired ({0} minutes), refreshing from network.",
                                        delta);
                                }
                            }
                            else
                            {
                                Log.Warning(this, "Invalid DateTime string, proceeding with network load.");
                            }
                        }
                    }

                    if (ForceUpdate)
                    {
                        ForceUpdate = false;

                        behavior = HttpRequestCacher.LoadBehavior.NetworkFirst;
                    }

                    // save last updated time
                    if (behavior == HttpRequestCacher.LoadBehavior.NetworkFirst)
                    {
                        sync.Queue((state, up) =>
                        {
                            Log.Info(this, "Saving last update time.");

                            state.App(id).LastUpdate = DateTime.Now.ToString();

                            up(state);
                        });
                    }

                    LoadApp(id, behavior)
                        .OnSuccess(token.Succeed)
                        .OnFailure(token.Fail);
                })
                .OnFailure(ex =>
                {
                    Log.Info(this, "Could not get user prefs : {0}", ex);

                    // load
                    LoadApp(id, HttpRequestCacher.LoadBehavior.NetworkFirst)
                        .OnSuccess(token.Succeed)
                        .OnFailure(token.Fail);
                });

            return token;
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
        /// Updates app data.
        /// </summary>
        private IAsyncToken<Void> LoadApp(
            string appId,
            HttpRequestCacher.LoadBehavior behavior)
        {
            var rtnToken = new AsyncToken<Void>();

            // Setup primary behavior downloads
            _primaryLoadToken = Async.All(
                    LoadPrerequisites(appId, behavior), 
                    LoadScenes(appId, behavior))
                .OnSuccess(_ => rtnToken.Succeed(Void.Instance))
                .OnFailure(rtnToken.Fail)
                .OnFinally(_ => InvalidateTokens());
            
            // Set delay before giving up on the network load
            if (_networkConfig.DiskFallbackSecs > float.Epsilon && behavior == HttpRequestCacher.LoadBehavior.NetworkFirst)
            {
                _bootstrapper.BootstrapCoroutine(WaitForLoad(_networkConfig.DiskFallbackSecs, appId, rtnToken));
            }
            
            return rtnToken;
        }

        /// <summary>
        /// Waits for a specified number of seconds before starting to load from disk.
        /// </summary>
        /// <param name="seconds"></param>
        /// <param name="appId"></param>
        /// <param name="rtnToken"></param>
        /// <returns></returns>
        private IEnumerator WaitForLoad(float seconds, string appId, AsyncToken<Void> rtnToken)
        {
            yield return new WaitForSeconds(seconds);

            if (_primaryLoadToken == null)
            {
                // Primary download finished in time!
                yield break;
            }

            // Abort existing tokens
            _assetToken.Abort();
            _scriptToken.Abort();
            _sceneToken.Abort();
            Log.Warning(this, "Timeout waiting for network content. Loading from disk.");

            Async.All(
                    LoadPrerequisites(appId, HttpRequestCacher.LoadBehavior.DiskOnly),
                    LoadScenes(appId, HttpRequestCacher.LoadBehavior.DiskOnly))
                .OnSuccess(_ => { rtnToken.Succeed(Void.Instance); })
                .OnFailure(rtnToken.Fail)
                .OnFinally(_ => InvalidateTokens());
        }

        /// <summary>
        /// Nulls out the tokens used during loading.
        /// </summary>
        private void InvalidateTokens()
        {
            _primaryLoadToken = null;
            _assetToken = null;
            _scriptToken = null;
            _sceneToken = null;
        }

        /// <summary>
        /// Loads prerequisites for an app.
        /// </summary>
        private IAsyncToken<Void> LoadPrerequisites(string appId, HttpRequestCacher.LoadBehavior behavior)
        {
            return Async.Map(
                Async.All(
                    GetAssets(appId, behavior),
                    GetScripts(appId, behavior)),
                _ => Void.Instance);
        }

        /// <summary>
        /// Loads all of an app's scenes.
        /// </summary>
        private IAsyncToken<Void> LoadScenes(
            string appId,
            HttpRequestCacher.LoadBehavior behavior)
        {
            var token = new AsyncToken<Void>();
            
            // get app
            _sceneToken = _helper
                .Request(
                    behavior,
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
                            .Select(scene => LoadScene(appId, scene, behavior).OnSuccess(description => _sceneData[scene] = description))
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
        private IAsyncToken<Void> GetAssets(string appId, HttpRequestCacher.LoadBehavior behavior)
        {
            var token = new AsyncToken<Void>();
            
            // load from network
            _assetToken = _helper.Request(
                    behavior,
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
        private IAsyncToken<Void> GetScripts(string appId, HttpRequestCacher.LoadBehavior behavior)
        {
            var token = new AsyncToken<Void>();

            // load from network
            _scriptToken = _helper.Request(
                    behavior,
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
        private IAsyncToken<ElementDescription> LoadScene(
            string appId,
            string sceneId,
            HttpRequestCacher.LoadBehavior behavior)
        {
            var token = new AsyncToken<ElementDescription>();

            _sceneLoads[sceneId] = _helper.Request(
                    behavior,
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