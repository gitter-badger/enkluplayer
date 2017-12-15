using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    public class Menu : Widget
    {
        private readonly IPrimitiveFactory _primitives;

        private ElementSchemaProp<string> _title;
        private ElementSchemaProp<string> _description;
        private ElementSchemaProp<int> _fontSize;
        private ElementSchemaProp<string> _layout;
        private ElementSchemaProp<float> _layoutRadius;
        private ElementSchemaProp<float> _layoutDegrees;
        private ElementSchemaProp<float> _headerWidth;

        private TextPrimitive _titlePrimitive;
        private TextPrimitive _descriptionPrimitive;

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

        protected override void LoadInternal()
        {
            base.LoadInternal();

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
            _titlePrimitive = _primitives.Text();
            _titlePrimitive.Parent = this;
            _titlePrimitive.Text = _title.Value;
            _titlePrimitive.FontSize = _fontSize.Value;

            // create + place description
            _descriptionPrimitive = _primitives.Text();
            _descriptionPrimitive.Parent = this;
            _descriptionPrimitive.Text = _description.Value;
            _descriptionPrimitive.FontSize = _fontSize.Value;

            UpdateHeaderLayout();
            UpdateChildLayout();
        }

        protected override void UnloadInternal()
        {
            _title.OnChanged -= Title_OnChanged;
            _fontSize.OnChanged -= FontSize_OnChanged;
            _description.OnChanged -= Description_OnChanged;
            _layout.OnChanged -= Layout_OnChanged;
            _layoutDegrees.OnChanged -= LayoutDegrees_OnChanged;
            _layoutRadius.OnChanged -= LayoutRadius_OnChanged;
            _headerWidth.OnChanged -= HeaderWidth_OnChanged;

            _titlePrimitive.Destroy();
            _descriptionPrimitive.Destroy();

            base.UnloadInternal();
        }

        private void Title_OnChanged(
            ElementSchemaProp<string> prop,
            string previous,
            string next)
        {
            _titlePrimitive.Text = next;
        }

        private void Description_OnChanged(
            ElementSchemaProp<string> prop,
            string previous,
            string next)
        {
            _descriptionPrimitive.Text = next;
        }

        private void FontSize_OnChanged(
            ElementSchemaProp<int> prop,
            int previous,
            int next)
        {
            _titlePrimitive.FontSize = _descriptionPrimitive.FontSize = next;
        }

        private void Layout_OnChanged(
            ElementSchemaProp<string> prop,
            string previous,
            string next)
        {
            UpdateChildLayout();
        }

        private void LayoutDegrees_OnChanged(
            ElementSchemaProp<float> prop,
            float prev,
            float next)
        {
            UpdateChildLayout();
        }

        private void LayoutRadius_OnChanged(
            ElementSchemaProp<float> prop,
            float prev,
            float next)
        {
            UpdateChildLayout();
        }

        private void HeaderWidth_OnChanged(
            ElementSchemaProp<float> prop,
            float prev,
            float next)
        {
            UpdateHeaderLayout();
        }

        private void UpdateHeaderLayout()
        {
            _titlePrimitive.Width = _headerWidth.Value;
            _descriptionPrimitive.Width = _headerWidth.Value;

            var offset = new Vec2(
                -_headerWidth.Value + 150,
                _descriptionPrimitive.Height);

            _titlePrimitive.Position = offset + new Vec2(0, 100f);
            _descriptionPrimitive.Position = offset + new Vec2(0, 0f);
        }

        private void UpdateChildLayout()
        {
            var layout = _layout.Value;
            if (layout == "Radial")
            {
                RadialLayout(
                    GameObject.transform,
                    Children,
                    _layoutRadius.Value,
                    _layoutDegrees.Value);
            }
        }

        private void RadialLayout(
            Transform parent,
            IList<Element> children,
            float worldRadius,
            float degrees)
        {
            if (children.Count == 0)
            {
                return;
            }

            var localRadius = CalculateLocalOffset(parent, worldRadius);

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
        /// Calculates the local offset relative to a world transform
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="worldOffset"></param>
        /// <returns></returns>
        private float CalculateLocalOffset(Transform parent, float worldOffset)
        {
            var worldPosition = parent.position;
            var worldEdgePosition = worldPosition + Vector3.forward * worldOffset;
            var localEdgePosition = parent.InverseTransformPoint(worldEdgePosition);
            var localOffset = localEdgePosition.magnitude;

            return localOffset;
        }
    }
}