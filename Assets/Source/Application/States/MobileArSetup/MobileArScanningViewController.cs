using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controls the view for the scanning prompt.
    /// </summary>
    public class MobileArScanningViewController : MonoBehaviourUIElement
    {
        /// <summary>
        /// MS between scan text deltas.
        /// </summary>
        private const float DELTA_SEC = 0.5f;

        /// <summary>
        /// Texts.
        /// </summary>
        private readonly string[] _texts = new[]
        {
            "Scanning",
            "Scanning.",
            "Scanning..",
            "Scanning...",
        };

        /// <summary>
        /// Index into _texts;
        /// </summary>
        private int _index;
        
        /// <summary>
        /// Link to text.
        /// </summary>
        public Text ScanText;

        /// <summary>
        /// Called when the behaviour is enabled.
        /// </summary>
        private void OnEnable()
        {
            StartCoroutine(UpdateText());
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        private IEnumerator UpdateText()
        {
            while (enabled)
            {
                ScanText.text = _texts[_index % _texts.Length];
                
                yield return new WaitForSecondsRealtime(DELTA_SEC);

                _index++;
            }
        }
    }
}