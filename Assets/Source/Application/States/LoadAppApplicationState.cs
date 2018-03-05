﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
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
        /// For Http.
        /// </summary>
        private readonly IHttpService _http;

        /// <summary>
        /// Messages.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Trellis connection.
        /// </summary>
        private readonly IConnection _connection;

        /// <summary>
        /// Constructor.
        /// </summary>
        public LoadAppApplicationState(
            ApplicationConfig config,
            ApiController api,
            IHttpService http,
            IMessageRouter messages,
            IConnection connection)
        {
            _config = config;
            _api = api;
            _http = http;
            _messages = messages;
            _connection = connection;
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            // setup http service
            _config
                .Network
                .Credentials(_config.Network.Current)
                .Apply(_http);

            Log.Info(this, "Connect to Trellis...");

            Async
                .All(
                    _connection.Connect(_config.Network.Environment(_config.Network.Current)),
                    GetAssets(),
                    GetScripts())
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
        private IAsyncToken<Void> GetAssets()
        {
            var token = new AsyncToken<Void>();

            _api
                .Assets
                .GetAssets(_config.Play.AppId)
                .OnSuccess(response =>
                {
                    if (response.Payload.Success)
                    {
                        var assets = new List<AssetData>();
                        var data = response.Payload.Body.Assets;
                        for (int i = 0, len = data.Length; i < len; i++)
                        {
                            var element = data[i];
                            assets.Add(ToAssetData(element));
                        }
                        
                        _messages.Publish(
                            MessageTypes.RECV_ASSET_LIST,
                            new AssetListEvent
                            {
                                Assets = assets.ToArray()
                            });

                        token.Succeed(Void.Instance);
                    }
                    else
                    {
                        token.Fail(new Exception(response.Payload.Error));
                    }
                })
                .OnFailure(token.Fail);

            return token;
        }

        /// <summary>
        /// Retrieves ScriptData.
        /// </summary>
        /// <returns></returns>
        private IAsyncToken<Void> GetScripts()
        {
            var token = new AsyncToken<Void>();

            _api
                .Scripts
                .GetAppScripts(_config.Play.AppId)
                .OnSuccess(response =>
                {
                    if (response.Payload.Success)
                    {
                        _messages.Publish(
                            MessageTypes.RECV_SCRIPT_LIST,
                            new ScriptListEvent
                            {
                                Scripts = null != response.Payload.Body
                                    ? response.Payload.Body.Select(ToScriptData).ToArray()
                                    : new ScriptData[0]
                            });

                        token.Succeed(Void.Instance);
                    }
                    else
                    {
                        token.Fail(new Exception(response.Payload.Error));
                    }
                })
                .OnFailure(token.Fail);

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
                        Min = new Vec3((float) data.Stats.Bounds.Min.X, (float) data.Stats.Bounds.Min.Y,
                            (float) data.Stats.Bounds.Min.Z),
                        Max = new Vec3((float) data.Stats.Bounds.Max.X, (float) data.Stats.Bounds.Max.Y,
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
