using System;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Alters ButtonWidget based on activator state.
    /// </summary>
    public class ButtonStateRenderer
    {
        /// <summary>
        /// Configuration.
        /// </summary>
        private readonly TweenConfig _tweens;
        private readonly ColorConfig _colors;
        
        /// <summary>
        /// The button to affect.
        /// </summary>
        private readonly ButtonWidget _button;

        /// <summary>
        /// Scale property.
        /// </summary>
        private ElementSchemaProp<Vec3> _scaleProp;

        private ElementSchemaProp<Vec3> _readyScaleProp;
        private ElementSchemaProp<Vec3> _activatingScaleProp;
        private ElementSchemaProp<Vec3> _activatedScaleProp;

        private ElementSchemaProp<string> _readyColorProp;
        private ElementSchemaProp<string> _activatingColorProp;
        private ElementSchemaProp<string> _activatedColorProp;

        private ElementSchemaProp<string> _readyTweenProp;
        private ElementSchemaProp<string> _activatingTweenProp;
        private ElementSchemaProp<string> _activatedTweenProp;

        private VirtualColor _color;
        private TweenType _tween;
        private float _tweenDuration;
        private VirtualColor _captionColor;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ButtonStateRenderer(
            TweenConfig tweens,
            ColorConfig colors,
            ButtonWidget button)
        {
            _tweens = tweens;
            _colors = colors;
            _button = button;
        }

        /// <summary>
        /// Called before first Update.
        /// </summary>
        public void Initialize()
        {
            _scaleProp = _button.Schema.GetOwn("scale", Vec3.One);

            _readyScaleProp = _button.Schema.Get<Vec3>("ready.scale");
            _activatingScaleProp = _button.Schema.Get<Vec3>("activating.scale");
            _activatedScaleProp = _button.Schema.Get<Vec3>("activated.scale");

            _readyColorProp = _button.Schema.Get<string>("ready.color");
            _activatingColorProp = _button.Schema.Get<string>("activating.color");
            _activatedColorProp = _button.Schema.Get<string>("activated.color");

            _readyTweenProp = _button.Schema.Get<string>("ready.tween");
            _activatingTweenProp = _button.Schema.Get<string>("activating.tween");
            _activatedTweenProp = _button.Schema.Get<string>("activated.tween");

            _button.Activator.OnStateChanged += Activator_OnStateChanged;
            UpdateProperties(_button.Activator.CurrentState);
        }

        /// <summary>
        /// Called when inactive.
        /// </summary>
        public void Uninitialize()
        {
            _button.Activator.OnStateChanged -= Activator_OnStateChanged;
        }
        
        /// <summary>
        /// Called every frame.
        /// </summary>
        /// <param name="dt">Delta time.</param>
        public void Update(float dt)
        {
            var isInteractable = _button.Interactable;

            var virtualColor = isInteractable
                ? _color
                : VirtualColor.Disabled;

            Col4 shellStateColor;
            _colors.TryGetColor(virtualColor, out shellStateColor);

            var tweenLerp = _tweenDuration > Mathf.Epsilon
                ? dt / _tweenDuration
                : 1.0f;

            _button.LocalColor = Col4.Lerp(
                _button.LocalColor,
                shellStateColor,
                tweenLerp);

            var defaultScale = _scaleProp.Value.ToVector();
            if (_button.GameObject)
            {
                _button.GameObject.transform.localScale = defaultScale;
            }
            /*_button.GameObject.transform.localScale = Vector3.Lerp(
                defaultScale,
                GetScale(defaultScale, state),
                tweenLerp);*/

            var captionVirtualColor = isInteractable
                ? _captionColor
                : VirtualColor.Disabled;

            Col4 captionColor;
            _colors.TryGetColor(captionVirtualColor, out captionColor);

            if (_button.GameObject)
            {
                _button.Text.LocalColor = Col4.Lerp(
                    _button.Text.LocalColor,
                    captionColor,
                    tweenLerp);
            }
        }
        
        /// <summary>
        /// Retrieves the color for the given state.
        /// </summary>
        /// <param name="state">State.</param>
        /// <returns></returns>
        private VirtualColor GetColor(ButtonState state)
        {
            switch (state)
            {
                case ButtonState.Activated:
                {
                    return ParseColor(_activatedColorProp.Value);
                }
                case ButtonState.Activating:
                {
                    return ParseColor(_activatingColorProp.Value);
                }
                default:
                {
                    return ParseColor(_readyColorProp.Value);
                }
            }
        }

        /// <summary>
        /// Retrieves the Tween value for the given state.
        /// </summary>
        /// <param name="state">sState.</param>
        /// <returns></returns>
        private TweenType GetTween(ButtonState state)
        {
            switch (state)
            {
                case ButtonState.Activated:
                {
                    return ParseTween(_activatedTweenProp.Value);
                }
                case ButtonState.Activating:
                {
                    return ParseTween(_activatingTweenProp.Value);
                }
                default:
                {
                    return ParseTween(_readyTweenProp.Value);
                }
            }
        }

        /// <summary>
        /// Retrieves the scale value for the given state.
        /// </summary>
        /// <param name="defaultScale">Starting scale.</param>
        /// <param name="state">State.</param>
        /// <returns></returns>
        private Vector3 GetScale(Vector3 defaultScale, ButtonState state)
        {
            Vec3 target;
            switch (state)
            {
                case ButtonState.Activated:
                {
                    target = _activatedScaleProp.Value;
                    break;
                }
                case ButtonState.Activating:
                {
                    target = _activatingScaleProp.Value;
                    break;
                }
                default:
                {
                    target = _readyScaleProp.Value;
                    break;
                }
            }

            return new Vector3(
                target.x / defaultScale.x,
                target.y / defaultScale.y,
                target.z / defaultScale.z);
        }

        /// <summary>
        /// Retrieves the caption color for the given state.
        /// </summary>
        /// <param name="state">State.</param>
        /// <returns></returns>
        private VirtualColor GetCaptionColor(ButtonState state)
        {
            var virtualColorString = _button
                .Schema
                .Get<string>(state.ToString().ToLowerInvariant() + ".captionColor")
                .Value;

            return ParseColor(virtualColorString);
        }

        /// <summary>
        /// Safely parses a string into a TweenType.
        /// </summary>
        /// <param name="tween">The tween.</param>
        /// <returns></returns>
        private TweenType ParseTween(string tween)
        {
            return EnumExtensions.Parse<TweenType>(tween);
        }

        /// <summary>
        /// Safely parses a string into a VirtualColor.
        /// </summary>
        /// <param name="color">The color string.</param>
        /// <returns></returns>
        private VirtualColor ParseColor(string color)
        {
            return EnumExtensions.Parse<VirtualColor>(color);
        }

        /// <summary>
        /// Called when the state changes.
        /// </summary>
        private void Activator_OnStateChanged(ActivatorState activatorState)
        {
            UpdateProperties(activatorState);
        }

        private void UpdateProperties(ActivatorState activatorState)
        {
            var buttonState = activatorState is ActivatorActivatedState
                ? ButtonState.Activated
                : activatorState is ActivatorActivatingState
                    ? ButtonState.Activating
                    : ButtonState.Ready;

            _color = GetColor(buttonState);
            _tween = GetTween(buttonState);
            _tweenDuration = _tweens.DurationSeconds(_tween);
            _captionColor = GetCaptionColor(buttonState);
        }
    }
}