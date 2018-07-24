using System;
using System.Collections.Generic;
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
        private const int BUTTONS_PER_PAGE = NUM_COLS * NUM_ROWS;

        /// <summary>
        /// We secretly put the Option on the Button using this prop.
        /// </summary>
        private const string OPTION_PROPNAME = "__option__";

        /// <summary>
        /// Config.
        /// </summary>
        private readonly WidgetConfig _config;

        /// <summary>
        /// For creating elements.
        /// </summary>
        private readonly IElementFactory _elements;
        
        /// <summary>
        /// Handles events.
        /// </summary>
        private readonly IUXEventHandler _events;
        
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
        /// True during Populate.
        /// </summary>
        private bool _isPopulating;
        
        /// <summary>
        /// Called when a specific <c>Option</c> has been selected.
        /// </summary>
        public event Action<Option> OnSelected; 
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public GridWidget(
            GameObject gameObject,
            WidgetConfig config,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
            IElementFactory elements)
            : base(
                gameObject,
                layers,
                tweens,
                colors)
        {
            _config = config;
            _elements = elements;

            _events = gameObject.GetComponent<IUXEventHandler>()
                ?? gameObject.AddComponent<IUXEventHandler>();
        }

        /// <inheritdoc cref="IIUXEventDelegate"/>
        public bool OnEvent(IUXEvent @event)
        {
            var id = @event.Target.Id;
            if (id.StartsWith("grid.btn-"))
            {
                int index;
                if (int.TryParse(id.Split('-')[1], out index))
                {
                    index = index % BUTTONS_PER_PAGE;
                    var option = _buttons[index];
                    if (null != OnSelected)
                    {
                        OnSelected(option.Schema.Get<Option>(OPTION_PROPNAME).Value);
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Populates with all option groups. This is more optimized than
        /// multiple calls to AddChild.
        /// </summary>
        /// <param name="groups">The groups to populate with.</param>
        public void Populate(List<OptionGroup> groups)
        {
            _isPopulating = true;

            for (var i = 0; i < groups.Count; i++)
            {
                AddChild(groups[i]);
            }

            CreateGroupOptions();

            _isPopulating = false;
        }

        /// <inheritdoc />
        protected override void LoadInternalAfterChildren()
        {
            base.LoadInternalAfterChildren();

            _verticalPaddingProp = Schema.Get<float>("padding.vertical");
            _verticalPaddingProp.OnChanged += VerticalPadding_OnChanged;

            _horizontalPaddingProp = Schema.Get<float>("padding.horizontal");
            _horizontalPaddingProp.OnChanged += HorizontalPadding_OnChanged;
            
            // shell
            {
                _shell = Object.Instantiate(
                    _config.GridShell.gameObject,
                    Vector3.zero,
                    Quaternion.identity);
                var transform = _shell.GetComponent<RectTransform>();
                transform.SetParent(
                    GameObject.transform,
                    false);
            }

            // page select
            {
                _pageSelect = (SelectWidget) _elements.Element(@"<?Vine><Select position=(-0.241, -0.22, 0) />");
                _pageSelect.OnValueChanged += PageSelect_OnChanged;
                AddChild(_pageSelect);
            }

            // group select
            {
                _groupSelect = (SelectWidget) _elements.Element(@"<?Vine><Select position=(-0.241, 0.22, 0) />");
                _groupSelect.OnValueChanged += GroupSelect_OnChange;
                AddChild(_groupSelect);

                // generate select element with all options
                CreateGroupOptions();
            }

            _events.AddHandler(MessageTypes.BUTTON_ACTIVATE, this);
        }

        /// <inheritdoc />
        protected override void UnloadInternalAfterChildren()
        {
            base.UnloadInternalAfterChildren();

            _horizontalPaddingProp.OnChanged -= HorizontalPadding_OnChanged;
            _verticalPaddingProp.OnChanged -= VerticalPadding_OnChanged;
        }

        /// <inheritdoc />
        protected override void AddChildInternal(Element element)
        {
            base.AddChildInternal(element);

            if (_isPopulating)
            {
                return;
            }

            if (element is OptionGroup)
            {
                CreateGroupOptions();
            }
        }

        /// <inheritdoc />
        protected override void RemoveChildInternal(Element element)
        {
            base.RemoveChildInternal(element);

            if (element is OptionGroup)
            {
                CreateGroupOptions();
            }
        }

        /// <summary>
        /// Creates options for group selector.
        /// </summary>
        private void CreateGroupOptions()
        {
            // destroy old options
            var groupOptions = _groupSelect.Options;
            for (var i = groupOptions.Count - 1; i >= 0; i--)
            {
                groupOptions[i].Destroy();
            }

            var groups = new List<OptionGroup>();
            Find("(@type==OptionGroup)", groups);
            for (int i = 0, len = groups.Count; i < len; i++)
            {
                var group = groups[i];
                
                var option = (Option) _elements.Element(string.Format(
                    "<?Vine><Option label='{0}' value='{1}' />",
                    group.Label,
                    group.Value));

                _groupSelect.AddChild(option);
            }

            // recreate pages
            CreatePageOptions();

            // recreate buttons
            CreateButtons();
        }

        /// <summary>
        /// Creates options for page selector.
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
            var total = options.Count;
            var numPages = 0;
            while (total > 0)
            {
                numPages++;
                total -= BUTTONS_PER_PAGE;
            }

            for (var i = 0; i < numPages; i++)
            {
                var option = (Option) _elements.Element(string.Format(
                    @"<?Vine><Option value='{0}' label='{1}' />",
                    i,
                    string.Format(
                        "Page {0}/{1}",
                        i + 1,
                        numPages)));
                
                _pageSelect.AddChild(option);
            }
        }

        /// <summary>
        /// Recreates buttons for current page.
        /// </summary>
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
                i < Mathf.Min(options.Count, (page + 1) * resultsPerPage);
                i++)
            {
                var option = options[i];
                
                var button = (ButtonWidget) _elements.Element(string.Format(
                    @"<?Vine><Button
                    layout='vertical'
                    id='grid.btn-{0}'
                    label='{1}'
                    value='{2}'
                    src='{3}'
                    icon.scale=7.0
                    fontSize=50 />",
                    i,
                    option.Label,
                    option.Value,
                    option.Schema.Get<string>("src").Value));

                AddChild(button);

                button.Schema.Set(OPTION_PROPNAME, option);

                _buttons.Add(button);
            }

            // position children
            UpdateButtonLayout();
        }

        /// <summary>
        /// Updates the position of visible buttons.
        /// </summary>
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
            for (int i = 0, len = Children.Count; i < len; i++)
            {
                var group = Children[i] as OptionGroup;
                if (null != group && group.Value == value)
                {
                    return group;
                }
            }

            return null;
        }

        /// <summary>
        /// Called when the group select widget changes its selection.
        /// </summary>
        private void GroupSelect_OnChange(SelectWidget select)
        {
            CreatePageOptions();
            CreateButtons();
        }

        /// <summary>
        /// Called when the page select widget changes its selection.
        /// </summary>
        private void PageSelect_OnChanged(SelectWidget select)
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