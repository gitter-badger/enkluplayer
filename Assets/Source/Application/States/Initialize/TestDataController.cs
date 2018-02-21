using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Adds test data.
    /// </summary>
    public class TestDataController : ITestDataController
    {
        /// <summary>
        /// Sends messages.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Constructor.
        /// </summary>
        public TestDataController(
            IMessageRouter message,
            TestDataConfig config)
        {
            _messages = message;

            Action unsub = null;

            _messages.Subscribe(
                MessageTypes.APPLICATION_INITIALIZED,
                _ =>
                {
                    if (null != unsub)
                    {
                        unsub();
                    }

                    unsub = _messages.Subscribe(
                        MessageTypes.RECV_ASSET_LIST,
                        __ => LoadAssetData(config.Data));

                    LoadAssetData(config.Data);
                });
        }
        
        /// <summary>
        /// Loads static asset data.
        /// </summary>
        private void LoadAssetData(ApplicationTestData data)
        {
            Log.Info(this, "Adding test asset data.");
            
            var assets = Read<AssetData>(data.Asset);

            foreach (var asset in assets)
            {
                _messages.Publish(MessageTypes.RECV_ASSET_ADD, new AssetAddEvent
                {
                    Asset = asset
                });
            }
        }

        /// <summary>
        /// Reads in test data.
        /// </summary>
        /// <typeparam name="T">Type to read.</typeparam>
        /// <param name="asset">Asset to read from.</param>
        /// <returns></returns>
        private T[] Read<T>(TextAsset asset)
        {
            var data = asset.bytes;

            object objects;
            new JsonSerializer().Deserialize(typeof(T[]), ref data, out objects);

            return (T[]) objects;
        }
    }
}