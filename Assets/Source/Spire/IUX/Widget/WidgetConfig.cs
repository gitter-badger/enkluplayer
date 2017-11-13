﻿using CreateAR.SpirePlayer.UI;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// TODO: Replace most/if-not-all/of this with schema
    /// Configuration for all widgets.
    /// </summary>
    public class WidgetConfig : MonoBehaviour, IWidgetConfig
    {
        /// <summary>
        /// Color of the aim widget as a function of aim percentage.
        /// </summary>
        [Header("Aim")]
        public Gradient AimFeedbackColor;

        /// <summary>
        /// Minimum scale of the Aim Widget.
        /// </summary>
        public AnimationCurve AimFeedbackScale = new AnimationCurve(
            new Keyframe(0, 0.04f),
            new Keyframe(1, 0.06f));

        /// <summary>
        /// Multiplies aim fill.
        /// </summary>
        public AnimationCurve AimFillMultiplier = new AnimationCurve(
            new Keyframe(0, 1),
            new Keyframe(1, 4));

        /// <summary>
        /// Accessor for stability multiplier.
        /// </summary>
        /// <param name="aim"></param>
        /// <returns></returns>
        public float GetAimScale(float aim)
        {
            return AimFeedbackScale.Evaluate(aim);
        }

        /// <summary>
        /// Accessor for stability multiplier.
        /// </summary>
        /// <param name="aim"></param>
        /// <returns></returns>
        public Color GetAimColor(float aim)
        {
            return AimFeedbackColor.Evaluate(aim);
        }

        /// <summary>
        /// Accessor for stability multiplier.
        /// </summary>
        /// <param name="stability"></param>
        /// <returns></returns>
        public float GetFillRateMultiplierFromAim(float stability)
        {
            return AimFillMultiplier.Evaluate(stability);
        }

        /// <summary>
        /// Multiplies steadiness fill.
        /// </summary>
        [Header("Steadiness")]
        public AnimationCurve SteadinessFillMultiplier = new AnimationCurve(
            new Keyframe(0, 1),
            new Keyframe(1, 4));

        /// <summary>
        /// Accessor for stability multiplier.
        /// </summary>
        /// <param name="stability"></param>
        /// <returns></returns>
        public float GetFillRateMultiplierFromStability(float stability)
        {
            return SteadinessFillMultiplier.Evaluate(stability);
        }

        /// <summary>
        /// The rotation in degrees of the steadiness transform
        /// </summary>
        public float SteadinessRotation;

        /// <summary>
        /// Steadiness Rotation Accessor
        /// </summary>
        public float StabilityRotation { get { return SteadinessRotation; } }

        /// <summary>
        /// Duration for full fillm in seconds.
        /// </summary>
        [Header("Activation")]
        public float FillDuration = 2.0f;

        /// <summary>
        /// Duration for full fillm in seconds.
        /// </summary>
        /// <returns></returns>
        public float GetFillDuration()
        {
            return FillDuration;
        }

        /// <summary>
        /// Fill decay multiplier.
        /// </summary>
        public AnimationCurve FillDecay = AnimationCurve.EaseInOut(0, 1, 1, 0);

        /// <summary>
        /// Fill decay multiplier.
        /// </summary>
        /// <param name="elapsed"></param>
        /// <returns></returns>
        public float GetFillDelay(float elapsed)
        {
            return FillDecay.Evaluate(elapsed);
        }

        /// <summary>
        /// Spawns the effect on activation
        /// </summary>
        [Header("Activation")]
        public GameObject ActivationVFX;
    }
}