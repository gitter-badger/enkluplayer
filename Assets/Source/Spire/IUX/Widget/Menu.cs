using System.Collections.Generic;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Presents menu options.
    /// </summary>
    public class Menu : Widget
    {
        /// <summary>
        /// Creates primitives.
        /// </summary>
        private readonly IPrimitiveFactory _primitives;

        /// <summary>
        /// List of children to layout. This filters out menu, title, etc.
        /// </summary>
        private readonly List<Element> _filteredChildren = new List<Element>();

        /// <summary>
        /// Properties.
        /// </summary>
        private ElementSchemaProp<string> _title;
        private ElementSchemaProp<string> _description;
        private ElementSchemaProp<int> _fontSize;
        private ElementSchemaProp<string> _layout;
        private ElementSchemaProp<float> _layoutRadius;
        private ElementSchemaProp<float> _layoutDegrees;
        private ElementSchemaProp<float> _headerWidth;

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
        /// Constructor.
        /// </summary>
        public Menu(
            WidgetConfig config,
            ILayerManager layers,
            ITweenConfig tweens,
            IColorConfig colors,
            IMessageRouter messages,
            IPrimitiveFactory primitives)
            : base(
                  new GameObject("Menu"),
                  config,
                  layers,
                  tweens,
                  colors,
                  messages)
        {
            _primitives = primitives;
        }

        /// <inheritdoc cref="Element"/>
        protected override void AfterLoadChildrenInternal()
        {
            base.AfterLoadChildrenInternal();

            // retrieve properties
            _title = Schema.Get<string>("title");
            _title.OnChanged += Title_OnChanged;

            _description = Schema.Get<string>("description");
            _description.OnChanged += Description_OnChanged;

            _fontSize = Schema.Get<int>("fontSize");
            _fontSize.OnChanged += FontSize_OnChanged;

            _layout = Schema.Get<string>("layout");
            _layout.OnChanged += Layout_OnChanged;

            _layoutDegrees = Schema.Get<float>("layout.degrees");
            _layoutDegrees.OnChanged += LayoutDegrees_OnChanged;

            _layoutRadius = Schema.Get<float>("layout.radius");
            _layoutRadius.OnChanged += LayoutRadius_OnChanged;

            _headerWidth = Schema.Get<float>("headerWidth");
            _headerWidth.OnChanged += HeaderWidth_OnChanged;

            // create + place title
            _titlePrimitive = _primitives.Text(Schema);
            AddChild(_titlePrimitive);
            _titlePrimitive.Text = _title.Value;
            _titlePrimitive.FontSize = _fontSize.Value;

            // create + place description
            _descriptionPrimitive = _primitives.Text(Schema);
            AddChild(_descriptionPrimitive);
            _descriptionPrimitive.Overflow = HorizontalWrapMode.Wrap;
            _descriptionPrimitive.Alignment = AlignmentTypes.TOP_LEFT;
            _descriptionPrimitive.Text = _description.Value;
            _descriptionPrimitive.FontSize = _fontSize.Value;

            _halfMoon = Object.Instantiate(
                Config.HalfMoon.gameObject,
                Vector3.zero,
                Quaternion.identity);
            var transform = _halfMoon.GetComponent<RectTransform>();
            transform.SetParent(
                GameObject.transform,
                false);
            transform.position = new Vector3(-0.54f, 0f, 0f);

            UpdateHeaderLayout();
            UpdateChildLayout();
        }

        /// <inheritdoc cref="Element"/>
        protected override void UnloadInternal()
        {
            _title.OnChanged -= Title_OnChanged;
            _fontSize.OnChanged -= FontSize_OnChanged;
            _description.OnChanged -= Description_OnChanged;
            _layout.OnChanged -= Layout_OnChanged;
            _layoutDegrees.OnChanged -= LayoutDegrees_OnChanged;
            _layoutRadius.OnChanged -= LayoutRadius_OnChanged;
            _headerWidth.OnChanged -= HeaderWidth_OnChanged;

            Object.Destroy(_halfMoon);
            _halfMoon = null;
            
            base.UnloadInternal();
        }

        /// <inheritdoc />
        protected override void DestroyInternal()
        {
            _titlePrimitive.Destroy();
            _descriptionPrimitive.Destroy();

            base.DestroyInternal();
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
            _titlePrimitive.FontSize = _descriptionPrimitive.FontSize = next;
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
            ElementSchemaProp<float> prop,
            float prev,
            float next)
        {
            UpdateHeaderLayout();
        }

        /// <summary>
        /// Updates the header layout.
        /// </summary>
        private void UpdateHeaderLayout()
        {
            _titlePrimitive.Width = _headerWidth.Value;
            _descriptionPrimitive.Width = _headerWidth.Value;
            
            _descriptionPrimitive.LocalPosition = new Vector3(
                0,
                // TODO: move to prop
                -_descriptionPrimitive.Height / 2f - 0.02f,
                0f);

            if (!string.IsNullOrEmpty(_description.Value))
            {
                _titlePrimitive.LocalPosition = new Vector3();
            }
            else
            {
                _titlePrimitive.LocalPosition = new Vector3(
                    0,
                    -_titlePrimitive.Rect.size.y / 2);
            }
        }

        /// <summary>
        /// Updates child layout.
        /// </summary>
        private void UpdateChildLayout()
        {
            var layout = _layout.Value;
            if (layout == "Radial")
            {
                var children = Children;
                _filteredChildren.Clear();
                for (int i = 0, len = children.Length; i < len; i++)
                {
                    var child = children[i];
                    if (child == _titlePrimitive || child == _descriptionPrimitive)
                    {
                        continue;
                    }

                    _filteredChildren.Add(child);
                }

                RadialLayout(
                    _filteredChildren,
                    _layoutRadius.Value,
                    _layoutDegrees.Value);
            }
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
    }
}