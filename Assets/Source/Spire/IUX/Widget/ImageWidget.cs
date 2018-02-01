using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;
using UnityEngine.UI;

namespace CreateAR.SpirePlayer.IUX
{
    public class ImageWidget : Widget
    {
        private readonly IImageLoader _loader;
        private readonly Image _image;

        private ElementSchemaProp<string> _srcProp;
        private ElementSchemaProp<float> _widthProp;
        private ElementSchemaProp<float> _heightProp;

        private IAsyncToken<Func<Texture2D, bool>> _loadToken;

        private Texture2D _texture;

        public ImageWidget(
            GameObject gameObject,
            WidgetConfig config,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
            IMessageRouter messages,
            IImageLoader loader)
            : base(
                gameObject,
                config,
                layers,
                tweens,
                colors,
                messages)
        {
            _loader = loader;
            _image = gameObject.GetComponentInChildren<Image>();
        }

        protected override void AfterLoadChildrenInternal()
        {
            base.AfterLoadChildrenInternal();

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

        protected override void AfterUnloadChildrenInternal()
        {
            base.AfterUnloadChildrenInternal();

            // props
            {
                _widthProp.OnChanged -= Width_OnChanged;
                _heightProp.OnChanged -= Height_OnChanged;
            }
        }

        private void UpdateDimensions()
        {
            _image.rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                _widthProp.Value);

            _image.rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                _heightProp.Value);
        }

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

            _image.enabled = false;

            _loadToken = _loader
                .Load(src)
                .OnSuccess(action =>
                {
                    // lazily create
                    if (null == _texture)
                    {
                        _texture = new Texture2D(2, 2);
                    }

                    // apply
                    if (!action(_texture))
                    {
                        Log.Error(this, "Could not load bytes into texture.");
                        return;
                    }

                    _image.enabled = true;
                    _image.sprite = Sprite.Create(
                        _texture,
                        Rect.MinMaxRect(0, 0, _texture.width, _texture.height),
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