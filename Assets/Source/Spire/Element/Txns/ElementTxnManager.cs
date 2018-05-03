﻿using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Manages element transactions.
    /// </summary>
    public class ElementTxnManager : IElementTxnManager
    {
        /// <summary>
        /// Transport implementation.
        /// </summary>
        private readonly IElementTxnTransport _transport;
        
        /// <summary>
        /// Creates strategies.
        /// </summary>
        private readonly IElementActionStrategyFactory _strategyFactory;

        /// <summary>
        /// Creates stores.
        /// </summary>
        private readonly IElementTxnStoreFactory _storeFactory;

        /// <summary>
        /// Lookup from sceneId -> store.
        /// </summary>
        private readonly Dictionary<string, IElementTxnStore> _stores = new Dictionary<string, IElementTxnStore>();
        
        /// <summary>
        /// Ids of transactions.
        /// </summary>
        private readonly List<long> _txnIds = new List<long>();
        
        /// <summary>
        /// App id.
        /// </summary>
        private string _appId;

        /// <summary>
        /// Manages scenes.
        /// </summary>
        private IAppSceneManager _scenes;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public ElementTxnManager(
            IElementTxnTransport transport,
            IElementActionStrategyFactory strategyFactory,
            IElementTxnStoreFactory storeFactory)
        {
            _transport = transport;
            _strategyFactory = strategyFactory;
            _storeFactory = storeFactory;
        }
        
        /// <inheritdoc />
        public IAsyncToken<Void> Initialize(string appId, IAppSceneManager scenes)
        {
            _appId = appId;
            _scenes = scenes;

            var token = new AsyncToken<Void>();

            foreach (var sceneId in scenes.All)
            {
                var root = scenes.Root(sceneId);
                var store = _storeFactory.Instance(_strategyFactory.Instance(root));

                _stores[sceneId] = store;
            }

            return token;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Uninitialize()
        {
            _appId = string.Empty;
            
            // destroy scene stores
            foreach (var pair in _stores)
            {
                pair.Value.Destroy();
            }
            _stores.Clear();
            
            return new AsyncToken<Void>(Void.Instance);
        }
        
        /// <inheritdoc />
        public IAsyncToken<ElementResponse> Request(ElementTxn txn)
        {
            // find scene
            IElementTxnStore store;
            if (!_stores.TryGetValue(txn.SceneId, out store))
            {
                return new AsyncToken<ElementResponse>(new Exception(
                    "Cannot make transaction request against untracked scene. Did you forget to call Track() first?"));
            }

            var elementResponse = new ElementResponse();

            // find affected elements
            AddAffectedElements(txn, elementResponse, ElementActionTypes.DELETE);
            AddAffectedElements(txn, elementResponse, ElementActionTypes.UPDATE);

            // send txn to store
            string error;
            if (!store.Request(txn, out error))
            {
                return new AsyncToken<ElementResponse>(new Exception(string.Format(
                    "Could not process txn : {0}.", error)));
            }

            var token = new AsyncToken<ElementResponse>();
            
            // send
            _txnIds.Add(txn.Id);

            while (_txnIds.Count > 1000)
            {
                _txnIds.RemoveAt(0);
            }

            _transport
                .Request(txn.Id, _appId, txn.SceneId, txn.Actions.ToArray())
                .OnSuccess(_ =>
                {
                    store.Commit(txn.Id);

                    // add created elements
                    AddAffectedElements(txn, elementResponse, ElementActionTypes.CREATE);

                    token.Succeed(elementResponse);
                })
                .OnFailure(exception =>
                {
                    // rollback txn
                    store.Rollback(txn.Id);

                    token.Fail(new Exception(string.Format(
                        "Error sending element txn : {0}.",
                        exception)));
                });

            return token;
        }

        /// <inheritdoc />
        public ElementResponse Apply(ElementTxn txn)
        {
            // find scene
            IElementTxnStore store;
            if (!_stores.TryGetValue(txn.SceneId, out store))
            {
                Log.Warning(
                    this,
                    "Cannot apply transaction against untracked scene. Did you forget to call Track() first?");
                return new ElementResponse();
            }

            var elementResponse = new ElementResponse();

            // find affected elements
            AddAffectedElements(txn, elementResponse, ElementActionTypes.DELETE);
            AddAffectedElements(txn, elementResponse, ElementActionTypes.UPDATE);

            // apply!
            store.Apply(txn);
            
            // add created elements
            AddAffectedElements(txn, elementResponse, ElementActionTypes.CREATE);

            return elementResponse;
        }

        /// <inheritdoc />
        public bool IsTracked(long txnId)
        {
            return _txnIds.Contains(txnId);
        }

        /// <summary>
        /// Finds affected elements and adds them to the response.
        /// </summary>
        /// <param name="txn">The txn.</param>
        /// <param name="elementResponse">The response object.</param>
        /// <param name="actionType">Type of action to inspect.</param>
        private void AddAffectedElements(
            ElementTxn txn,
            ElementResponse elementResponse,
            string actionType)
        {
            var root = _scenes.Root(txn.SceneId);
            for (var i = 0; i < txn.Actions.Count; i++)
            {
                var action = txn.Actions[i];
                if (actionType != action.Type)
                {
                    continue;
                }

                var elementId = null == action.Element || string.IsNullOrEmpty(action.Element.Id)
                    ? action.ElementId
                    : action.Element.Id;
                var element = root.Id == elementId
                    ? root
                    : root.FindOne<Element>(".." + elementId);
                if (null == element)
                {
                    Log.Warning(this,
                        "Could not find affected Element for action {0} : {1}.",
                        action,
                        elementId);
                }
                else
                {
                    elementResponse.Elements.Add(element);
                }
            }
        }
    }
}