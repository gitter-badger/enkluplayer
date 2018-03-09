#if !UNITY_EDITOR && UNITY_WSA

using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using UnityEngine.XR.WSA;
using UnityEngine.XR.WSA.Sharing;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Implementation for HoloLens.
    /// </summary>
    public class HoloLensWorldAnchorProvider : IWorldAnchorProvider
    {
        /// <summary>
        /// Import dictionary.
        /// </summary>
        private readonly Dictionary<GameObject, AsyncToken<Void>> _imports = new Dictionary<GameObject, AsyncToken<Void>>();

        /// <summary>
        /// Export dictionary.
        /// </summary>
        private readonly Dictionary<GameObject, AsyncToken<byte[]>> _exports = new Dictionary<GameObject, AsyncToken<byte[]>>();

        /// <inheritdoc />
        public IAsyncToken<byte[]> Export(GameObject gameObject)
        {
            AsyncToken<byte[]> token;
            if (_exports.TryGetValue(gameObject, out token))
            {
                return token.Token();
            }

            token = _exports[gameObject] = new AsyncToken<byte[]>();

            // TODO: pool these
            var buffer = new byte[4096];
            var index = 0;

            Action<byte[]> onExportDataAvailable = bytes =>
            {
                var len = bytes.Length;
                var delta = buffer.Length - index;

                // resize buffer
                while (len > delta)
                {
                    var target = len * 2;
                    Log.Warning(this, "Increasing size to {0} bytes.", target);

                    var newBuffer = new byte[target];
                    Array.Copy(buffer, 0, newBuffer, 0, index);
                    buffer = newBuffer;

                    delta = buffer.Length - index;
                }

                Array.Copy(bytes, 0, buffer, index, len);
                index += len;
            };

            Action<SerializationCompletionReason> onExportComplete = reason =>
            {
                _exports.Remove(gameObject);

                if (reason == SerializationCompletionReason.Succeeded)
                {
                    // prep buffer
                    var completeBuffer = new byte[index];
                    Array.Copy(buffer, 0, completeBuffer, 0, index);

                    token.Succeed(completeBuffer);
                }
                else
                {
                    token.Fail(new Exception(string.Format(
                        "Could not export : {0}.",
                        reason)));
                }
            };

            var anchor = gameObject.GetComponent<WorldAnchor>()
                ?? gameObject.AddComponent<WorldAnchor>();

            var batch = new WorldAnchorTransferBatch();
            batch.AddWorldAnchor(gameObject.name, anchor);
            WorldAnchorTransferBatch.ExportAsync(
                batch,
                new WorldAnchorTransferBatch.SerializationDataAvailableDelegate(onExportDataAvailable),
                new WorldAnchorTransferBatch.SerializationCompleteDelegate(onExportComplete));

            return token;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Import(GameObject gameObject, byte[] bytes)
        {
            AsyncToken<Void> token;
            if (_imports.TryGetValue(gameObject, out token))
            {
                return token.Token();
            }

            token = _imports[gameObject] = new AsyncToken<Void>();

            Action<SerializationCompletionReason, WorldAnchorTransferBatch> onComplete = (reason, batch) =>
            {
                _imports.Remove(gameObject);

                if (reason != SerializationCompletionReason.Succeeded)
                {
                    token.Fail(new Exception(string.Format(
                        "Could not import : {0}.",
                        reason)));
                    return;
                }

                batch.LockObject(gameObject.name, gameObject);
            };

            WorldAnchorTransferBatch.ImportAsync(
                bytes,
                new WorldAnchorTransferBatch.DeserializationCompleteDelegate(onComplete));

            return token;
        }

        /// <inheritdoc />
        public void Disable(GameObject gameObject)
        {
            var anchor = gameObject.GetComponent<WorldAnchor>();
            if (null != anchor)
            {
                //anchor.enabled = false;
            }
        }

        /// <inheritdoc />
        public void Enable(GameObject gameObject)
        {
            var anchor = gameObject.GetComponent<WorldAnchor>();
            if (null != anchor)
            {
                //anchor.enabled = true;
            }
        }
    }
}

#endif