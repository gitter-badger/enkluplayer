using System;
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
        /// Configuration.
        /// </summary>
        private readonly TestDataConfig _config;

        /// <summary>
        /// Constructor.
        /// </summary>
        public TestDataController(
            IMessageRouter message,
            TestDataConfig config)
        {
            _messages = message;
            _config = config;

            Action unsub = null;

            _messages.Subscribe(
                MessageTypes.READY,
                _ =>
                {
                    if (null != unsub)
                    {
                        unsub();
                    }

                    unsub = _messages.Subscribe(
                        MessageTypes.ASSET_LIST,
                        __ => LoadAssetData(_config.Data));

                    LoadAssetData(_config.Data);
                });
        }
        
        /// <summary>
        /// Loads static asset data.
        /// </summary>
        private void LoadAssetData(ApplicationTestData data)
        {
            var assets = Read<AssetData>(data.Asset);

            foreach (var asset in assets)
            {
                _messages.Publish(MessageTypes.ASSET_ADD, new AssetAddEvent
                {
                    Asset = asset
                });
            }
        }

        /// <summary>
        /// Loads static content data.
        /// </summary>
        private void LoadContentData(ApplicationTestData data)
        {
            var contents = Read<ContentData>(data.Content);

            foreach (var content in contents)
            {
                _messages.Publish(MessageTypes.CONTENT_ADD, new ContentAddEvent
                {
                    Content = content
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