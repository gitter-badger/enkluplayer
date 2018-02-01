using System.Collections.Generic;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.Vine;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Displays buttons in a grid format.
    /// </summary>
    public class GridWidget : Widget, IIUXEventDelegate
    {
        /// <summary>
        /// Enforced layout consts.
        /// </summary>
        private const int NUM_COLS = 3;
        private const int NUM_ROWS = 2;
        
        /// <summary>
        /// For creating elements.
        /// </summary>
        private readonly IElementFactory _elements;

        /// <summary>
        /// Parses vines.
        /// </summary>
        private readonly VineImporter _parser;

        /// <summary>
        /// Handles events.
        /// </summary>
        private readonly IUXEventHandler _events;

        /// <summary>
        /// List of groups.
        /// </summary>
        private readonly List<OptionGroup> _groups = new List<OptionGroup>();

        /// <summary>
        /// List of buttons currently presented.
        /// </summary>
        private readonly List<ButtonWidget> _buttons = new List<ButtonWidget>();

        /// <summary>
        /// The select widget for groups.
        /// </summary>
        private SelectWidget _groupSelect;

        /// <summary>
        /// The select widget for pages.
        /// </summary>
        private SelectWidget _pageSelect;
        
        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<float> _horizontalPaddingProp;
        private ElementSchemaProp<float> _verticalPaddingProp;

        /// <summary>
        /// Shell.
        /// </summary>
        private GameObject _shell;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public GridWidget(
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

            _events = gameObject.GetComponent<IUXEventHandler>()
                ?? gameObject.AddComponent<IUXEventHandler>();
        }

        /// <inheritdoc cref="IIUXEventDelegate"/>
        public bool OnEvent(IUXEvent @event)
        {
            return false;
        }

        /// <inheritdoc />
        protected override void AfterLoadChildrenInternal()
        {
            base.AfterLoadChildrenInternal();

            _verticalPaddingProp = Schema.Get<float>("padding.vertical");
            _verticalPaddingProp.OnChanged += VerticalPadding_OnChanged;

            _horizontalPaddingProp = Schema.Get<float>("padding.horizontal");
            _horizontalPaddingProp.OnChanged += HorizontalPadding_OnChanged;

            RefreshOptionGroups();

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
            
            // group select
            {
                // generate select element with all options
                var vine = @"<Select>";
                for (int i = 0, len = _groups.Count; i < len; i++)
                {
                    var group = _groups[i];

                    vine += string.Format("<Option label='{0}' value='{1}' />",
                        group.Label,
                        group.Value);
                }
                vine += @"</Select>";

                var groupSelectDescription = _parser.Parse(vine);

                // TODO: REMOVE HACK
                groupSelectDescription.Elements[0].Schema.Vectors["position"] = new Vec3(
                    -0.241f,
                    0.22f,
                    0f);

                _groupSelect = (SelectWidget) _elements.Element(groupSelectDescription);
                _groupSelect.OnChanged += GroupSelect_OnChange;
                AddChild(_groupSelect);
            }

            // page select
            {
                // generate page select
                var pageSelectDescription = _parser.Parse(@"<Select\>");

                // TODO: REMOVE HACK
                pageSelectDescription.Elements[0].Schema.Vectors["position"] = new Vec3(
                    -0.241f,
                    -0.22f,
                    0f);

                _pageSelect = (SelectWidget) _elements.Element(pageSelectDescription);
                _pageSelect.OnChanged += PageSelect_OnChanged;
                AddChild(_pageSelect);

                CreatePageOptions();
            }

            // update buttons for current selections
            {
                CreateButtons();
            }

            _events.AddHandler(MessageTypes.BUTTON_ACTIVATE, this);
        }

        /// <inheritdoc />
        protected override void AfterUnloadChildrenInternal()
        {
            base.AfterUnloadChildrenInternal();

            _horizontalPaddingProp.OnChanged -= HorizontalPadding_OnChanged;
            _verticalPaddingProp.OnChanged -= VerticalPadding_OnChanged;
        }

        /// <summary>
        /// Creates options for pafe selector.
        /// </summary>
        private void CreatePageOptions()
        {
            // destroy old options
            var pageOptions = _pageSelect.Options;
            for (var i = pageOptions.Count - 1; i >= 0; i--)
            {
                pageOptions[i].Destroy();
            }

            // create new options
            var group = CurrentGroup();
            if (null == group)
            {
                return;
            }

            var options = group.Options;
            var total = options.Length;
            var buttonsPerPage = NUM_COLS * NUM_ROWS;
            var numPages = 0;
            while (total > 0)
            {
                numPages++;
                total -= buttonsPerPage;
            }

            for (var i = 0; i < numPages; i++)
            {
                var option = (Option) _elements.Element(_parser.Parse(
                    string.Format(
                        @"<Option value='{0}' label='{1}' />",
                        i,
                        string.Format(
                            "Page {0}/{1}",
                            i + 1,
                            numPages))));
                
                _pageSelect.AddChild(option);
            }
        }

        private void CreateButtons()
        {
            var group = CurrentGroup();
            if (null == group)
            {
                return;
            }

            var page = CurrentPage();
            if (-1 == page)
            {
                return;
            }

            // clear old children
            for (int i = 0, len = _buttons.Count; i < len; i++)
            {
                _buttons[i].Destroy();
            }
            _buttons.Clear();

            // create new children
            var resultsPerPage = NUM_COLS * NUM_ROWS;
            var options = group.Options;
            for (var i = page * resultsPerPage;
                i < Mathf.Min(options.Length, (page + 1) * resultsPerPage);
                i++)
            {
                var option = options[i];
                var description = _parser.Parse(string.Format(
                    @"<Button
                        layout='vertical'
                        label='{0}'
                        value='{1}'
                        src='{2}'
                        fontSize=50 />",
                    option.Label,
                    option.Value,
                    option.Schema.Get<string>("src")));

                var button = (ButtonWidget) _elements.Element(description);

                AddChild(button);

                _buttons.Add(button);
            }

            // position children
            UpdateButtonLayout();
        }

        private void UpdateButtonLayout()
        {
            var elementHeight = _verticalPaddingProp.Value;
            var elementWidth = _horizontalPaddingProp.Value;
            var offset = new Vec3(-0.15f, 0.11f, 0f);

            for (var i = 0; i < _buttons.Count; i++)
            {
                var button = _buttons[i];

                // position
                var row = i / NUM_COLS;
                var col = i % NUM_COLS;
                
                var targetPosition = offset + new Vec3(
                    col * elementWidth,
                    -row * elementHeight,
                    0);
                button.Schema.Set("position", targetPosition);
            }
        }

        /// <summary>
        /// Grabs all child OptionGroups.
        /// </summary>
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

        /// <summary>
        /// Retrieves the currently selected <c>OptionGroup</c>.
        /// </summary>
        /// <returns></returns>
        private OptionGroup CurrentGroup()
        {
            var selection = _groupSelect.Selection;
            if (null == selection)
            {
                return null;
            }

            return Group(selection.Value);
        }

        /// <summary>
        /// Retrieves the current page.
        /// </summary>
        /// <returns></returns>
        private int CurrentPage()
        {
            var selection = _pageSelect.Selection;
            if (null == selection)
            {
                return -1;
            }

            return int.Parse(selection.Value);
        }

        /// <summary>
        /// Retrieves an <c>OptionGroup</c> by value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private OptionGroup Group(string value)
        {
            for (int i = 0, len = _groups.Count; i < len; i++)
            {
                var group = _groups[i];
                if (group.Value == value)
                {
                    return group;
                }
            }

            return null;
        }

        /// <summary>
        /// Called when the group select widget changes its selection.
        /// </summary>
        private void GroupSelect_OnChange()
        {
            CreatePageOptions();
            CreateButtons();
        }

        /// <summary>
        /// Called when the page select widget changes its selection.
        /// </summary>
        private void PageSelect_OnChanged()
        {
            CreateButtons();
        }

        /// <summary>
        /// Called when horizontal padding changes.
        /// </summary>
        /// <param name="prop">The prop.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value</param>
        private void HorizontalPadding_OnChanged(
            ElementSchemaProp<float> prop,
            float prev,
            float next)
        {
            UpdateButtonLayout();
        }

        /// <summary>
        /// Called when horizontal padding changes.
        /// </summary>
        /// <param name="prop">The prop.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value</param>
        private void VerticalPadding_OnChanged(
            ElementSchemaProp<float> prop,
            float prev,
            float next)
        {
            UpdateButtonLayout();
        }
    }
}