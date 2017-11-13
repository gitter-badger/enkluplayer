using UnityEngine;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Basic text rendering primitive.
    /// </summary>
    public class TextPrimitive : WidgetPrimitive, ITextPrimitive
    {
        /// <summary>
        /// Text Hookup
        /// </summary>
        public UnityEngine.UI.Text UIText;

        /// <summary>
        /// The visible text on the primitive.
        /// </summary>
        public string Text
        {
            get { return UIText.text; }
            set { UIText.text = value; }
        }

        /// <summary>
        /// The size of the font used on the primitive
        /// </summary>
        public int FontSize
        {
            get { return UIText.fontSize; }
            set { UIText.fontSize = value; }
        }

        /*
        /// <summary>
        /// Sets up the caption based upon the schema.
        /// </summary>
        public override void Load(ElementSchema schema)
        {
            if (schema == null)
            {
                return;
            }

            if (Text != null)
            {
                Text.text = schema.Get<string>("text").Value;

                var fontSize = schema.Get<int>("fontSize").Value;
                if (fontSize != 0)
                {
                    Text.fontSize = fontSize;
                }

                var anchorPosition = schema.Get<WidgetAnchorPosition>("anchorPosition").Value;
                Text.alignment = GetTextAnchor(anchorPosition);
                Text.rectTransform.pivot = GetPivot(anchorPosition);
            }
        }*/

        /// <summary>
        /// Translates from widgetAnchor to TextAnchor.
        /// </summary>
        /// <returns></returns>
        private static TextAnchor GetTextAnchor(WidgetAnchorPosition widgetAnchorPosition)
        {
            switch (widgetAnchorPosition)
            {
                case WidgetAnchorPosition.Bottom:
                    return TextAnchor.UpperCenter;
                case WidgetAnchorPosition.Top:
                    return TextAnchor.LowerCenter;
                case WidgetAnchorPosition.Right:
                    return TextAnchor.MiddleLeft;
                case WidgetAnchorPosition.Left:
                    return TextAnchor.MiddleRight;
            }

            return TextAnchor.MiddleCenter;
        }

        /// <summary>
        /// Get Caption Parent.
        /// </summary>
        /// <returns></returns>
        private static Vector2 GetPivot(WidgetAnchorPosition widgetAnchorPosition)
        {
            switch (widgetAnchorPosition)
            {
                case WidgetAnchorPosition.Bottom:
                    return new Vector2(0.5f, 1.0f);
                case WidgetAnchorPosition.Top:
                    return new Vector2(0.5f, 0.0f);
                case WidgetAnchorPosition.Right:
                    return new Vector2(0.0f, 0.5f);
                case WidgetAnchorPosition.Left:
                    return new Vector2(1.0f, 0.5f);
            }

            return new Vector2(0.5f, 0.5f);
        }
    }
}
