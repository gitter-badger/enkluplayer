using System.Collections.Generic;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.Vine;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Displays options in a grid format.
    /// </summary>
    public class Grid : Widget
    {
        /// <summary>
        /// For creating primitives.
        /// </summary>
        private readonly IPrimitiveFactory _primitives;

        /// <summary>
        /// For creating elements.
        /// </summary>
        private readonly IElementFactory _elements;

        /// <summary>
        /// Parses vines.
        /// </summary>
        private readonly VineImporter _parser;

        private readonly List<OptionGroup> _groups = new List<OptionGroup>();

        private Select _groupSelect;

        private Select _pageSelect;

        private Button _backButton;

        /// <summary>
        /// Shell.
        /// </summary>
        private GameObject _shell;

        /// <summary>
        /// Constructor.
        /// </summary>
        public Grid(
            GameObject gameObject,
            WidgetConfig config,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
            IMessageRouter messages,
            IPrimitiveFactory primitives,
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
            _primitives = primitives;
            _elements = elements;
            _parser = parser;
        }

        protected override void AfterLoadChildrenInternal()
        {
            base.AfterLoadChildrenInternal();

            RefreshOptionGroups();

            // group select
            {
                // generate vine
                var vine = @"<?Vine><Select>";
                for (int i = 0, len = _groups.Count; i < len; i++)
                {
                    vine += "<Option label='{0}' value='{0}' />";
                }
                vine += "</Select>";

                var groupSelectDescription = _parser.Parse(vine);
                groupSelectDescription.Elements[0].Schema.Vectors["position"] = new Vec3(
                    -0.241f,
                    0.22f,
                    0f);

                _groupSelect = (Select) _elements.Element(groupSelectDescription);
                AddChild(_groupSelect);
            }

            // page select
            {
                // generate vine
                var vine = @"<?Vine><Select>";
                /*for (int i = 0, len = _groups.Count; i < len; i++)
                {
                    vine += "<Option label='{0}' value='{0}' />";
                }*/
                vine += "</Select>";

                var pageSelectDescription = _parser.Parse(vine);

                // TODO: REMOVE HACK
                pageSelectDescription.Elements[0].Schema.Vectors["position"] = new Vec3(
                    -0.241f,
                    -0.22f,
                    0f);

                _pageSelect = (Select) _elements.Element(pageSelectDescription);
                AddChild(_pageSelect);
            }

            // shell
            {
                _shell = Object.Instantiate(
                    Config.GridShell.gameObject,
                    Vector3.zero,
                    Quaternion.identity);
                var transform = _shell.GetComponent<RectTransform>();
                transform.SetParent(
                    GameObject.transform,
                    false);
            }

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
        }

        private void RefreshOptionGroups()
        {
            var children = Children;

            _groups.Clear();
            for (int i = 0, len = children.Length; i < len; i++)
            {
                var group = children[i] as OptionGroup;
                if (null != @group)
                {
                    _groups.Add(@group);
                }
            }
        }
    }
}