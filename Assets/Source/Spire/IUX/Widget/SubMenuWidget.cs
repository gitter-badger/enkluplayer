using System.Linq;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    public class SubMenuWidget : Widget
    {
        private readonly IElementFactory _elements;
        private readonly SubMenuWidgetLineRenderer _renderer;

        private MenuWidget _menu;
        private ButtonWidget _button;
        private ElementSchemaProp<string> _labelProp;
        private ElementSchemaProp<string> _iconProp;

        public SubMenuWidget(
            GameObject gameObject,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
            IElementFactory elements)
            : base(gameObject, layers, tweens, colors)
        {
            _elements = elements;
            _renderer = GameObject.AddComponent<SubMenuWidgetLineRenderer>();
        }
        
        /// <inheritdoc />
        protected override void LoadInternalAfterChildren()
        {
            base.LoadInternalAfterChildren();

            // get props
            {
                _labelProp = Schema.GetOwn("label", "");
                _labelProp.OnChanged += Label_OnChanged;

                _iconProp = Schema.GetOwn("icon", "");
                _iconProp.OnChanged += Icon_OnChanged;
            }

            var buttonId = string.Format("__{0}_btn", Id);
            var menuId = string.Format("__{0}_menu", Id);

            // button
            {
                _button = (ButtonWidget) _elements.Element(string.Format(
                    "<Button id='{0}' label='{1}' icon='{2}' />",
                    buttonId,
                    _labelProp.Value,
                    _iconProp.Value));
                _button.Activator.OnActivated += Button_OnActivated;
                AddChild(_button);
            }

            // menu
            {
                _menu = (MenuWidget) _elements.Element(string.Format(
                    "<Menu id='{0}' position=(-0.4, 0, 0) divider.visible=false visible=false focus.visible=false />",
                    menuId));
                AddChild(_menu);
            }

            // get child buttons and move them to the menu
            {
                var childrenCopy= Children.ToArray();
                for (var i = 0; i < childrenCopy.Length; i++)
                {
                    var child = childrenCopy[i];
                    if (child.Id != buttonId && child.Id != menuId)
                    {
                        _menu.AddChild(child);
                    }
                }
            }

            _renderer.Initialize(_menu);
        }

        /// <inheritdoc />
        protected override void UnloadInternalAfterChildren()
        {
            base.UnloadInternalAfterChildren();

            _renderer.Uninitialize();
        }

        /// <summary>
        /// Updates the button state based on menu visibility.
        /// </summary>
        private void UpdateButtonState()
        {
            var isVisible = _menu.Schema.Get<bool>("visible").Value;
            if (isVisible)
            {
                _button.Schema.Set("label", "");
                _button.Schema.Set("icon", "arrow-left");
                _button.Schema.Set("ready.color", "Negative");
            }
            else
            {
                _button.Schema.Set("label", _labelProp.Value);
                _button.Schema.Set("icon", _iconProp.Value);
                _button.Schema.Set("ready.color", "Ready");
            }
        }

        /// <summary>
        /// Called when label changes.
        /// </summary>
        private void Label_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            UpdateButtonState();
        }

        /// <summary>
        /// Called when icon changes.
        /// </summary>
        private void Icon_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            UpdateButtonState();
        }

        /// <summary>
        /// Called when button is activated.
        /// </summary>
        /// <param name="activatorPrimitive">The activator.</param>
        private void Button_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            // flip visibility of menu
            var isVisible = !_menu.Schema.Get<bool>("visible").Value;
            _menu.Schema.Set("visible", isVisible);

            UpdateButtonState();
        }
    }
}