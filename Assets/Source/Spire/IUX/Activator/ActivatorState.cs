﻿using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Base class for different states inside of activator.
    /// </summary>
    public class ActivatorState : IState
    {
        /// <summary>
        /// Props.
        /// </summary>
        private readonly ElementSchemaProp<float> _propFrameScale;
        private readonly ElementSchemaProp<string> _propFrameColor;
        private readonly ElementSchemaProp<string> _propTween;

        /// <summary>
        /// Color of the button during this state.
        /// </summary>
        public VirtualColor FrameColor
        {
            get
            {
                try
                {
                    return (VirtualColor) Enum.Parse(
                        typeof(VirtualColor),
                        _propFrameColor.Value);
                }
                catch
                {
                    return VirtualColor.Disabled;
                }
            }
        }

        /// <summary>
        /// Color of the button during this state.
        /// </summary>
        public float FrameScale
        {
            get
            {
                return _propFrameScale.Value;
            }
        }

        /// <summary>
        /// Tween into this state.
        /// </summary>
        public TweenType Tween
        {
            get
            {
                try
                {
                    return (TweenType) Enum.Parse(
                        typeof(TweenType),
                        _propTween.Value);
                }
                catch
                {
                    return TweenType.Responsive;
                }
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="frameColor">Color of frame.</param>
        /// <param name="tween">Tween prop.</param>
        /// <param name="frameScale">Frame scale prop.</param>
        public ActivatorState(
            ElementSchemaProp<string> frameColor,
            ElementSchemaProp<string> tween,
            ElementSchemaProp<float> frameScale)
        {
            _propFrameColor = frameColor;
            _propTween = tween;
            _propFrameScale = frameScale;
        }

        /// <summary>
        /// Invoked when the state is entered.
        /// </summary>
        /// <param name="context"></param>
        public virtual void Enter(object context)
        {
            // empty.
        }

        /// <summary>
        /// Invoked once every frame.
        /// </summary>
        /// <param name="deltaTime"></param>
        public virtual void Update(float deltaTime)
        {
            // empty.
        }

        /// <summary>
        /// Invoked when the state is exited.
        /// </summary>
        public virtual void Exit()
        {
            // empty.
        }
    }
}