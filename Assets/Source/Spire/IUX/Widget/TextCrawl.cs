using System.Collections.Generic;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine; 

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Presents text and then fades/scrolls out of sight.
    /// </summary>
    public class TextCrawl : Widget
    {
        /// <summary>
        /// Tracks spawned text entries.
        /// </summary>
        protected class TextEntry
        {
            /// <summary>
            /// Text primitive.
            /// </summary>
            public TextPrimitive TextPrimitive;

            /// <summary>
            /// TODO: A proprietary solution for time?
            /// </summary>
            public float StartTime;

            /// <summary>
            /// Color of the text rendering
            /// </summary>
            public Col4 Color;
        }

        /// <summary>
        /// Primitives!
        /// </summary>
        private readonly IPrimitiveFactory _primitives;

        /// <summary>
        /// List of active entries
        /// </summary>
        private readonly List<TextEntry> _textEntries = new List<TextEntry>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public TextCrawl(
            WidgetConfig config,
            ILayerManager layers,
            ITweenConfig tweens,
            IColorConfig colors,
            IMessageRouter messages,
            IPrimitiveFactory primitives)
            : base(
                new GameObject("TextCrawl"),
                config,
                layers,
                tweens,
                colors,
                messages)
        {
            _primitives = primitives;
        }

        /// <summary>
        /// Adds a new text element
        /// </summary>
        /// <param name="text"></param>
        /// <param name="textColor"></param>
        /// <returns></returns>
        public void Add(string text, Col4 textColor)
        {
            // create a text primitive.
            var textPrimitive = _primitives.Text(Schema);

            textPrimitive.Text = text;
            textPrimitive.Parent = this;
            
            var textEntry = new TextEntry()
            {
                TextPrimitive = textPrimitive,
                StartTime = Time.time,
                Color = textColor
            };

            _textEntries.Add(textEntry);
        }

        /// <inheritdoc cref="Element"/>
        protected override void LoadInternal()
        {
            base.LoadInternal();
        }

        /// <inheritdoc cref="Element"/>
        protected override void UnloadInternal()
        {
            base.UnloadInternal();
        }

        private float countdown = 1.0f;

        /// <summary>
        /// Frame Based Update
        /// </summary>
        protected override void UpdateInternal()
        {
            base.UpdateInternal();

            if (countdown > 0)
            {
                countdown -= Time.time;
                if (countdown <= 0)
                {
                    Add("Hello World!", new Col4(1,0,0,1));
                }
            }

            var fadeOutDuration = Config.CrawlFadeOutOffset.keys[Config.CrawlFadeOutOffset.keys.Length - 1].time;

            // Color and position entries
            var time = Time.time;
            var offset = 0.0f;
            for (int i = _textEntries.Count; i > 0;)
            {
                var entry = _textEntries[--i];
                var elapsed = time - entry.StartTime;
                var localOffset = 0.0f;
                var alpha = 0.0f;

                if (elapsed > Config.CrawlDuration)
                {
                    var fadeOutElapsed = elapsed - Config.CrawlDuration;
                    if (fadeOutElapsed > fadeOutDuration)
                    {
                        entry.TextPrimitive.Destroy();
                        _textEntries.RemoveAt(i);
                        continue;
                    }

                    localOffset = Config.CrawlFadeOutOffset.Evaluate(fadeOutElapsed);
                    alpha = Config.CrawlFadeOutAlpha.Evaluate(fadeOutElapsed);
                }
                else
                {
                    alpha = Config.CrawlFadeInAlpha.Evaluate(elapsed);
                }

                var lerp = elapsed;
                var text = entry.TextPrimitive;

                var color = entry.Color;
                color.a *= alpha;
                text.LocalColor = color;

                var scale = Config.CrawlScale.Evaluate(lerp);
                text.LocalScale = Vector3.one * scale;
                text.LocalPosition
                    = Vector3.up
                      * (offset + localOffset)
                      - text.Forward * localOffset * Config.CrawlFadeOutDepthScale;
                offset += Config.CrawlSeperation * scale;
            }
        }
    }
}
