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
        private ElementSchemaProp<Sprite> _spriteProp;
        private ElementSchemaProp<float> _widthProp;
        private ElementSchemaProp<float> _heightProp;

        /// <summary>
        /// Un-alpha'd color.
        /// </summary>
        private Color _color;

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
                _widthProp = Schema.GetOwn("width", 0f);
                _widthProp.OnChanged += Width_OnChanged;

                _heightProp = Schema.GetOwn("height", 0f);
                _heightProp.OnChanged += Height_OnChanged;

                _srcProp = Schema.Get<string>("src");
                _srcProp.OnChanged += Src_OnChanged;

                _spriteProp = Schema.Get<Sprite>("sprite");
                _spriteProp.OnChanged += Sprite_OnChanged;
            }

            _color = _image.color;

            UpdateSrc();
            UpdateAlpha();
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

        /// <inheritdoc />
        protected override void OnAlphaUpdated()
        {
            base.OnAlphaUpdated();

            UpdateAlpha();
        }

        /// <summary>
        /// Updates the image alpha.
        /// </summary>
        private void UpdateAlpha()
        {
            _image.color = new Color(
                _color.r,
                _color.g,
                _color.b,
                _color.a * Alpha);
        }

        /// <summary>
        /// Updates the image dimensions.
        /// </summary>
        private void UpdateDimensions()
        {
            var texture = null == _image.sprite ? null : _image.sprite.texture;
            var sourceWidth = null == texture ? 0 : texture.width;
            var sourceHeight = null == texture ? 0 : texture.height;

            var width = _widthProp.Value;
            var height = _heightProp.Value;

            if (width < Mathf.Epsilon)
            {
                if (height < Mathf.Epsilon)
                {
                    // both auto
                    width = sourceWidth;
                    height = sourceHeight;
                }
                else
                {
                    // auto based on height
                    width = (height / sourceHeight) * sourceWidth;
                }
            }
            else if (height < Mathf.Epsilon)
            {
                // auto based on width
                height = (width / sourceWidth) * sourceHeight;
            }

            _image.rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                width);
            _image.rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                height);
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

                    UpdateSprite(Sprite.Create(
                        _texture.Source,
                        Rect.MinMaxRect(0, 0, _texture.Source.width, _texture.Source.height),
                        new Vector2(0.5f, 0.5f)));
                })
                .OnFailure(exception => Log.Error(this,
                    "Could not load {0} : {1}.",
                    src,
                    exception));
        }

        /// <summary>
        /// Updates the images sprite.
        /// </summary>
        /// <param name="sprite">The sprite!</param>
        private void UpdateSprite(Sprite sprite)
        {
            _image.enabled = true;
            _image.sprite = sprite;

            UpdateDimensions();
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

        /// <summary>
        /// Called when the sprite property has changed.
        /// </summary>
        /// <param name="prop">Prop.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Sprite_OnChanged(
            ElementSchemaProp<Sprite> prop,
            Sprite prev,
            Sprite next)
        {
            UpdateSprite(next);
        }
    }
}