using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Text Element
    /// </summary>
    public class Caption : Widget
    {
        /// <summary>
        /// Handles rendering from unity's perspective
        /// </summary>
        private ITextPrimitive _primitive;

        /// <summary>
        /// Initialization
        /// </summary>
        protected override void LoadInternal()
        {
            base.LoadInternal();

            _primitive = Primitives.RequestText(GameObject.transform);
            _primitive.Text = Schema.Get<string>("text").Value;

            var fontSize = Schema.Get<int>("fontSize").Value;
            if (fontSize > 0)
            {
                _primitive.FontSize = fontSize;
            }
        }

        /// <summary>
        /// Shutdown
        /// </summary>
        protected override void UnloadInternal()
        {
            Primitives.Release(_primitive);
            _primitive = null;
        }
    }
}
