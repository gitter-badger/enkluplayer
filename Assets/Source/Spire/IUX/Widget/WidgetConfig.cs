using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Configuration for all widgets.
    /// </summary>
    public class WidgetConfig : MonoBehaviour
    {
        /// <summary>
        /// Multiplier for auto-generated buffer colliders.
        /// </summary>
        public float AutoGenBufferFactor = 1.5f;

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
        /// Multiplies steadiness fill.
        /// </summary>
        [Header("Steadiness")]
        public AnimationCurve SteadinessFillMultiplier = new AnimationCurve(
            new Keyframe(0, 1),
            new Keyframe(1, 4));

        /// <summary>
        /// The rotation in degrees of the steadiness transform
        /// </summary>
        public float SteadinessRotation;

        /// <summary>
        /// Duration for full fillm in seconds.
        /// </summary>
        [Header("Activation")]
        public float FillDuration = 2.0f;

        /// <summary>
        /// Fill decay multiplier.
        /// </summary>
        public AnimationCurve FillDecay = AnimationCurve.EaseInOut(0, 1, 1, 0);
        
        /// <summary>
        /// Spawns the effect on activation
        /// </summary>
        [Header("Activation")]
        public GameObject ActivationVFX;
    }
}