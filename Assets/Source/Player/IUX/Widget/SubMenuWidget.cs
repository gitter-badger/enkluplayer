using System.Linq;
using UnityEngine;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// A button and a menu that is toggled with the button.
    /// </summary>
    public class SubMenuWidget : Widget
    {
        /// <summary>
        /// Creates elements.
        /// </summary>
        private readonly IElementFactory _elements;

        /// <summary>
        /// Renders lines to submenu.
        /// </summary>
        private readonly SubMenuWidgetLineRenderer _renderer;

        /// <summary>
        /// The menu.
        /// </summary>
        private MenuWidget _menu;

        /// <summary>
        /// The toggle button.
        /// </summary>
        private ButtonWidget _button;
        
        /// <summary>
        /// The label property, passed on to the button.
        /// </summary>
        private ElementSchemaProp<string> _labelProp;

        /// <summary>
        /// The icon property, passed on to the button.
        /// </summary>
        private ElementSchemaProp<string> _iconProp;

        /// <summary>
        /// Constructor.
        /// </summary>
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

        /// <summary>
        /// Opens the submenu.
        /// </summary>
        public void Open()
        {
            // hide any other menus at the same level
            if (null != Parent)
            {
                var siblings = Parent.Children;
                for (var i = 0; i < siblings.Count; i++)
                {
                    var submenu = siblings[i] as SubMenuWidget;
                    if (null != submenu && submenu != this)
                    {
                        submenu.Close();
                    }
                }
            }

            // show yourself
            _menu.Schema.Set("visible", true);

            UpdateButtonState();
        }

        /// <summary>
        /// Closes the submenu.
        /// </summary>
        public void Close()
        {
            _menu.Schema.Set("visible", false);

            UpdateButtonState();
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
        /// Toggles the submenu.
        /// </summary>
        private void Toggle()
        {
            var isVisible = _menu.Schema.Get<bool>("visible").Value;
            if (isVisible)
            {
                Close();
            }
            else
            {
                Open();
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
            Toggle();
        }
    }
}