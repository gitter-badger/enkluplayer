using System;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.GetAssets;
using Body = CreateAR.Trellis.Messages.GetAppScripts.Body;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// State that loads necessary data before progressing to play. This is used
    /// when the player is running without a connected editor to push it data.
    /// </summary>
    public class LoadAppApplicationState : IState
    {
        /// <summary>
        /// Configuration.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// API.
        /// </summary>
        private readonly ApiController _api;
        
        /// <summary>
        /// Messages.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Trellis connection.
        /// </summary>
        private readonly IConnection _connection;

        /// <summary>
        /// Files.
        /// </summary>
        private readonly IFileManager _files;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public LoadAppApplicationState(
            ApplicationConfig config,
            ApiController api,
            IMessageRouter messages,
            IConnection connection,
            IFileManager files)
        {
            _config = config;
            _api = api;
            _messages = messages;
            _connection = connection;
            _files = files;
            
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            Log.Info(this, "Connect to Trellis...");

            Async
                .All(
                    _connection.Connect(_config.Network.Environment),
                    GetAssets(_config.Play.AppId),
                    GetScripts(_config.Play.AppId))
                .OnSuccess(_ =>
                {
                    Log.Info(this, "App loaded, proceeding to play.");

                    _messages.Publish(MessageTypes.PLAY);
                })
                .OnFailure(exception =>
                {
                    Log.Error(this,
                        "Could not load prerequisites for app : {0}.",
                        exception);
                });
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            
        }

        /// <inheritdoc />
        public void Exit()
        {
            
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
                .OnSuccess(assets =>
                {
                    _messages.Publish(
                        MessageTypes.RECV_ASSET_LIST,
                        new AssetListEvent
                        {
                            Assets = assets
                        });

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
                    // write to disk for next time
                    _files
                        .Set(string.Format("appdata://{0}/assets", appId), assets)
                        .OnFailure(exception => Log.Error(this, "Could not write assets data to disk : {0}.", exception));

                    // cancel disk load
                    diskLoad.Abort();

                    _messages.Publish(
                        MessageTypes.RECV_ASSET_LIST,
                        new AssetListEvent
                        {
                            Assets = assets
                        });

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
        private IAsyncToken<AssetData[]> GetAssetsFromDisk(string appId)
        {
            return Async.Map(
                _files.Get<AssetData[]>(string.Format("appdata://{0}/assets", appId)),
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
                .OnSuccess(scripts =>
                {
                    _messages.Publish(
                        MessageTypes.RECV_SCRIPT_LIST,
                        new ScriptListEvent
                        {
                            Scripts = scripts
                        });

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
                    // write to disk for next time
                    _files
                        .Set(string.Format("appdata://{0}/scripts", appId), scripts)
                        .OnFailure(exception => Log.Error(this, "Could not write scripts data to disk : {0}.", exception));

                    // cancel disk load
                    diskLoad.Abort();

                    _messages.Publish(
                        MessageTypes.RECV_SCRIPT_LIST,
                        new ScriptListEvent
                        {
                            Scripts = scripts
                        });

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
        private IAsyncToken<ScriptData[]> GetScriptsFromDisk(string appId)
        {
            return Async.Map(
                _files.Get<ScriptData[]>(string.Format("appdata://{0}/scripts", appId)),
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
        /// Creates AssetData from response body.
        /// </summary>
        /// <param name="data">Asset data.</param>
        /// <returns></returns>
        private AssetData ToAssetData(Asset data)
        {
            var stats = new AssetStatsData();
            if (null != data.Stats)
            {
                stats.TriCount = (int) data.Stats.TriCount;
                stats.VertCount = (int) data.Stats.VertCount;
                
                if (null != data.Stats.Bounds)
                {
                    stats.Bounds = new AssetStatsBoundsData
                    {
                        Min = new Vec3(
                            (float) data.Stats.Bounds.Min.X,
                            (float) data.Stats.Bounds.Min.Y,
                            (float) data.Stats.Bounds.Min.Z),
                        Max = new Vec3(
                            (float) data.Stats.Bounds.Max.X,
                            (float) data.Stats.Bounds.Max.Y,
                            (float) data.Stats.Bounds.Max.Z)
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
        private ScriptData ToScriptData(Body data)
        {
            return new ScriptData
            {
                Id = data.Id,
                Name = data.Name,
                Description = data.Description,
                Crc = data.Crc,
                CreatedAt = data.CreatedAt,
                UpdatedAt = data.UpdatedAt,
                Version = (int) data.Version,
                Uri = data.Uri,
                Owner = data.Owner,
                TagString = data.Tags
            };
        }
    }
}
