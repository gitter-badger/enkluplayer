using System;
using System.Collections;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.AR;
using CreateAR.SpirePlayer.Assets;
using CreateAR.SpirePlayer.BLE;
using UnityEngine;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Initializes the application.
    /// </summary>
    public class InitializeApplicationState : IState
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IMessageRouter _messages;
        private readonly IBootstrapper _bootstrapper;
        private readonly IFileManager _files;
        private readonly IAssetManager _assets;
        private readonly IAssetLoader _assetLoader;
        private readonly IArService _ar;
        private readonly IBleService _ble;
        private readonly ArServiceConfiguration _arConfig;
        private readonly BleServiceConfiguration _bleConfig;
        private readonly UrlFormatterCollection _urls;

        /// <summary>
        /// App config.
        /// </summary>
        private ApplicationConfig _appConfig;

        /// <summary>
        /// Time at which we started looking for the floor.
        /// </summary>
        private DateTime _startFloorSearch;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public InitializeApplicationState(
            IMessageRouter messages,
            IBootstrapper bootstrapper,
            IFileManager files,
            IAssetManager assets,
            IAssetLoader assetLoader,
            IArService ar,
            IBleService ble,
            ArServiceConfiguration arConfig,
            BleServiceConfiguration bleConfig,
            UrlFormatterCollection urls)
        {
            _messages = messages;
            _bootstrapper = bootstrapper;
            _files = files;
            _assets = assets;
            _assetLoader = assetLoader;
            _arConfig = arConfig;
            _ar = ar;
            _bleConfig = bleConfig;
            _ble = ble;
            _urls = urls;
        }

        /// <inheritdoc cref="IState"/>
        public void Enter(object context)
        {
            _appConfig = (ApplicationConfig) context;

            // ar
            _ar.Setup(_arConfig);

            // ble
            _ble.Setup(_bleConfig);

            // setup URL builders from environment
            {
                var env = _appConfig.Network.Environment;

                var trellisFormatter = new LoggedUrlFormatter();
                if (!trellisFormatter.FromUrl(env.TrellisUrl))
                {
                    Log.Error(this, "Invalid trellis URL : " + env.TrellisUrl);
                }

                var assetsFormatter = new LoggedUrlFormatter();
                if (!assetsFormatter.FromUrl(env.AssetsUrl))
                {
                    Log.Error(this, "Invalid assets URL : " + env.AssetsUrl);
                }

                var thumbsFormatter = new LoggedUrlFormatter();
                if (!thumbsFormatter.FromUrl(env.ThumbsUrl))
                {
                    Log.Error(this, "Invalid thumbs URL : " + env.ThumbsUrl);
                }

                _urls.Register("trellis", trellisFormatter);
                _urls.Register("assets", assetsFormatter);
                _urls.Register("thumbs", thumbsFormatter);
            }
            // files
            {
                _files.Register(
                    "appdata://",
                    new JsonSerializer(),
                    new LocalFileSystem("AppData"));
            }

            // reset assets
            _assets.Uninitialize();
            
            // wait for assets to initialize and for the floor to be recognized
            var tasks = new List<IAsyncToken<Void>>
            {
                _assets.Initialize(new AssetManagerConfiguration
                {
                    Loader = _assetLoader,
                    Queries = new StandardQueryResolver()
                }),
                FindFloor()
            };
            
            Async
                .All(tasks.ToArray())
                .OnSuccess(_ =>
                {
                    _messages.Publish(
                        MessageTypes.APPLICATION_INITIALIZED,
                        Void.Instance);
                })
                .OnFailure(exception =>
                {
                    // rethrow
                    throw exception;
                });
        }

        /// <inheritdoc cref="IState"/>
        public void Update(float dt)
        {
            
        }

        /// <inheritdoc cref="IState"/>
        public void Exit()
        {
            // untag all anchors
            foreach (var anchor in _ar.Anchors)
            {
                anchor.ClearTags();
            }
        }
        
        /// <summary>
        /// Finds the floor, tags it, then resolves the token.
        /// 
        /// TODO: Should be part of <c>IArService</c>.
        /// </summary>
        /// <returns></returns>
        private IAsyncToken<Void> FindFloor()
        {
            var token = new AsyncToken<Void>();

            Log.Info(this, "Attempting to find the floor.");
            
            _startFloorSearch = DateTime.Now;
            _bootstrapper.BootstrapCoroutine(PollAnchors(token));

            return token;
        }

        /// <summary>
        /// Polls the list of anchors for a floor. 
        /// </summary>
        /// <param name="token">The token to resolve when found.</param>
        /// <returns></returns>
        private IEnumerator PollAnchors(AsyncToken<Void> token)
        {
            while (true)
            {
                var deltaSec = (DateTime.Now.Subtract(_startFloorSearch).TotalSeconds);
                
                // wait at least min
                if (deltaSec < Mathf.Max(1, _arConfig.MinSearchSec))
                {
                    //
                }
                else
                {
                    // look for lowest anchor to call floor
                    var anchors = _ar.Anchors;
                    ArAnchor lowest = null;
                    for (int i = 0, len = anchors.Length; i < len; i++)
                    {
                        var anchor = anchors[i];
                        if (null == lowest
                            || anchor.Position.y < lowest.Position.y)
                        {
                            lowest = anchor;
                        }
                    }
                
                    // floor found!
                    if (null != lowest)
                    {
                        Log.Info(this, "Floor found : {0}.", lowest);
                    
                        // tag it
                        lowest.Tag(ArAnchorTags.FLOOR);
                        
                        // set camera rig
                        _arConfig.Rig.SetFloor(lowest);
                    
                        token.Succeed(Void.Instance);
                        yield break;
                    }
                
                    // waited too long!
                    if (deltaSec > _arConfig.MaxSearchSec)
                    {
                        token.Fail(new Exception("Timeout."));
                        yield break;
                    }
                }
                
                yield return null;
            }
        }
    }
}