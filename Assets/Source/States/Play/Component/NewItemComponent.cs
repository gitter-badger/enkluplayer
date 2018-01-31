using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.IUX;
using CreateAR.SpirePlayer.Vine;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class NewItemComponent : Widget
    {
        /// <summary>
        /// For creating elements.
        /// </summary>
        private readonly IElementFactory _elements;

        /// <summary>
        /// Parses vines.
        /// </summary>
        private readonly VineImporter _parser;

        private GridWidget _grid;

        private Button _backButton;

        public NewItemComponent(
            GameObject gameObject,
            WidgetConfig config, 
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
            IMessageRouter messages,
            IElementFactory elements,
            VineImporter parser)
            : base(
                gameObject,
                config,
                layers,
                tweens,
                colors,
                messages)
        {
            _elements = elements;
            _parser = parser;
        }

        protected override void AfterLoadChildrenInternal()
        {
            base.AfterLoadChildrenInternal();

            // back button
            {
                var description = _parser.Parse(string.Format(
                    @"<?Vine><Button id='{0}.btn-back' icon='cancel' />",
                    Id));

                // TODO: REMOVE HACK
                description.Elements[0].Schema.Vectors["position"] = new Vec3(-0.35f, 0, 0);

                _backButton = (Button) _elements.Element(description);
                AddChild(_backButton);
            }

            // grid
            {
                _grid = (GridWidget) _elements.Element(
                    _parser.Parse(@"<Grid fontSize=20 />"));
                AddChild(_grid);
            }
        }
    }
}