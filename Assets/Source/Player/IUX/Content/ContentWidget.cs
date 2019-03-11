using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Widget that loads + holds an asset, behaviors, and vines.
    /// </summary>
    public class ContentWidget : Widget
    {
        /// <summary>
        /// Token for asset readiness.
        /// </summary>
        private readonly MutableAsyncToken<ContentWidget> _onAssetLoaded = new MutableAsyncToken<ContentWidget>();

        /// <summary>
        /// Assembles an asset.
        /// </summary>
        private readonly IAssetAssembler _assembler;

        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<string> _srcAssetProp;

        /// <summary>
        /// Set to true when we should poll to update the asset.
        /// </summary>
        private bool _pollUpdateAsset;

        /// <summary>
        /// Cached from the callback of the <see cref="IAssetAssembler"/>.
        /// </summary>
        public GameObject Asset { get; private set; }
        
        /// <summary>
        /// A token that is fired whenever the content has loaded.
        /// </summary>
        public IMutableAsyncToken<ContentWidget> OnLoaded
        {
            get { return _onAssetLoaded; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        [Construct]
        public ContentWidget(
            GameObject gameObject,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
            IAssetAssembler assembler)
            : base(
                gameObject,
                layers,
                tweens,
                colors)
        {
            _assembler = assembler;
        }

        /// <summary>
        /// Constructor used for testing.
        /// </summary>
        public ContentWidget(
            GameObject gameObject, 
            IAssetAssembler assembler)
            : base(gameObject, null, null, null)
        {
            _assembler = assembler;
        }

        /// <summary>
        /// Attempts to get <c>T</c> from the underlying asset's GameObject.
        /// Throws NullReferenceException if the assembly hasn't finished yet.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComponent<T>()
        {
            if (Asset)
            {
                return Asset.GetComponent<T>();
            }

            return default(T);
        }

        /// <summary>
        /// Attempts to get <c>T</c> from the underlying asset's GameObject & children.
        /// Throws NullReferenceException if the assembly hasn't finished yet.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComponentInChildren<T>()
        {
            if (Asset)
            {
                return Asset.GetComponentInChildren<T>();
            }

            return default(T);
        }

        /// <inheritdoc />
        protected override void DestroyInternal()
        {
            _assembler.OnAssemblyUpdated -= Assembler_OnAssemblyUpdated;

            base.DestroyInternal();
        }

        /// <inheritdoc />
        protected override void LoadInternalAfterChildren()
        {
            base.LoadInternalAfterChildren();

            _assembler.OnAssemblyUpdated += Assembler_OnAssemblyUpdated;

            _srcAssetProp = Schema.GetOwn("assetSrc", "");
            _srcAssetProp.OnChanged += AssetSrc_OnChanged;
            
            UpdateAsset();
        }

        /// <inheritdoc />
        protected override void UnloadInternalAfterChildren()
        {
            base.UnloadInternalAfterChildren();
            
            _srcAssetProp.OnChanged -= AssetSrc_OnChanged;

            _assembler.Teardown();
        }

        /// <inheritdoc />
        protected override void UpdateInternal()
        {
            base.UpdateInternal();

            if (_pollUpdateAsset)
            {
                UpdateAsset();
            }
        }

        /// <summary>
        /// Determines if this piece of content should load its assets.
        /// </summary>
        /// <returns></returns>
        private bool ShouldLoadAsset()
        {
            if (!Visible)
            {
                return false;
            }

            if (DeviceHelper.IsHoloLens())
            {
                return PrimaryAnchorManager.AreAllAnchorsReady;
            }

            return true;
        }

        /// <summary>
        /// Tears down the asset and sets it back up.
        /// </summary>
        private void UpdateAsset()
        {
            _pollUpdateAsset = false;

            if (!ShouldLoadAsset())
            {
                _pollUpdateAsset = true;
                return;
            }

            LogVerbose("Refresh asset for {0}.", Id);

            var assetId = _srcAssetProp.Value;
            var version = -1;

            var split = assetId.Split(':');
            if (2 == split.Length)
            {
                assetId = split[0];

                if (!int.TryParse(split[1], out version))
                {
                    Log.Warning(this, "Could not parse asset version : {0}.", assetId);
                }
            }

            _assembler.Teardown();
            _assembler.Setup(
                GameObject.transform,
                assetId,
                version);
        }

        /// <summary>
        /// Called when the assembler has completed seting up the asset.
        /// </summary>
        private void Assembler_OnAssemblyUpdated()
        {
            LogVerbose("Assembly complete.");

            Asset = _assembler.Assembly;

            if (Asset != null)
            {
                // parent + orient
                Asset.name = _srcAssetProp.Value;
                Asset.transform.SetParent(GameObject.transform, false);
                Asset.SetActive(true);

                // setup collider
                var bounds = _assembler.Bounds;
                var collider = EditCollider;
                if (null != collider) {
                    collider.center = bounds.center;
                    collider.size = bounds.size;
                }
            }
            
            // Not having an asset should still mark loading as complete
            _onAssetLoaded.Succeed(this);
        }

        /// <summary>
        /// Called when the asset src changes.
        /// </summary>
        /// <param name="prop">The prop.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void AssetSrc_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            UpdateAsset();
        }
    }
}