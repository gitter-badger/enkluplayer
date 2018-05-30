using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.Qr;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Widget that receives an event when a QR code is recognized.
    /// </summary>
    public class QrAnchorWidget : Widget
    {
        /// <summary>
        /// Qr service.
        /// </summary>
        private readonly IQrReaderService _qr;

        /// <summary>
        /// Manages user intention.
        /// </summary>
        private readonly IIntentionManager _intention;

        /// <summary>
        /// Manages elements.
        /// </summary>
        private readonly IElementManager _elements;

        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<string> _valueProp;
        private ElementSchemaProp<bool> _exclusiveProp;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public QrAnchorWidget(
            GameObject gameObject,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
            IQrReaderService qr,
            IIntentionManager intention,
            IElementManager elements)
            : base(gameObject, layers, tweens, colors)
        {
            _qr = qr;
            _intention = intention;
            _elements = elements;
        }
        
        /// <inheritdoc />
        protected override void LoadInternalAfterChildren()
        {
            base.LoadInternalAfterChildren();

            _valueProp = Schema.GetOwn("qr_value", "");
            _exclusiveProp = Schema.GetOwn("exclusive", true);

            _qr.OnRead += Qr_OnRead;
        }

        /// <inheritdoc />
        protected override void UnloadInternalBeforeChildren()
        {
            _qr.OnRead -= Qr_OnRead;

            base.UnloadInternalBeforeChildren();
        }
        
        /// <summary>
        /// Called when a QR code has been decoded.
        /// </summary>
        /// <param name="value">Value of the Qr code.</param>
        private void Qr_OnRead(string value)
        {
            if (_valueProp.Value == value)
            {
                Log.Info(this, "Matching QR value read! Toggling visibility.");

                // show!
                Show();

                if (_exclusiveProp.Value)
                {
                    HideOthers();
                }
            }
        }

        private void HideOthers()
        {
            
        }

        private bool _isShowing = false;

        /// <summary>
        /// Shows the anchor.
        /// </summary>
        private void Show()
        {
            if (_isShowing)
            {
                return;
            }

            _isShowing = true;

            // TODO: calculate intersection using AR service

            // for now, make something up
            var targetPosition = (_intention.Origin + 0.5f * _intention.Forward).ToVector();

            // set local position
            Schema.Set(
                "position",
                GameObject.transform.worldToLocalMatrix.MultiplyPoint3x4(targetPosition).ToVec());

            // make visible
            Schema.Set("visible", true);
        }

        /// <summary>
        /// Hides the anchor.
        /// </summary>
        private void Hide()
        {
            if (!_isShowing)
            {
                _isShowing = false;
            }

            _isShowing = true;

            Schema.Set("visible", false);
        }
    }
}