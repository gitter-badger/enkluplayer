using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.AR;
using CreateAR.SpirePlayer.Assets;
using CreateAR.SpirePlayer.BLE;
using CreateAR.SpirePlayer.IUX;
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
        private readonly IHttpService _http;
        private readonly IBootstrapper _bootstrapper;
        private readonly IHashProvider _hashMethod;
        private readonly IAssetManager _assets;
        private readonly ArServiceConfiguration _arConfig;
        private readonly IArService _ar;
        private readonly BleServiceConfiguration _bleConfig;
        private readonly IBleService _ble;

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
            IHttpService http,
            IBootstrapper bootstrapper,
            IHashProvider hashMethod,
            IAssetManager assets,
            ArServiceConfiguration arConfig,
            IArService ar,
            BleServiceConfiguration bleConfig,
            IBleService ble,
            IImageLoader imageLoader)
        {
            _messages = messages;
            _http = http;
            _bootstrapper = bootstrapper;
            _hashMethod = hashMethod;
            _assets = assets;
            _arConfig = arConfig;
            _ar = ar;
            _bleConfig = bleConfig;
            _ble = ble;

            imageLoader.ReplaceProtocol(
                "assets",
                "https://assets.enklu.com:9091");
        }

        /// <inheritdoc cref="IState"/>
        public void Enter(object context)
        {
            _appConfig = (ApplicationConfig) context;

            // ar
            _ar.Setup(_arConfig);

            // ble
            _ble.Setup(_bleConfig);
            
            // setup http
            var env = _appConfig.Network.Environment(_appConfig.Network.Current);
            _http.UrlBuilder.BaseUrl = env.BaseUrl;
            _http.UrlBuilder.Version = env.ApiVersion;
            _http.UrlBuilder.Port = env.Port;

            // setup assets
            var loader = new StandardAssetLoader(
                _bootstrapper,
                new StandardAssetBundleCache(
                    _bootstrapper,
                    _hashMethod,
                    Path.Combine(
                        UnityEngine.Application.persistentDataPath,
                        "Bundles")), 
                new UrlBuilder
                {
                    BaseUrl = "assets.enklu.com",
                    Port = 9091,
                    Protocol = "https"
                });

            // reset assets
            _assets.Uninitialize();
            
            // wait for assets to initialize and for the floor to be recognized
            var tasks = new List<IAsyncToken<Void>>
            {
                _assets.Initialize(new AssetManagerConfiguration
                {
                    Loader = loader,
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