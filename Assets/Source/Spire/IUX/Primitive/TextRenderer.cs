using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Controls rendering of text.
    /// </summary>
    public class TextRenderer : MonoBehaviour
    {
        /// <summary>
        /// Alignment types
        /// </summary>
        private int _alignment = AlignmentTypes.MID_LEFT;

        /// <summary>
        /// Unity text rendering.
        /// </summary>
        public Text Text;
        
        /// <summary>
        /// Alignment accessor/mutator.
        /// </summary>
        public int Alignment
        {
            get { return _alignment; }
            set
            {
                _alignment = value;
                RefreshAlignment();
            }
        }

        /// <summary>
        /// Fontsize accessor/mutator.
        /// </summary>
        public int FontSize
        {
            get { return Text.fontSize; }
            set { Text.fontSize = value; }
        }

        /// <summary>
        /// Font accessor/mutator
        /// </summary>
        public string Font
        {
            get { return Text.font.name; }
            set { Text.font = (Font) Resources.Load("Fonts/" + value); }
        }

        /// <summary>
        /// Refreshes the alignment of the text renderer
        /// </summary>
        private void RefreshAlignment()
        {
            switch (_alignment)
            {
                case AlignmentTypes.MID_CENTER:
                {
                    Text.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    Text.alignment = TextAnchor.MiddleCenter;
                    break;
                }

                case AlignmentTypes.TOP_LEFT:
                {
                    Text.rectTransform.pivot = new Vector2(0.0f, 0.0f);
                    Text.alignment = TextAnchor.MiddleCenter;
                    break;
                }

                case AlignmentTypes.TOP_CENTER:
                {
                    Text.rectTransform.pivot = new Vector2(0.5f, 0.0f);
                    Text.alignment = TextAnchor.MiddleCenter;
                    break;
                }

                case AlignmentTypes.TOP_RIGHT:
                {
                    Text.rectTransform.pivot = new Vector2(1.0f, 0.0f);
                    Text.alignment = TextAnchor.MiddleCenter;
                    break;
                }

                case AlignmentTypes.MID_RIGHT:
                {
                    Text.rectTransform.pivot = new Vector2(1.0f, 0.5f);
                    Text.alignment = TextAnchor.MiddleCenter;
                    break;
                }

                case AlignmentTypes.BOT_RIGHT:
                    Text.rectTransform.pivot = new Vector2(1.0f, 1.0f);
                    Text.alignment = TextAnchor.MiddleCenter;
                    break;

                case AlignmentTypes.BOT_CENTER:
                {
                    Text.rectTransform.pivot = new Vector2(0.5f, 1.0f);
                    Text.alignment = TextAnchor.MiddleCenter;
                    break;
                }

                case AlignmentTypes.BOT_LEFT:
                {
                    Text.rectTransform.pivot = new Vector2(0.0f, 1.0f);
                    Text.alignment = TextAnchor.MiddleCenter;
                    break;
                }

                case AlignmentTypes.MID_LEFT:
                {
                    Text.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    Text.alignment = TextAnchor.MiddleLeft;
                    break;
                }

                default:
                {
                    Log.Warning(this, "Unknown alignment type : {0}.", _alignment);
                    break;
                }
            }
        }
    }
}