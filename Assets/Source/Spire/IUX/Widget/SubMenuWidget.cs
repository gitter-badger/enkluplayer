using System.Collections.Generic;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    public class SubMenuWidget : Widget
    {
        private readonly IElementFactory _elements;

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
        }
        
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
                    "<Menu id='{0}' visible=false focus.visible=false />",
                    menuId));
                AddChild(_menu);
            }

            // get child buttons and move them to the menu
            {
                var buttons = new List<ButtonWidget>();
                Find(".(@type==Button)", buttons);

                for (var i = 0; i < buttons.Count; i++)
                {
                    var button = buttons[i];
                    if (button.Id != buttonId)
                    {
                        _menu.AddChild(button);
                    }
                }
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
            _button.Schema.Set("label", next);
        }

        /// <summary>
        /// Called when icon changes.
        /// </summary>
        private void Icon_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            _button.Schema.Set("icon", next);
        }

        private void Button_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            // flip visibility of menu
            _menu.Schema.Set(
                "visible",
                !_menu.Schema.Get<bool>("visible").Value);
        }
    }
}