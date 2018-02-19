﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using CreateAR.Trellis.Messages;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Manages element transactions.
    /// </summary>
    public class ElementTxnManager : IElementTxnManager
    {
        /// <summary>
        /// Api.
        /// </summary>
        private readonly ApiController _api;

        /// <summary>
        /// Json.
        /// </summary>
        private readonly JsonSerializer _json;

        /// <summary>
        /// Makes Http services.
        /// </summary>
        private readonly IHttpService _http;

        /// <summary>
        /// Creates elements.
        /// </summary>
        private readonly IElementFactory _elements;

        /// <summary>
        /// Creates strategies.
        /// </summary>
        private readonly IElementActionStrategyFactory _strategyFactory;

        /// <summary>
        /// Creates stores.
        /// </summary>
        private readonly IElementTxnStoreFactory _storeFactory;

        /// <summary>
        /// Lookup from sceneId -> scene loads.
        /// </summary>
        private readonly Dictionary<string, IAsyncToken<Void>> _sceneLoads = new Dictionary<string, IAsyncToken<Void>>();

        /// <summary>
        /// Lookup from sceneId -> store.
        /// </summary>
        private readonly Dictionary<string, IElementTxnStore> _stores = new Dictionary<string, IElementTxnStore>();

        /// <summary>
        /// App id.
        /// </summary>
        private string _appId;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ElementTxnManager(
            ApiController api,
            JsonSerializer json,
            IHttpService http,
            IElementFactory elements,
            IElementActionStrategyFactory strategyFactory,
            IElementTxnStoreFactory storeFactory)
        {
            _api = api;
            _json = json;
            _http = http;
            _elements = elements;
            _strategyFactory = strategyFactory;
            _storeFactory = storeFactory;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Initialize(string appId)
        {
            _appId = appId;

            return new AsyncToken<Void>(Void.Instance);
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Uninitialize()
        {
            _appId = string.Empty;

            foreach (var pair in _sceneLoads)
            {
                pair.Value.Abort();
            }
            _sceneLoads.Clear();

            foreach (var pair in _stores)
            {
                // TODO: destroy?
            }
            _stores.Clear();

            return new AsyncToken<Void>(Void.Instance);
        }

        /// <inheritdoc />
        public void TrackScene(string sceneId)
        {
            IAsyncToken<Void> token;
            if (!_sceneLoads.TryGetValue(sceneId, out token))
            {
                _sceneLoads[sceneId] = LoadScene(sceneId);
            }
        }

        /// <inheritdoc />
        public void UntrackScene(string sceneId)
        {
            IAsyncToken<Void> token;
            if (_sceneLoads.TryGetValue(sceneId, out token))
            {
                token.Abort();
            }
            _sceneLoads.Remove(sceneId);

            IElementTxnStore store;
            if (_stores.TryGetValue(sceneId, out store))
            {
                // TODO: teardown necessary?
            }
            _stores.Remove(sceneId);
        }

        /// <inheritdoc />
        public void Request(ElementTxn txn)
        {
            // find scene
            IElementTxnStore store;
            if (!_stores.TryGetValue(txn.SceneId, out store))
            {
                Log.Warning(this, "Cannot make transaction request against untracked scene. Did you forget to call Track() first?");
                return;
            }

            // send to store
            string error;
            if (!store.Request(txn, out error))
            {
                Log.Warning(this, "Could not process txn : {0}.", error);
                return;
            }
            
            // translate into network requests
            var requestedActions = new ElementRequest[txn.Actions.Count];
            for (var i = 0; i < txn.Actions.Count; i++)
            {
                var action = txn.Actions[i];
                requestedActions[i] = ToElementRequest(action);
            }

            // send
            _http
                .Put<Trellis.Messages.UpdateSceneElement.Response>(
                    _http.UrlBuilder.Url(string.Format("/editor/app/{0}/scene/{1}", _appId, txn.SceneId)),
                    new ElementTxnRequest
                    {
                        Actions = requestedActions.ToArray()
                    })
                .OnSuccess(response =>
                {
                    if (response.Payload.Success)
                    {
                        store.Commit(txn.Id);
                    }
                    else
                    {
                        Log.Error(this,
                            "Error updating element : {0}.",
                            response.Payload.Error);

                        store.Rollback(txn.Id);
                    }
                })
                .OnFailure(exception =>
                {
                    Log.Error(this,
                        "Error sending element txn : {0}.",
                        exception);

                    // rollback txn
                    store.Rollback(txn.Id);
                });
        }

        /// <summary>
        /// Loads a scene by id.
        /// </summary>
        /// <param name="sceneId">The id of the scene.</param>
        /// <returns></returns>
        private IAsyncToken<Void> LoadScene(string sceneId)
        {
            var token = new AsyncToken<Void>();

            _api
                .Scenes
                .GetScene(_appId, sceneId)
                .OnSuccess(response =>
                {
                    if (response.Payload.Success)
                    {
                        object obj;
                        var bytes = Encoding.UTF8.GetBytes(response.Payload.Body.Elements);
                        _json.Deserialize(
                            typeof(ElementData),
                            ref bytes,
                            out obj);

                        var root = _elements.Element(new ElementDescription
                        {
                            Root = new ElementRef
                            {
                                Id = "root"
                            },
                            Elements = new []
                            {
                                (ElementData) obj
                            }
                        });

                        var strategy = _strategyFactory.Instance(root);
                        _stores[sceneId] = _storeFactory.Instance(strategy);

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
        /// Creates <c>ElementRequest</c> from <c>ElementActionData</c>.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private ElementRequest ToElementRequest(ElementActionData action)
        {
            switch (action.Type)
            {
                case ElementActionTypes.CREATE:
                    {
                        return new CreateElementRequest
                        {
                            Type = ElementActionTypes.CREATE,
                            ElementId = action.ElementId,
                            ParentId = action.ParentId
                        };
                    }
                case ElementActionTypes.UPDATE:
                    {
                        switch (action.SchemaType)
                        {
                            case ElementActionSchemaTypes.INT:
                                {
                                    return new UpdateElementIntRequest
                                    {
                                        Type = ElementActionTypes.UPDATE,
                                        ElementId = action.ElementId,
                                        SchemaType = action.SchemaType,
                                        Key = action.Key,
                                        Value = int.Parse(action.Value)
                                    };
                                }
                            case ElementActionSchemaTypes.FLOAT:
                                {
                                    return new UpdateElementFloatRequest
                                    {
                                        Type = ElementActionTypes.UPDATE,
                                        ElementId = action.ElementId,
                                        SchemaType = action.SchemaType,
                                        Key = action.Key,
                                        Value = float.Parse(action.Value)
                                    };
                                }
                            case ElementActionSchemaTypes.BOOL:
                                {
                                    return new UpdateElementBoolRequest
                                    {
                                        Type = ElementActionTypes.UPDATE,
                                        ElementId = action.ElementId,
                                        SchemaType = action.SchemaType,
                                        Key = action.Key,
                                        Value = bool.Parse(action.Value)
                                    };
                                }
                            case ElementActionSchemaTypes.STRING:
                                {
                                    return new UpdateElementStringRequest
                                    {
                                        Type = ElementActionTypes.UPDATE,
                                        ElementId = action.ElementId,
                                        SchemaType = action.SchemaType,
                                        Key = action.Key,
                                        Value = action.Value
                                    };
                                }
                            case ElementActionSchemaTypes.VEC3:
                                {
                                    var split = action.Value.Split(',');

                                    return new UpdateElementVec3Request
                                    {
                                        Type = ElementActionTypes.UPDATE,
                                        ElementId = action.ElementId,
                                        SchemaType = action.SchemaType,
                                        Key = action.Key,
                                        Value = new Vec3(
                                            float.Parse(split[0]),
                                            float.Parse(split[1]),
                                            float.Parse(split[2]))
                                    };
                                }
                            case ElementActionSchemaTypes.COL4:
                                {
                                    var split = action.Value.Split(',');

                                    return new UpdateElementCol4Request
                                    {
                                        Type = ElementActionTypes.UPDATE,
                                        ElementId = action.ElementId,
                                        SchemaType = action.SchemaType,
                                        Key = action.Key,
                                        Value = new Col4(
                                            float.Parse(split[0]),
                                            float.Parse(split[1]),
                                            float.Parse(split[2]),
                                            float.Parse(split[3]))
                                    };
                                }
                            default:
                                {
                                    Log.Error(this,
                                        "Unknown schemaType '{0}'.",
                                        action.SchemaType);
                                    return null;
                                }
                        }
                    }
                case ElementActionTypes.DELETE:
                    {
                        return new DeleteElementRequest
                        {
                            Type = ElementActionTypes.DELETE,
                            ElementId = action.ElementId
                        };
                    }
                default:
                    {
                        return null;
                    }
            }
        }
    }
}