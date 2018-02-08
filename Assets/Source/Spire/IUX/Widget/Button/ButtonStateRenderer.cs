﻿using System;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Alters <b>Button</b> based on activator state.
    /// </summary>
    public class ButtonStateRenderer
    {
        /// <summary>
        /// Configuration.
        /// </summary>
        private readonly TweenConfig _tweens;
        private readonly ColorConfig _colors;
        public readonly WidgetConfig _config;
        
        /// <summary>
        /// The button to affect.
        /// </summary>
        private readonly ButtonWidget _button;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ButtonStateRenderer(
            TweenConfig tweens,
            ColorConfig colors,
            WidgetConfig config,
            ButtonWidget button)
        {
            _tweens = tweens;
            _colors = colors;
            _config = config;
            _button = button;
        }
        
        /// <summary>
        /// Called every frame.
        /// </summary>
        /// <param name="dt">Delta time.</param>
        public void Update(float dt)
        {
            var state = GetCurrentButtonState();
            var isInteractable = _button.Interactable;

            var virtualColor = isInteractable
                ? GetColor(state)
                : VirtualColor.Disabled;

            Col4 shellStateColor;
            _colors.TryGetColor(virtualColor, out shellStateColor);

            var tweenDuration = _tweens.DurationSeconds(GetTween(state));
            var tweenLerp = tweenDuration > Mathf.Epsilon
                ? dt / tweenDuration
                : 1.0f;

            _button.LocalColor = Col4.Lerp(
                _button.LocalColor,
                shellStateColor,
                tweenLerp);

            _button.GameObject.transform.localScale = Vector3.Lerp(
                _button.GameObject.transform.localScale,
                GetScale(state),
                tweenLerp);

            var captionVirtualColor = isInteractable
                ? GetCaptionColor(state)
                : VirtualColor.Disabled;

            Col4 captionColor;
            _colors.TryGetColor(captionVirtualColor, out captionColor);

            _button.Text.LocalColor = Col4.Lerp(
                _button.Text.LocalColor,
                captionColor,
                tweenLerp);
        }

        private VirtualColor GetColor(ButtonState state)
        {
            var virtualColorString = _button
                .Schema
                .Get<string>(state.ToString().ToLowerInvariant() + ".color")
                .Value;

            return ParseColor(virtualColorString);
        }

        private TweenType GetTween(ButtonState state)
        {
            var tweenTypeString = _button
                .Schema
                .Get<string>(state.ToString().ToLowerInvariant())
                .Value;

            return ParseTween(tweenTypeString);
        }

        private Vector3 GetScale(ButtonState state)
        {
            return _button
                .Schema
                .Get<Vec3>(state.ToString().ToLowerInvariant() + ".scale")
                .Value
                .ToVector();
        }

        private VirtualColor GetCaptionColor(ButtonState state)
        {
            var virtualColorString = _button
                .Schema
                .Get<string>(state.ToString().ToLowerInvariant() + ".captionColor")
                .Value;

            return ParseColor(virtualColorString);
        }

        private TweenType ParseTween(string tween)
        {
            try
            {
                return (TweenType) Enum.Parse(
                    typeof(TweenType),
                    tween);
            }
            catch
            {
                return TweenType.Responsive;
            }
        }

        private VirtualColor ParseColor(string color)
        {
            try
            {
                return (VirtualColor) Enum.Parse(
                    typeof(VirtualColor),
                    color);
            }
            catch
            {
                return VirtualColor.Disabled;
            }
        }

        /// <summary>
        /// Determines the current button state using the activator.
        /// </summary>
        /// <returns></returns>
        private ButtonState GetCurrentButtonState()
        {
            var activator = _button.Activator;
            if (activator.CurrentState is ActivatorActivatingState)
            {
                return ButtonState.Activating;
            }

            if (activator.CurrentState is ActivatorActivatedState)
            {
                return ButtonState.Activated;
            }

            return ButtonState.Ready;
        }
    }
}