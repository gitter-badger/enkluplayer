using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Presents menu options.
    /// </summary>
    public class MenuWidget : Widget
    {
        /// <summary>
        /// Configs.
        /// </summary>
        private readonly WidgetConfig _config;

        /// <summary>
        /// Creates elements.
        /// </summary>
        private readonly IElementFactory _elements;

        /// <summary>
        /// Creates primitives.
        /// </summary>
        private readonly IPrimitiveFactory _primitives;

        /// <summary>
        /// List of children to layout. This filters out menu, title, etc.
        /// </summary>
        private readonly List<Element> _filteredChildren = new List<Element>();

        /// <summary>
        /// Back button.
        /// </summary>
        private ButtonWidget _backButton;

        /// <summary>
        /// Properties.
        /// </summary>
        private ElementSchemaProp<string> _titleProp;
        private ElementSchemaProp<int> _titleFontSizeProp;
        private ElementSchemaProp<string> _descriptionProp;
        private ElementSchemaProp<int> _descriptionFontSizeProp;
        private ElementSchemaProp<int> _fontSizeProp;
        private ElementSchemaProp<string> _layoutProp;
        private ElementSchemaProp<float> _layoutRadiusProp;
        private ElementSchemaProp<float> _layoutDegreesProp;
        private ElementSchemaProp<int> _headerWidthProp;
        private ElementSchemaProp<float> _headerPaddingProp;
        private ElementSchemaProp<bool> _showBackButtonProp;
        private ElementSchemaProp<float> _dividerOffset;
        private ElementSchemaProp<bool> _dividerVisible;

        /// <summary>
        /// Title text.
        /// </summary>
        private TextPrimitive _titlePrimitive;

        /// <summary>
        /// Description text.
        /// </summary>
        private TextPrimitive _descriptionPrimitive;

        /// <summary>
        /// Half moon.
        /// </summary>
        private GameObject _halfMoon;

        /// <summary>
        /// Refers to children that have been part of the layout.
        /// </summary>
        public ReadOnlyCollection<Element> LayoutChildren { get; private set; }
        
        /// <summary>
        /// Called when the back button has been activated.
        /// </summary>
        public event Action<MenuWidget> OnBack;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MenuWidget(
            GameObject gameObject,
            WidgetConfig config,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
            IPrimitiveFactory primitives,
            IElementFactory elements)
            : base(
                  gameObject,
                  layers,
                  tweens,
                  colors)
        {
            _config = config;
            _primitives = primitives;
            _elements = elements;

            LayoutChildren = new ReadOnlyCollection<Element>(_filteredChildren);
        }

        /// <inheritdoc cref="Element"/>
        protected override void LoadInternalAfterChildren()
        {
            base.LoadInternalAfterChildren();

            // retrieve properties
            _titleProp = Schema.Get<string>("title");
            _titleProp.OnChanged += Title_OnChanged;
            
            _descriptionProp = Schema.Get<string>("description");
            _descriptionProp.OnChanged += Description_OnChanged;

            _fontSizeProp = Schema.Get<int>("fontSize");
            _fontSizeProp.OnChanged += FontSize_OnChanged;

            _layoutProp = Schema.Get<string>("layout");
            _layoutProp.OnChanged += Layout_OnChanged;

            _layoutDegreesProp = Schema.Get<float>("layout.degrees");
            _layoutDegreesProp.OnChanged += LayoutDegrees_OnChanged;

            _layoutRadiusProp = Schema.Get<float>("layout.radius");
            _layoutRadiusProp.OnChanged += LayoutRadius_OnChanged;

            _headerWidthProp = Schema.Get<int>("header.width");
            _headerWidthProp.OnChanged += HeaderWidth_OnChanged;

            _headerPaddingProp = Schema.Get<float>("header.padding");
            _headerPaddingProp.OnChanged += HeaderPadding_OnChange;

            _showBackButtonProp = Schema.Get<bool>("showBackButton");
            _showBackButtonProp.OnChanged += ShowBackButton_OnChanged;

            _dividerOffset = Schema.Get<float>("dividerOffset");
            _dividerOffset.OnChanged += DividerOffset_OnChanged;

            _dividerVisible = Schema.Get<bool>("divider.visible");
            _dividerVisible.OnChanged += DividerVisible_OnChanged;

            Schema.OnSelfPropAdded += Schema_OnPropAdded;

            // create + place title
            _titlePrimitive = _primitives.Text(Schema);
            AddChild(_titlePrimitive);
            _titlePrimitive.Text = _titleProp.Value;

            // create + place description
            _descriptionPrimitive = _primitives.Text(Schema);
            AddChild(_descriptionPrimitive);

            _descriptionPrimitive.Overflow = HorizontalWrapMode.Wrap;
            _descriptionPrimitive.Alignment = TextAlignmentType.TopLeft;
            _descriptionPrimitive.Text = _descriptionProp.Value;

            UpdateFontSizes();

            _halfMoon = Object.Instantiate(
                _config.HalfMoon.gameObject,
                Vector3.zero,
                Quaternion.identity);
            var transform = _halfMoon.GetComponent<RectTransform>();
            transform.SetParent(
                GameObject.transform,
                false);
            _halfMoon.SetActive(_dividerVisible.Value);

            UpdateHeaderLayout();
            UpdateChildLayout();

            OnChildAdded += This_OnChildAdded;

            _titlePrimitive.OnTextRectUpdated += Header_TextRectUpdated;
            _descriptionPrimitive.OnTextRectUpdated += Header_TextRectUpdated;
        }

        /// <inheritdoc cref="Element"/>
        protected override void UnloadInternalAfterChildren()
        {
            OnChildAdded -= This_OnChildAdded;

            _titleProp.OnChanged -= Title_OnChanged;
            _fontSizeProp.OnChanged -= FontSize_OnChanged;

            if (null != _titleFontSizeProp)
            {
                _titleFontSizeProp.OnChanged -= FontSize_OnChanged;
            }

            if (null != _descriptionFontSizeProp)
            {
                _descriptionFontSizeProp.OnChanged -= FontSize_OnChanged;
            }

            _descriptionProp.OnChanged -= Description_OnChanged;
            _layoutProp.OnChanged -= Layout_OnChanged;
            _layoutDegreesProp.OnChanged -= LayoutDegrees_OnChanged;
            _layoutRadiusProp.OnChanged -= LayoutRadius_OnChanged;
            _headerWidthProp.OnChanged -= HeaderWidth_OnChanged;
            _headerPaddingProp.OnChanged -= HeaderPadding_OnChange;
            _showBackButtonProp.OnChanged -= ShowBackButton_OnChanged;

            Object.Destroy(_halfMoon);
            _halfMoon = null;
            
            base.UnloadInternalAfterChildren();
        }

        /// <inheritdoc />
        protected override void AddChildInternal(Element element)
        {
            base.AddChildInternal(element);

            if (IsLoaded)
            {
                UpdateChildLayout();
            }
        }

        /// <inheritdoc />
        protected override void RemoveChildInternal(Element element)
        {
            base.AddChildInternal(element);

            if (IsLoaded)
            {
                UpdateChildLayout();
            }
        }

        /// <inheritdoc />
        protected override void DestroyInternal()
        {
            _titlePrimitive.Destroy();
            _descriptionPrimitive.Destroy();

            base.DestroyInternal();
        }

        /// <summary>
        /// Creates back button if need be.
        /// </summary>
        private void InitializeBackButton()
        {
            if (null != _backButton)
            {
                return;
            }

            _backButton = (ButtonWidget)_elements.Element(string.Format(
                "<?Vine><Button id='{0}' icon='arrow-left' ready.color='Negative' />",
                Id + ".btn-back"));
            _backButton.Activator.OnActivated += Back_OnActivated;
            AddChild(_backButton);
        }

        /// <summary>
        /// Adjusts children according to radial layout specs.
        /// </summary>
        /// <param name="children">The children.</param>
        /// <param name="worldRadius">The radius in world space.</param>
        /// <param name="degrees">The radius in degrees.</param>
        private void RadialLayout(IList<Element> children,
            float worldRadius,
            float degrees)
        {
            if (children.Count == 0)
            {
                return;
            }

            var localRadius = worldRadius;

            var baseTheta = children.Count > 1
                ? degrees * -0.5f
                : 0.0f;

            var stepTheta = children.Count > 1
                ? degrees / (children.Count - 1)
                : 0.0f;

            for (int i = 0, count = children.Count; i < count; ++i)
            {
                var child = children[i];
                if (child != null)
                {
                    var theta = baseTheta + stepTheta * i;
                    var thetaRadians = theta * Mathf.Deg2Rad;
                    var targetPosition = localRadius * new Vector3(
                        Mathf.Cos(thetaRadians),
                        -Mathf.Sin(thetaRadians),
                        0);

                    child.Schema.Set("position", targetPosition.ToVec());
                }
            }
        }

        /// <summary>
        /// Updates the font sizes across the board.
        /// </summary>
        private void UpdateFontSizes()
        {
            var defaultFontSize = _fontSizeProp.Value;

            // title
            var titleFontSize = defaultFontSize;
            if (null != _titleFontSizeProp)
            {
                titleFontSize = _titleFontSizeProp.Value;
            }
            else if (Schema.HasProp("title.fontSize"))
            {
                _titleFontSizeProp = Schema.Get<int>("title.fontSize");
                _titleFontSizeProp.OnChanged += FontSize_OnChanged;
                titleFontSize = _titleFontSizeProp.Value;
            }
            _titlePrimitive.FontSize = titleFontSize;

            // description
            var descriptionFontSize = defaultFontSize;
            if (null != _descriptionFontSizeProp)
            {
                descriptionFontSize = _descriptionFontSizeProp.Value;
            }
            else if (Schema.HasProp("description.fontSize"))
            {
                _descriptionFontSizeProp = Schema.Get<int>("description.fontSize");
                _descriptionFontSizeProp.OnChanged += FontSize_OnChanged;
                descriptionFontSize = _descriptionFontSizeProp.Value;
            }
            _descriptionPrimitive.FontSize = descriptionFontSize;
        }

        /// <summary>
        /// Updates the header layout.
        /// </summary>
        private void UpdateHeaderLayout()
        {
            var padding = -_headerPaddingProp.Value;

            _titlePrimitive.Width = _headerWidthProp.Value;
            _descriptionPrimitive.Width = _headerWidthProp.Value;

            _descriptionPrimitive.LocalPosition = new Vector3(
                padding,
                -_descriptionPrimitive.Height / 2f,
                0f);

            _titlePrimitive.LocalPosition = new Vector3(
                padding,
                _titlePrimitive.Height,
                0f);

            // back button
            if (_showBackButtonProp.Value)
            {
                InitializeBackButton();

                // place back button
                _backButton.Schema.Set("position", new Vec3(-0.2f, 0, 0));
            }
            else if (null != _backButton)
            {
                RemoveChild(_backButton);
                _backButton.Destroy();
                _backButton = null;
            }

            UpdateDivider();
        }

        /// <summary>
        /// Updates the divider.
        /// </summary>
        private void UpdateDivider()
        {
            _halfMoon
                .GetComponent<RectTransform>()
                .localPosition = new Vector3(
                _dividerOffset.Value,
                0f, 0f);
        }

        /// <summary>
        /// Updates child layout.
        /// </summary>
        private void UpdateChildLayout()
        {
            var layout = _layoutProp.Value;
            if (layout == "Radial")
            {
                _filteredChildren.Clear();
                for (int i = 0, len = Children.Count; i < len; i++)
                {
                    var child = Children[i];
                    if (child == _titlePrimitive
                        || child == _descriptionPrimitive
                        || child == _backButton)
                    {
                        continue;
                    }

                    _filteredChildren.Add(child);
                }

                RadialLayout(
                    _filteredChildren,
                    _layoutRadiusProp.Value,
                    _layoutDegreesProp.Value);
            }
        }

        /// <summary>
        /// Called when a child was added.
        /// </summary>
        /// <param name="parent">Parent.</param>
        /// <param name="child">Added element.</param>
        private void This_OnChildAdded(Element parent, Element child)
        {
            if (parent != this)
            {
                return;
            }

            UpdateChildLayout();
        }

        /// <summary>
        /// Called when a prop has been added to a schema.
        /// </summary>
        /// <param name="name">The name of the prop.</param>
        /// <param name="type">The type of the prop.</param>
        private void Schema_OnPropAdded(string name, Type type)
        {
            if (name.Contains("fontSize"))
            {
                UpdateFontSizes();
            }
        }

        /// <summary>
        /// Called when the title value has changed.
        /// </summary>
        /// <param name="prop">Title prop.</param>
        /// <param name="previous">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Title_OnChanged(
            ElementSchemaProp<string> prop,
            string previous,
            string next)
        {
            _titlePrimitive.Text = next;
        }

        /// <summary>
        /// Called when the description value has changed.
        /// </summary>
        /// <param name="prop">Description prop.</param>
        /// <param name="previous">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Description_OnChanged(
            ElementSchemaProp<string> prop,
            string previous,
            string next)
        {
            _descriptionPrimitive.Text = next;
        }

        /// <summary>
        /// Called when the fontSize value has changed.
        /// </summary>
        /// <param name="prop">FontSize prop.</param>
        /// <param name="previous">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void FontSize_OnChanged(
            ElementSchemaProp<int> prop,
            int previous,
            int next)
        {
            UpdateFontSizes();
        }

        /// <summary>
        /// Called when the layout value has changed.
        /// </summary>
        /// <param name="prop">Layou prop.</param>
        /// <param name="previous">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Layout_OnChanged(
            ElementSchemaProp<string> prop,
            string previous,
            string next)
        {
            UpdateChildLayout();
        }

        /// <summary>
        /// Called when the layoutDegrees value has changed.
        /// </summary>
        /// <param name="prop">layoutDegrees prop.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void LayoutDegrees_OnChanged(
            ElementSchemaProp<float> prop,
            float prev,
            float next)
        {
            UpdateChildLayout();
        }

        /// <summary>
        /// Called when the layoutRadius value has changed.
        /// </summary>
        /// <param name="prop">layoutRadius prop.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void LayoutRadius_OnChanged(
            ElementSchemaProp<float> prop,
            float prev,
            float next)
        {
            UpdateChildLayout();
        }

        /// <summary>
        /// Called when the headerWidth value has changed.
        /// </summary>
        /// <param name="prop">HeaderWidth prop.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void HeaderWidth_OnChanged(
            ElementSchemaProp<int> prop,
            int prev,
            int next)
        {
            UpdateHeaderLayout();
        }

        /// <summary>
        /// Called when the headerPadding value has changed.
        /// </summary>
        /// <param name="prop">HeaderPadding prop.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void HeaderPadding_OnChange(
            ElementSchemaProp<float> prop,
            float prev,
            float next)
        {
            UpdateHeaderLayout();
        }

        /// <summary>
        /// Called when the show back button property changes.
        /// </summary>
        /// <param name="prop">Prop in question.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void ShowBackButton_OnChanged(
            ElementSchemaProp<bool> prop,
            bool prev,
            bool next)
        {
            //
        }

        /// <summary>
        /// Called when the divider offset changes.
        /// </summary>
        /// <param name="prop">The property.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void DividerOffset_OnChanged(
            ElementSchemaProp<float> prop,
            float prev,
            float next)
        {
            UpdateDivider();
        }

        /// <summary>
        /// Called when the divider visibility changes.
        /// </summary>
        /// <param name="prop">The property.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void DividerVisible_OnChanged(
            ElementSchemaProp<bool> prop,
            bool prev,
            bool next)
        {
            if (null != _halfMoon)
            {
                _halfMoon.SetActive(next);
            }
        }

        /// <summary>
        /// Called when the textrect changes.
        /// </summary>
        /// <param name="textPrimitive">The primitive.</param>
        private void Header_TextRectUpdated(TextPrimitive textPrimitive)
        {
            UpdateHeaderLayout();
        }

        /// <summary>
        /// Called when the back button has been activated.
        /// </summary>
        /// <param name="activatorPrimitive">Activator.</param>
        private void Back_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            if (null != OnBack)
            {
                OnBack(this);
            }
        }
    }
}