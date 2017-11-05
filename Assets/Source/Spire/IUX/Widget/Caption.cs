using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// TODO: Refactor with new Element Schema pipeline.
    /// </summary>
    public class TextSchema
    {
        /// <summary>
        /// Button data.
        /// </summary>
        public string Text;

        /// <summary>
        /// Size of the text's font.
        /// </summary>
        public int FontSize;

        /// <summary>
        /// Widget anchor position.
        /// </summary>
        public WidgetAnchorPosition AnchorPosition;
    }

    /// <summary>
    /// Displays a caption.
    /// </summary>
    public class Caption : Widget
    {
        /// <summary>
        /// Display text.
        /// </summary>
        public Text Text;

        /// <summary>
        /// Anchor position.
        /// </summary>
        public WidgetAnchor Anchor;

        /// <summary>
        /// Sets up the caption based upon the schema.
        /// </summary>
        /// <param name="schema"></param>
        public void SetSchema(TextSchema schema)
        {
            if (schema == null)
            {
                return;
            }

            if (Text != null)
            {
                Text.text = schema.Text;

                if (schema.FontSize != 0)
                {
                    Text.fontSize = schema.FontSize;
                }

                if (Anchor != null)
                {
                    Anchor.Refresh(schema.AnchorPosition);
                }

                Text.alignment = GetTextAnchor(schema.AnchorPosition);
                Text.rectTransform.pivot = GetPivot(schema.AnchorPosition);
            }

            LocalVisible = !string.IsNullOrEmpty(schema.Text);
        }

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
