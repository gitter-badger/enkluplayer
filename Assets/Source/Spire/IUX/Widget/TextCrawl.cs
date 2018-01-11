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
            /// Time at which the entry started.
            /// 
            /// TODO: A proprietary solution for time?
            /// </summary>
            public float StartTime;

            /// <summary>
            /// Color of the text rendering.
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
        /// Used for timing.
        /// </summary>
        private float _countdown = 1.0f;

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
            
            var textEntry = new TextEntry
            {
                TextPrimitive = textPrimitive,
                StartTime = Time.time,
                Color = textColor
            };

            _textEntries.Add(textEntry);
        }
        
        /// <inheritdoc />
        protected override void UpdateInternal()
        {
            base.UpdateInternal();

            if (_countdown > 0)
            {
                _countdown -= Time.time;
                if (_countdown <= 0)
                {
                    Add("Hello World!", new Col4(1,0,0,1));
                }
            }

            var keys = Config.CrawlFadeOutOffset.keys;
            var fadeOutDuration = keys[keys.Length - 1].time;

            // Color and position entries
            var time = Time.time;
            var offset = 0.0f;
            for (var i = _textEntries.Count; i > 0;)
            {
                var entry = _textEntries[--i];
                var elapsed = time - entry.StartTime;
                var localOffset = 0.0f;
                float alpha;

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
                text.LocalPosition = Vector3.up
                    * (offset + localOffset)
                    - text.Forward * localOffset * Config.CrawlFadeOutDepthScale;
                offset += Config.CrawlSeperation * scale;
            }
        }
    }
}