﻿using UnityEngine;
using UnityEngine.UI;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// TODO: Replace most/if-not-all/of this with schema
    /// Configuration for all widgets.
    /// </summary>
    public class WidgetConfig : MonoBehaviour
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
        public Col4 GetAimColor(float aim)
        {
            return AimFeedbackColor.Evaluate(aim).ToCol();
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
        /// Default Cursor Distance
        /// </summary>
        [Header("Cursor")]
        public float DefaultFocalDistance = 2.0f;

        /// <summary>
        /// How fast does the cursor state change
        /// </summary>
        public TweenType AimTweenType = TweenType.Instant;

        /// <summary>
        /// Tween for gaining focus
        /// </summary>
        public TweenType GainFocusTween = TweenType.Responsive;

        /// <summary>
        /// Tween for losing focus
        /// </summary>
        public TweenType LostFocusTween = TweenType.Deliberate;

        /// <summary>
        /// Spread when activating
        /// </summary>
        public AnimationCurve AimSpread = new AnimationCurve();

        /// <summary>
        /// Modifies Aim Scale
        /// </summary>
        public float AimSpreadMultiplier = 1.0f;

        /// <summary>
        /// Defines how magnetty the cursor is
        /// </summary>
        public AnimationCurve AimMagnet = new AnimationCurve();

        /// <summary>
        /// How aim affects the scale of points
        /// </summary>
        public AnimationCurve AimScale = new AnimationCurve();

        /// <summary>
        /// Modifies Aim Scale
        /// </summary>
        public float AimScaleMultiplier = 1.0f;

        /// <summary>
        /// How aim affects color of points
        /// </summary>
        public Gradient AimColor = new Gradient();

        /// <summary>
        /// Spins per second.
        /// </summary>
        public float SpinSpeed = 2.0f;

        /// <summary>
        /// Text prefab.
        /// </summary>
        [Header("Prefabs")]
        public Text Text;

        /// <summary>
        /// Reticle.
        /// </summary>
        public ReticleRenderer Reticle;

        /// <summary>
        /// Activator.
        /// </summary>
        public ActivatorRenderer Activator;

        /// <summary>
        /// Spins per second.
        /// </summary>
        public float GetDefaultDistanceForCursor()
        {
            return DefaultFocalDistance;
        }

        /// <summary>
        /// Spins per second.
        /// </summary>
        public float GetReticleSpinRateForCursor()
        {
            return SpinSpeed;
        }

        /// <summary>
        /// Spins per second.
        /// </summary>
        public float GetMagnetFromAim(float aim)
        {
            return AimMagnet.Evaluate(aim);
        }

        /// <summary>
        /// Spins per second.
        /// </summary>
        public float GetReticleSpreadFromAim(float aim)
        {
            return AimSpread.Evaluate(aim) * AimSpreadMultiplier;
        }

        /// <summary>
        /// Spins per second.
        /// </summary>
        public Col4 GetReticleColorFromAim(float aim)
        {
            return AimColor.Evaluate(aim).ToCol();
        }

        /// <summary>
        /// Spins per second.
        /// </summary>
        public float GetReticleScaleFromAim(float aim)
        {
            return AimScale.Evaluate(aim) * AimScaleMultiplier;
        }
    }
}