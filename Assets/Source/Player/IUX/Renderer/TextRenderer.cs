using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Controls rendering of text.
    /// </summary>
    public class TextRenderer : MonoBehaviour
    {
        /// <summary>
        /// Alignment.
        /// </summary>
        private TextAlignmentType _alignment;

        /// <summary>
        /// Unity text rendering.
        /// </summary>
        public Text Text;
        
        /// <summary>
        /// Alignment accessor/mutator.
        /// </summary>
        public TextAlignmentType Alignment
        {
            get
            {
                return _alignment;
            }
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
                case TextAlignmentType.MidCenter:
                {
                    Text.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    Text.alignment = TextAnchor.MiddleCenter;
                    break;
                }

                case TextAlignmentType.TopLeft:
                {
                    Text.rectTransform.pivot = new Vector2(0f, 0f);
                    Text.alignment = TextAnchor.UpperLeft;
                    break;
                }

                case TextAlignmentType.TopCenter:
                {
                    Text.rectTransform.pivot = new Vector2(0.5f, 0.0f);
                    Text.alignment = TextAnchor.UpperCenter;
                    break;
                }

                case TextAlignmentType.TopRight:
                {
                    Text.rectTransform.pivot = new Vector2(1.0f, 0.0f);
                    Text.alignment = TextAnchor.MiddleCenter;
                    break;
                }

                case TextAlignmentType.MidRight:
                {
                    Text.rectTransform.pivot = new Vector2(1.0f, 0.5f);
                    Text.alignment = TextAnchor.MiddleCenter;
                    break;
                }

                case TextAlignmentType.BotRight:
                    Text.rectTransform.pivot = new Vector2(1.0f, 1.0f);
                    Text.alignment = TextAnchor.MiddleCenter;
                    break;

                case TextAlignmentType.BotCenter:
                {
                    Text.rectTransform.pivot = new Vector2(0.5f, 1.0f);
                    Text.alignment = TextAnchor.MiddleCenter;
                    break;
                }

                case TextAlignmentType.BotLeft:
                {
                    Text.rectTransform.pivot = new Vector2(0.0f, 1.0f);
                    Text.alignment = TextAnchor.MiddleCenter;
                    break;
                }

                case TextAlignmentType.MidLeft:
                {
                    Text.rectTransform.pivot = new Vector2(0f, 0.5f);
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