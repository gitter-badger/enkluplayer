using System;
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
        private readonly Button _button;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ButtonStateRenderer(
            TweenConfig tweens,
            ColorConfig colors,
            WidgetConfig config,
            Button button)
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
            var config = Config(GetCurrentButtonState());
            var isInteractable = _button.Interactable;

            var virtualColor = isInteractable
                ? config.Color
                : VirtualColor.Disabled;

            Col4 shellStateColor;
            _colors.TryGetColor(virtualColor, out shellStateColor);

            var tweenDuration = _tweens.DurationSeconds(config.Tween);
            var tweenLerp = tweenDuration > Mathf.Epsilon
                ? dt / tweenDuration
                : 1.0f;

            _button.LocalColor = Col4.Lerp(
                _button.LocalColor,
                shellStateColor,
                tweenLerp);

            _button.GameObject.transform.localScale = Vector3.Lerp(
                _button.GameObject.transform.localScale,
                config.Scale,
                tweenLerp);

            var captionVirtualColor = isInteractable
                ? config.CaptionColor
                : VirtualColor.Disabled;

            Col4 captionColor;
            _colors.TryGetColor(captionVirtualColor, out captionColor);

            _button.Text.LocalColor = Col4.Lerp(
                _button.Text.LocalColor,
                captionColor,
                tweenLerp);
        }

        /// <summary>
        /// Retrieves the correct config for a state..
        /// </summary>
        /// <param name="state">The ButtonState.</param>
        /// <returns></returns>
        private ButtonStateConfig Config(ButtonState state)
        {
            switch (state)
            {
                case ButtonState.Ready:
                {
                    return _config.ButtonReady;
                }
                case ButtonState.Activating:
                {
                    return _config.ButtonActivating;
                }
                case ButtonState.Activated:
                {
                    return _config.ButtonActivated;
                }
            }

            throw new Exception(String.Format(
                "Could not find ButtonConfig for {0}.",
                state));
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