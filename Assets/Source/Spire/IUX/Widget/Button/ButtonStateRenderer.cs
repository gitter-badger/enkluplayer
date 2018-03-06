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

        /// <summary>
        /// Retrieves the color for the given state.
        /// </summary>
        /// <param name="state">State.</param>
        /// <returns></returns>
        private VirtualColor GetColor(ButtonState state)
        {
            var virtualColorString = _button
                .Schema
                .Get<string>(state.ToString().ToLowerInvariant() + ".color")
                .Value;

            return ParseColor(virtualColorString);
        }

        /// <summary>
        /// Retrieves the Tween value for the given state.
        /// </summary>
        /// <param name="state">sState.</param>
        /// <returns></returns>
        private TweenType GetTween(ButtonState state)
        {
            var tweenTypeString = _button
                .Schema
                .Get<string>(state.ToString().ToLowerInvariant())
                .Value;

            return ParseTween(tweenTypeString);
        }

        /// <summary>
        /// Retrieves the scale value for the given state.
        /// </summary>
        /// <param name="state">State.</param>
        /// <returns></returns>
        private Vector3 GetScale(ButtonState state)
        {
            return _button
                .Schema
                .Get<Vec3>(state.ToString().ToLowerInvariant() + ".scale")
                .Value
                .ToVector();
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