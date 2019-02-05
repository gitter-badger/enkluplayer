using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.Qr;
using UnityEngine;

namespace CreateAR.EnkluPlayer.IUX
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
        /// True iff the QR anchor is currently showing. This is used to track Show(), Hide(),
        /// and does not necessarily reflect visibility.
        /// </summary>
        private bool _isShowing;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public QrAnchorWidget(
            GameObject gameObject,
            ILayerManager layers,
            ITweenConfig tweens,
            IColorConfig colors,
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

            // selection collider
            {
                var collider = EditCollider;
                if (null != collider)
                {
                    collider.center = Vector3.zero;
                    collider.size = 0.5f * Vector3.one;
                }
            }
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
                Log.Info(this, "Matching QR value read!");

                if (!_isShowing)
                {
                    // show!
                    Show();

                    if (_exclusiveProp.Value)
                    {
                        HideOthers();
                    }   
                }
            }
        }

        /// <summary>
        /// Hides other QRAnchorWidgets.
        /// </summary>
        private void HideOthers()
        {
            var all = _elements.All;
            for (var i = 0; i < all.Count; i++)
            {
                var element = all[i] as QrAnchorWidget;
                if (null != element && element != this)
                {
                    element.Hide();
                }
            }
        }

        /// <summary>
        /// Shows the anchor.
        /// </summary>
        private void Show()
        {
            Log.Info(this, "{0}::Show()", Id);
            if (_isShowing)
            {
                Log.Info(this, "{0} is already showing.", Id);
                return;
            }

            _isShowing = true;

            // TODO: calculate intersection using AR service

            // for now, make something up
            var targetPosition = (_intention.Origin + 0.5f * _intention.Forward).ToVector();

            // set local position
            /*Schema.Set(
                "position",
                GameObject.transform.worldToLocalMatrix.MultiplyPoint3x4(targetPosition).ToVec());*/
            GameObject.transform.position = targetPosition;

            // make visible
            Schema.Set("visible", true);
        }

        /// <summary>
        /// Hides the anchor.
        /// </summary>
        private void Hide()
        {
            Log.Info(this, "{0}::Hide()", Id);
            if (!_isShowing)
            {
                Log.Info(this, "{0} is already hiding.", Id);
                return;
            }

            _isShowing = false;

            Schema.Set("visible", false);
        }
    }
}