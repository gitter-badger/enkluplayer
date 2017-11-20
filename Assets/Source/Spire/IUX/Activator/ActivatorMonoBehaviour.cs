using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.UI;
using UnityEngine;
using UnityEngine.UI;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Unity implementation of an IActivator.
    /// </summary>
    public class ActivatorMonoBehaviour : WidgetMonoBehaviour, IActivator
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        public IWidgetConfig Config { get; set; }

        /// <summary>
        /// Internal driver.
        /// </summary>
        private Activator _activator;

        /// <summary>
        /// IInteractive interface.
        /// </summary>
        public bool Focused { get { return _activator.Focused; } }
        public bool Interactable { get { return _activator.Interactable; } }
        public int HighlightPriority
        {
            get { return _activator.HighlightPriority; }
            set { _activator.HighlightPriority = value; }
        }

        /// <summary>
        /// IActivator interface.
        /// </summary>
        public float Radius { get { return _activator.Radius; } }
        public float Aim { get { return _activator.Aim; } }
        public float Stability { get { return _activator.Stability; } }
        public float Activation { get { return _activator.Activation; } }
        public void Activate() { _activator.Activate(); }
        public event Action<IActivator> OnActivated;

        /// <summary>
        /// Primary widget of the activator.
        /// </summary>
        public WidgetMonoBehaviour FrameWidget;

        /// <summary>
        /// Transform affected by the steadiness of intention.
        /// </summary>
        public Transform StabilityTransform;

        /// <summary>
        /// Fills with activation percentage.
        /// </summary>
        public Image FillImage;

        /// <summary>
        /// Fill Widget
        /// </summary>
        public WidgetMonoBehaviour FillWidget;

        /// <summary>
        /// Shows/Hides w/ Focus.
        /// </summary>
        public WidgetMonoBehaviour ActivationWidget;

        /// <summary>
        /// Aim Scale Transform.
        /// </summary>
        public WidgetMonoBehaviour AimWidget;

        /// <summary>
        /// Spawns when activated
        /// </summary>
        public GameObject ActivationVFX;

        /// <summary>
        /// For gaining focus.
        /// </summary>
        public BoxCollider FocusCollider;

        /// <summary>
        /// For losing focus.
        /// </summary>
        public BoxCollider BufferCollider;
       
        /// <summary>
        /// Initialization
        /// </summary>
        internal void Initialize(
            IWidgetConfig config,
            ILayerManager layers,
            ITweenConfig tweens,
            IColorConfig colors,
            IPrimitiveFactory primitives,
            IMessageRouter messages,
            IIntentionManager intention, 
            IInteractionManager interaction)
        {
            Config = config;

            GenerateBufferCollider();

            _activator 
                = new Activator(
                    config, 
                    layers, 
                    tweens, 
                    colors, 
                    primitives, 
                    messages, 
                    intention, 
                    interaction, 
                    CalculateRadius(),
                    Cast);
            _activator.OnActivated += Activator_OnActivated;
        }

        /// <summary>
        /// Frame based update.
        /// </summary>
        public void UpdateInternal()
        {
            var deltaTime = Time.smoothDeltaTime;

            UpdateAimWidget();
            UpdateStabilityTransform();
            UpdateFillImage();
            UpdateFrameWidget(deltaTime);
            UpdateColliders();
        }

        /// <summary>
        /// Activates the spawn VFX
        /// </summary>
        public void Activator_OnActivated(IActivator activator)
        {
            if (ActivationVFX != null)
            {
                /// TODO: ActivationVFX Pooling.
                var spawnGameObject
                    = Instantiate(ActivationVFX,
                            gameObject.transform.position,
                            gameObject.transform.rotation);
                spawnGameObject.SetActive(true);
            }

            if (OnActivated != null)
            {
                OnActivated(this);
            }
        }

        /// <summary>
        /// Returns the radius of the widget.
        /// </summary>
        public float CalculateRadius()
        {
            var radius = 1f;
            if (null != FocusCollider)
            {
                var size = FocusCollider.size;
                var scale = FocusCollider.transform.lossyScale;
                var scaledSize = new Vector3(
                    size.x * scale.x,
                    size.y * scale.y,
                    size.z * scale.z);
                radius = 0.5f * (scaledSize.x + scaledSize.y + scaledSize.z) / 3f;
            }

            return radius;
        }

        /// <summary>
        /// Generate buffer collider
        /// </summary>
        private void GenerateBufferCollider()
        {
            if (FocusCollider == null)
            {
                Log.Error(this, "Missing FocusCollider for AutoGenerateBufferCollider!");
                return;
            }

            if (BufferCollider == null)
            {
                BufferCollider = gameObject.AddComponent<BoxCollider>();
            }

            const float AUTO_GEN_BUFFER_FACTOR = 2.0f;
            BufferCollider.size = FocusCollider.size * AUTO_GEN_BUFFER_FACTOR;
        }

        /// <summary>
        /// Returns true if the primitive is targeted.
        /// </summary>
        /// <param name="ray"></param>
        /// <returns></returns>
        public bool Cast(Ray ray)
        {
            if (BufferCollider != null)
            {
                RaycastHit hitInfo;
                if (BufferCollider.Raycast(ray, out hitInfo, float.PositiveInfinity))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Enables/disables interaction on the primitive.
        /// </summary>
        public void UpdateColliders()
        {
            if (FocusCollider != null)
            {
                FocusCollider.enabled = Interactable;
            }

            if (BufferCollider != null)
            {
                BufferCollider.enabled = Focused;
            }
        }


        /// <summary>
        /// Updates the rotation and scale of the stability transform.
        /// </summary>
        public void UpdateStabilityTransform()
        {
            if (StabilityTransform == null)
            {
                return;
            }

            var focusTween
                = ActivationWidget != null
                    ? ActivationWidget.Tween
                    : 1.0f;

            var degrees
                = Stability
                  * Config.StabilityRotation;

            StabilityTransform.localRotation = Quaternion.Euler(0, 0, degrees);
            StabilityTransform.localScale = Vector3.one * focusTween;
        }

        /// <summary>
        /// Updates the fill image with current activation percent.
        /// </summary>
        public void UpdateFillImage()
        {
            if (FillImage == null)
            {
                return;
            }

            FillImage.fillAmount = Activation;


            if (FillWidget == null)
            {
                return;
            }

            FillWidget.LocalVisible = _activator.CurrentState is ActivatorActivatingState;
        }

        /// <summary>
        /// Sets the aim scale.
        /// </summary>
        public void UpdateAimWidget()
        {
            if (AimWidget == null)
            {
                return;
            }

            var aimScale = Config.GetAimScale(Aim);
            var aimColor = Config.GetAimColor(Aim);

            AimWidget.transform.localScale = Vector3.one * aimScale;
            AimWidget.LocalColor = aimColor;
        }

        /// <summary>
        /// Updates the frame widget based on activator state.
        /// </summary>
        /// <param name="deltaTime"></param>
        public void UpdateFrameWidget(float deltaTime)
        {
            var activatorState = _activator.CurrentState;

            var tweenDuration = _activator.Tweens.DurationSeconds(activatorState.Tween);
            var tweenLerp
                = tweenDuration > Mathf.Epsilon
                    ? deltaTime / tweenDuration
                    : 1.0f;

            // blend the frame's color.
            var frameColor = _activator.Colors.GetColor(activatorState.FrameColor);
            FrameWidget.LocalColor
                = Col4.Lerp(
                    FrameWidget.LocalColor,
                    frameColor,
                    tweenLerp);

            // blend the frame's scale.
            FrameWidget.GameObject.transform.localScale
                = Vector3.Lerp(
                    FrameWidget.GameObject.transform.localScale,
                    Vector3.one * activatorState.FrameScale,
                    tweenLerp);
        }
    }
}
