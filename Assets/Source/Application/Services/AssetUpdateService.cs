using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.Assets;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Handles <c>Asset</c> related updates.
    /// </summary>
    public class AssetUpdateService : ApplicationService
    {
        /// <summary>
        /// Manages assets.
        /// </summary>
        private readonly IAssetManager _assets;

        /// <summary>
        /// Constructor.
        /// </summary>
        public AssetUpdateService(
            IBridge bridge,
            IMessageRouter messages,
            IAssetManager assets)
            : base(bridge, messages)
        {
            _assets = assets;
        }

        /// <inheritdoc cref="ApplicationService"/>
        public override void Start()
        {
            Subscribe<AssetListEvent>(MessageTypes.ASSET_LIST, Messages_OnAssetList);
            Subscribe<AssetAddEvent>(MessageTypes.ASSET_ADD, Messages_OnAssetAdd);
            Subscribe<AssetRemoveEvent>(MessageTypes.ASSET_REMOVE, Messages_OnAssetRemove);
            Subscribe<AssetUpdateEvent>(MessageTypes.ASSET_UPDATE, Messages_OnAssetUpdate);
        }

        /// <inheritdoc cref="ApplicationService"/>
        public override void Stop()
        {
            base.Stop();

            _assets.Uninitialize();
        }

        /// <summary>
        /// Called when an <c>Asset</c> has been updated.
        /// </summary>
        /// <param name="event">The event.</param>
        private void Messages_OnAssetUpdate(AssetUpdateEvent @event)
        {
            Log.Info(this, "Update asset.");
            
            _assets.Manifest.Update(@event.Asset);
        }

        /// <summary>
        /// Called when an <c>Asset</c> has been removed.
        /// </summary>
        /// <param name="event">The event.</param>
        private void Messages_OnAssetRemove(AssetRemoveEvent @event)
        {
            Log.Info(this, "Remove asset.");

            _assets.Manifest.Remove(@event.Id);
        }

        /// <summary>
        /// Called when an <c>Asset</c> has been added.
        /// </summary>
        /// <param name="event">The event.</param>
        private void Messages_OnAssetAdd(AssetAddEvent @event)
        {
            Log.Info(this, "Add asset.");

            var asset = @event.Asset;

            FormatAssetData(asset);

            _assets.Manifest.Add(asset);
        }

        /// <summary>
        /// Called when a complete manifest of assets has been sent over.
        /// </summary>
        /// <param name="event">The event.</param>
        private void Messages_OnAssetList(AssetListEvent @event)
        {
            Log.Info(this,
                "Updating AssetManifest with {0} assets.",
                @event.Assets.Length);

            // handle adds + updates
            var manifest = _assets.Manifest;
            var assets = @event.Assets;
            for (int i = 0, len = assets.Length; i < len; i++)
            {
                var asset = assets[i];

                FormatAssetData(asset);

                var info = manifest.Data(asset.Guid);
                if (null == info)
                {
                    _assets.Manifest.Add(asset);
                }
                else
                {
                    manifest.Update(asset);
                }
            }

            // handle removes
            var all = manifest.All;
            for (int i = 0, ilen = all.Length; i < ilen; i++)
            {
                var data = all[i];

                var found = false;
                for (int j = 0, jlen = assets.Length; j < jlen; j++)
                {
                    var asset = assets[i];
                    if (data.Guid == asset.Guid)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    manifest.Remove(data.Guid);
                }
            }
        }

        /// <summary>
        /// Makes some modifications to <c>AssetData</c> passed in.
        /// </summary>
        /// <param name="asset">The asset received.</param>
        private void FormatAssetData(AssetData asset)
        {
            // append build target to URI
            var index = asset.Uri.IndexOf(".bundle", StringComparison.Ordinal);
            asset.Uri = string.Format(
                "{0}_{1}.bundle",
                asset.Uri.Substring(0, index),
                GetBuildTarget());
        }

        /// <summary>
        /// Retrieves the build target we wish to download bundles for.
        /// </summary>
        /// <returns></returns>
        private string GetBuildTarget()
        {
            switch (UnityEngine.Application.platform)
            {
                case RuntimePlatform.WebGLPlayer:
                {
                    return "webgl";
                }
                case RuntimePlatform.WSAPlayerX86:
                case RuntimePlatform.WSAPlayerX64:
                case RuntimePlatform.WSAPlayerARM:
                {
                    return "wsaplayer";
                }
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.LinuxEditor:
                {
                    return "webgl";
                }
                case RuntimePlatform.IPhonePlayer:
                {
                    return "ios";
                }
                default:
                {
                    return "UNKNOWN";
                }
            }
        }
    }
}