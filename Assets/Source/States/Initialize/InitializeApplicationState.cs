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
using CreateAR.Trellis;
using CreateAR.Trellis.Messages.RefreshToken;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Initializes the application.
    /// </summary>
    public class InitializeApplicationState : IState
    {
        /// <summary>
        /// Minimum amount of time to search for the floor.
        /// </summary>
        private const int MIN_SEC_SEARCH = 3;

        /// <summary>
        /// Maximum amount of time to search for the floor.
        /// </summary>
        private const int MAX_SEC_SEARCH = 20;
        
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
        private readonly ApiController _api;

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
            ApiController api)
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
            _api = api;
        }

        /// <inheritdoc cref="IState"/>
        public void Enter(object context)
        {
            var config = (ApplicationConfig) context;

            //_messages.Publish(MessageTypes.STATUS, "Initializing...");

            // ar
            _ar.Setup(_arConfig);

            // ble
            _ble.Setup(_bleConfig);
            
            // setup http
            var env = config.Network.Environment(config.Network.Current);
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
                    BaseUrl = "ec2-54-202-152-140.us-west-2.compute.amazonaws.com",
                    Port = 9091,
                    Protocol = "http"
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

            // if we're logging in automatically, also wait for login
            if (config.Network.AutoLogin)
            {
                tasks.Add(Login(config.Network));
            }

            Async
                .All(tasks.ToArray())
                .OnSuccess(_ =>
                {
                    _messages.Publish(MessageTypes.READY, Void.Instance);
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
        /// Automatically connects to Trellis.
        /// </summary>
        /// <param name="config">Configuration.</param>
        /// <returns></returns>
        private IAsyncToken<Void> Login(NetworkConfig config)
        {
            var token = new AsyncToken<Void>();

            Log.Info(this, "AutoLogin");

            // setup
            var creds = config.Credentials(config.Current);
            _http.Headers.Add(Commons.Unity.DataStructures.Tuple.Create(
                "Authorization",
                string.Format("Bearer {0}", creds.Token)));

            _api
                .Users
                .RefreshToken(creds.UserId, new Request
                {
                    Token = creds.Token
                })
                .OnSuccess(response =>
                {
                    if (null == response.Payload || !response.Payload.Success)
                    {
                        var message = string.Format("Could not refresh token : {0}.",
                            null != response.Payload
                                ? response.Payload.Error
                                : "Unknown");
                        Log.Error(this, message);

                        token.Fail(new Exception(message));
                    }
                    else
                    {
                        // TODO: resave token

                        token.Succeed(Void.Instance);
                    }
                })
                .OnFailure(token.Fail);

            return token;
        }

        /// <summary>
        /// Finds the floor, tags it, then resolves the token.
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
                if (deltaSec < MIN_SEC_SEARCH)
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
                    if (deltaSec > MAX_SEC_SEARCH)
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