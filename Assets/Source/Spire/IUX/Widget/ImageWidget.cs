using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Widget for a static image.
    /// </summary>
    public class ImageWidget : Widget
    {
        /// <summary>
        /// Loads images.
        /// </summary>
        private readonly IImageLoader _loader;

        /// <summary>
        /// The image reference.
        /// </summary>
        private readonly Image _image;

        /// <summary>
        /// Props!
        /// </summary>
        private ElementSchemaProp<string> _srcProp;
        private ElementSchemaProp<float> _widthProp;
        private ElementSchemaProp<float> _heightProp;

        /// <summary>
        /// The token returned from the loader.
        /// </summary>
        private IAsyncToken<ManagedTexture> _loadToken;

        /// <summary>
        /// Managed texture we must release.
        /// </summary>
        private ManagedTexture _texture;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ImageWidget(
            GameObject gameObject,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
            IImageLoader loader)
            : base(
                gameObject,
                layers,
                tweens,
                colors)
        {
            _loader = loader;
            _image = gameObject.GetComponentInChildren<Image>();
        }

        /// <inheritdoc />
        protected override void LoadInternalAfterChildren()
        {
            base.LoadInternalAfterChildren();

            // props
            {
                _widthProp = Schema.Get<float>("width");
                _widthProp.OnChanged += Width_OnChanged;

                _heightProp = Schema.Get<float>("height");
                _heightProp.OnChanged += Height_OnChanged;

                _srcProp = Schema.Get<string>("src");
                _srcProp.OnChanged += Src_OnChanged;
            }

            UpdateSrc();
        }

        /// <inheritdoc />
        protected override void UnloadInternalAfterChildren()
        {
            base.UnloadInternalAfterChildren();

            if (null != _loadToken)
            {
                _loadToken.Abort();
            }

            if (null != _texture)
            {
                _texture.Release();
                _texture = null;
            }

            // props
            {
                _widthProp.OnChanged -= Width_OnChanged;
                _heightProp.OnChanged -= Height_OnChanged;
            }
        }

        /// <summary>
        /// Updates the image dimensions.
        /// </summary>
        private void UpdateDimensions()
        {
            _image.rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                _widthProp.Value);

            _image.rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                _heightProp.Value);
        }

        /// <summary>
        /// Updates the image src.
        /// </summary>
        private void UpdateSrc()
        {
            var src = _srcProp.Value;
            if (string.IsNullOrEmpty(src))
            {
                return;
            }

            if (null != _loadToken)
            {
                _loadToken.Abort();
            }

            if (null != _texture)
            {
                _texture.Release();
                _texture = null;

                _image.sprite = null;
            }

            _image.enabled = false;

            _loadToken = _loader
                .Load(src)
                .OnSuccess(texture =>
                {
                    _texture = texture;
                    
                    _image.enabled = true;
                    _image.sprite = Sprite.Create(
                        _texture.Source,
                        Rect.MinMaxRect(0, 0, _texture.Source.width, _texture.Source.height),
                        new Vector2(0.5f, 0.5f));

                    UpdateDimensions();
                })
                .OnFailure(exception => Log.Error(this,
                    "Could not load {0} : {1}.",
                    src,
                    exception));
        }

        /// <summary>
        /// Called when the desired width changes.
        /// </summary>
        /// <param name="prop">The prop.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Width_OnChanged(
            ElementSchemaProp<float> prop,
            float prev,
            float next)
        {
            UpdateDimensions();
        }

        /// <summary>
        /// Called when the desired height changes.
        /// </summary>
        /// <param name="prop">The prop.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Height_OnChanged(
            ElementSchemaProp<float> prop,
            float prev,
            float next)
        {
            UpdateDimensions();
        }

        /// <summary>
        /// Called when the src changes.
        /// </summary>
        /// <param name="prop">The prop.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Src_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            UpdateSrc();
        }
    }
}