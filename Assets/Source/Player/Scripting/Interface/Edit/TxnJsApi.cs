﻿using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using Enklu.Orchid;
using Jint;
using Jint.Native;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Js api for transactions.
    /// </summary>
    public class TxnJsApi
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IAppSceneManager _scenes;
        private readonly IElementTxnManager _txns;
        private readonly IElementManager _elements;

        /// <summary>
        /// Constructor.
        /// </summary>
        public TxnJsApi(
            IAppSceneManager scenes,
            IElementTxnManager txns,
            IElementManager elements)
        {
            _scenes = scenes;
            _txns = txns;
            _elements = elements;
        }

        public ElementTxnJs create()
        {
            return new ElementTxnJs(
                _elements,
                new ElementTxn(_scenes.All[0]));
        }

        public string generateId()
        {
            return Guid.NewGuid().ToString();
        }

        public void request(ElementTxnJs txn)
        {
            _txns
                .Request(txn.Txn)
                .OnFailure(ex => Log.Warning(this, "Txn request unsuccessful : {0}", ex));
        }

        public void requestCallback(ElementTxnJs txn, IJsCallback cb)
        {
            _txns
                .Request(txn.Txn)
                .OnSuccess(res => cb.Invoke(null))
                .OnFailure(ex =>
                {
                    Log.Warning(this, "Txn request unsuccessful : {0}", ex);

                    cb.Invoke(ex.Message);
                });
        }
    }
}