using System;
using System.Text;
using CreateAR.Commons.Unity.Logging;
using QRCoder;
using UnityEngine;
using Image = UnityEngine.UI.Image;

namespace CreateAR.EnkluPlayer.States.HoloLogin
{
    /// <summary>
    /// View for holocode.
    /// </summary>
    public class MobileHoloLoginUIView : MonoBehaviourUIElement
    {
        /// <summary>
        /// Backing variable for Code property.
        /// </summary>
        private string _holoCode;
        
        /// <summary>
        /// The image to put the QR code texture in.
        /// </summary>
        public Image Image;

        /// <summary>
        /// Called when the okay button has been pressed.
        /// </summary>
        public event Action OnOk;
        
        /// <summary>
        /// Gets and sets the holocode.
        /// </summary>
        public string Code
        {
            get
            {
                return _holoCode;
            }
            set
            {
                _holoCode = value;
                
                UpdateQrCode();
            }
        }

        // <inheritdoc />
        public override void Revealed()
        {
            base.Revealed();
            
            Log.Info(this, "Generating QR code.");

            UpdateQrCode();
        }

        /// <summary>
        /// Updates the QR code from the holologin.
        /// </summary>
        private void UpdateQrCode()
        {
            var generator = new QRCodeGenerator();
            var data = generator.CreateQrCode(
                Convert.ToBase64String(Encoding.UTF8.GetBytes((_holoCode ?? "None") + ":foo")),
                QRCodeGenerator.ECCLevel.M,
                true);
            var qrCode = new UnityQRCode(data);
            var tex = qrCode.GetGraphic(20);

            Image.sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f));
        }

        /// <summary>
        /// Called by the Unity UI system.
        /// </summary>
        public void OkClicked()
        {
            if (null != OnOk)
            {
                OnOk();
            }
        }
    }
}