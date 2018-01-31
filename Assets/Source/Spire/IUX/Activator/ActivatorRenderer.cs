using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;
using UnityEngine.UI;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Renders an activator.
    /// </summary>
    public class ActivatorRenderer : MonoBehaviour
    {
        /// <summary>
        /// Factor for buffer.
        /// </summary>
        private const float AUTO_GEN_BUFFER_FACTOR = 2.0f;

        /// <summary>
        /// Dependencies.
        /// </summary>
        private WidgetConfig _config;
        private TweenConfig _tweens;
        private ColorConfig _colors;
        private ActivatorPrimitive _activator;

        /// <summary>
        /// For losing focus.
        /// </summary>
        private BoxCollider _bufferCollider;

        /// <summary>
        /// True iff the renderer is initialized.
        /// </summary>
        private bool _isInited = false;
        
        /// <summary>
        /// Primary widget of the activator.
        /// </summary>
        public WidgetRenderer Frame;

        /// <summary>
        /// Fill Widget
        /// </summary>
        public WidgetRenderer Fill;

        /// <summary>
        /// Aim Scale Transform.
        /// </summary>
        public WidgetRenderer Aim;

        /// <summary>
        /// Icon.
        /// </summary>
        public Image Icon;

        /// <summary>
        /// Transform affected by the steadiness of intention.
        /// </summary>
        public Transform StabilityTransform;

        /// <summary>
        /// Fills with activation percentage.
        /// </summary>
        public Image FillImage;

        /// <summary>
        /// Spawns when activated
        /// </summary>
        public GameObject ActivationVfx;

        /// <summary>
        /// For gaining focus.
        /// </summary>
        public BoxCollider FocusCollider;

        /// <summary>
        /// Bounding radius of the activator.
        /// </summary>
        public float Radius { get; private set; }

        /// <summary>
        /// Initialization.
        /// </summary>
        internal void Initialize(
            ActivatorPrimitive activator,
            WidgetConfig config,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
            IMessageRouter messages,
            IIntentionManager intention, 
            IInteractionManager interaction,
            IInteractableManager interactables)
        {
            _activator = activator;
            _tweens = tweens;
            _colors = colors;

            _config = config;

            GenerateBufferCollider();
            Radius = CalculateRadius();
            
            if (Aim != null)
            {
                Aim.Initialize(activator);
            }

            if (Fill != null)
            {
                Fill.Initialize(activator);
            }

            if (Frame != null)
            {
                Frame.Initialize(activator);
            }

            _isInited = true;
        }
        
        /// <summary>
        /// Forced activation.
        /// </summary>
        public void Activate()
        {
            if (ActivationVfx != null)
            {
                // TODO: ActivationVFX Pooling.
                var spawnGameObject = Instantiate(ActivationVfx,
                    gameObject.transform.position,
                    gameObject.transform.rotation);
                spawnGameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Frame based update.
        /// </summary>
        private void Update()
        {
            if (!_isInited)
            {
                return;
            }

            var deltaTime = Time.smoothDeltaTime;

            UpdateAimWidget();
            UpdateStabilityTransform();
            UpdateFillImage();
            UpdateFrameWidget(deltaTime);
            UpdateColliders();
        }
        
        /// <summary>
        /// Returns the radius of the widget.
        /// </summary>
        private float CalculateRadius()
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

            if (_bufferCollider == null)
            {
                _bufferCollider = gameObject.AddComponent<BoxCollider>();
            }

            _bufferCollider.size = FocusCollider.size * AUTO_GEN_BUFFER_FACTOR;
        }
        
        /// <summary>
        /// Enables/disables interaction on the primitive.
        /// </summary>
        public void UpdateColliders()
        {
            FocusCollider.enabled = _activator.Interactable;

            _bufferCollider.enabled = _activator.Focused;
        }
        
        /// <summary>
        /// Updates the rotation and scale of the stability transform.
        /// </summary>
        public void UpdateStabilityTransform()
        {
            var focusTween = Fill != null
                ? Fill.Tween
                : 1.0f;

            var degrees = _activator.Stability * _config.StabilityRotation;

            StabilityTransform.localRotation = Quaternion.Euler(0, 0, degrees);
            StabilityTransform.localScale = Vector3.one * focusTween;
        }

        /// <summary>
        /// Updates the fill image with current activation percent.
        /// </summary>
        public void UpdateFillImage()
        {
            FillImage.fillAmount = _activator.Activation;
            Fill.LocalVisible = _activator.CurrentState is ActivatorActivatingState;
        }

        /// <summary>
        /// Sets the aim scale.
        /// </summary>
        public void UpdateAimWidget()
        {
            var aimScale = _config.GetAimScale(_activator.Aim);
            var aimColor = _config.GetAimColor(_activator.Aim);

            Aim.transform.localScale = Vector3.one * aimScale;
            Aim.LocalColor = aimColor;
        }

        /// <summary>
        /// Updates the frame widget based on activator state.
        /// </summary>
        /// <param name="deltaTime"></param>
        public void UpdateFrameWidget(float deltaTime)
        {
            var activatorState = _activator.CurrentState;

            var tweenDuration = _tweens.DurationSeconds(activatorState.Tween);
            var tweenLerp = tweenDuration > Mathf.Epsilon
                ? deltaTime / tweenDuration
                : 1.0f;

            // blend the frame's color.
            var frameColor = _colors.GetColor(activatorState.FrameColor);
            Frame.LocalColor = Col4.Lerp(
                Frame.LocalColor,
                frameColor,
                tweenLerp);
            
            // blend the frame's scale.
            Frame.gameObject.transform.localScale = Vector3.Lerp(
                Frame.gameObject.transform.localScale,
                Vector3.one * activatorState.FrameScale,
                tweenLerp);
        }
    }
}