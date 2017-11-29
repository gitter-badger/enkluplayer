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
        /// Factor for buffer.
        /// </summary>
        private const float AUTO_GEN_BUFFER_FACTOR = 2.0f;

        /// <summary>
        /// Dependencies.
        /// </summary>
        public WidgetConfig Config { get; set; }

        /// <summary>
        /// Internal driver.
        /// </summary>
        private Activator _activator;

        /// <summary>
        /// For losing focus.
        /// </summary>
        private BoxCollider _bufferCollider;

        /// <inheritdoc cref="IInteractive"/>
        public bool Interactable { get { return _activator.Interactable; } }

        /// <inheritdoc cref="IInteractive"/>
        public bool Focused
        {
            get { return _activator.Focused; }
            set { _activator.Focused = value; }
        }

        /// <inheritdoc cref="IInteractive"/>
        public int HighlightPriority
        {
            get { return _activator.HighlightPriority; }
            set { _activator.HighlightPriority = value; }
        }

        /// <inheritdoc cref="IActivator"/>
        public float Radius { get { return _activator.Radius; } }

        /// <inheritdoc cref="IActivator"/>
        public float Aim { get { return _activator.Aim; } }

        /// <inheritdoc cref="IActivator"/>
        public float Stability { get { return _activator.Stability; } }

        /// <inheritdoc cref="IActivator"/>
        public float Activation { get { return _activator.Activation; } }

        /// <inheritdoc cref="IActivator"/>
        public void Activate() { _activator.Activate(); }

        /// <inheritdoc cref="IActivator"/>
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
        /// Initialization.
        /// </summary>
        internal void Initialize(
            WidgetConfig config,
            ILayerManager layers,
            ITweenConfig tweens,
            IColorConfig colors,
            IMessageRouter messages,
            IIntentionManager intention, 
            IInteractionManager interaction)
        {
            Config = config;

            GenerateBufferCollider();

            _activator = new Activator(
                gameObject,
                config, 
                layers, 
                tweens, 
                colors, 
                messages, 
                intention, 
                interaction, 
                CalculateRadius(),
                Cast);
            _activator.OnActivated += Activator_OnActivated;
            SetWidget(_activator);
        }

        /// <summary>
        /// Initialization.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="schema"></param>
        /// <param name="children"></param>
        public override void Load(ElementData data, ElementSchema schema, IElement[] children)
        {
            base.Load(data, schema, children);

            if (AimWidget != null)
            {
                AimWidget.LoadFromMonoBehaviour(this);
            }

            if (FillWidget != null)
            {
                FillWidget.LoadFromMonoBehaviour(this);
            }

            if (FrameWidget != null)
            {
                FrameWidget.LoadFromMonoBehaviour(this);
            }
        }

        /// <summary>
        /// Frame based update.
        /// </summary>
        public override void FrameUpdate()
        {
            base.FrameUpdate();

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
                // TODO: ActivationVFX Pooling.
                var spawnGameObject = Instantiate(ActivationVFX,
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

            if (_bufferCollider == null)
            {
                _bufferCollider = gameObject.AddComponent<BoxCollider>();
            }

            _bufferCollider.size = FocusCollider.size * AUTO_GEN_BUFFER_FACTOR;
        }

        /// <summary>
        /// Returns true if the primitive is targeted.
        /// </summary>
        /// <param name="ray"></param>
        /// <returns></returns>
        public bool Cast(Ray ray)
        {
            if (_bufferCollider != null)
            {
                RaycastHit hitInfo;
                if (_bufferCollider.Raycast(ray, out hitInfo, float.PositiveInfinity))
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

            if (_bufferCollider != null)
            {
                _bufferCollider.enabled = Focused;
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

            var focusTween = FillWidget != null
                ? FillWidget.Tween
                : 1.0f;

            var degrees = Stability * Config.StabilityRotation;

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
            var tweenLerp = tweenDuration > Mathf.Epsilon
                ? deltaTime / tweenDuration
                : 1.0f;

            // blend the frame's color.
            var frameColor = _activator.Colors.GetColor(activatorState.FrameColor);
            FrameWidget.LocalColor = Col4.Lerp(
                FrameWidget.LocalColor,
                frameColor,
                tweenLerp);

            // blend the frame's scale.
            FrameWidget.GameObject.transform.localScale = Vector3.Lerp(
                FrameWidget.GameObject.transform.localScale,
                Vector3.one * activatorState.FrameScale,
                tweenLerp);
        }
    }
}
