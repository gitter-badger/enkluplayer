using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// View controller for scanning QR codes.
    /// </summary>
    [InjectVine("Qr.Scanning")]
    public class QrViewController : InjectableIUXController
    {
        [InjectElements("..img-qr")]
        public ImageWidget Qr { get; set; }
        
        [InjectElements("..caption-progress")]
        public CaptionWidget Progress { get; set; }

        public void ShowMessage(string message)
        {
            Qr.Schema.Set("visible", false);

            Progress.Schema.Set("label", message);
            Progress.Schema.Set("visible", true);
        }
    }
}
