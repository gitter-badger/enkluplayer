using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.UI;
using UnityEngine;
using UnityEngine.UI;

namespace CreateAR.SpirePlayer
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
        //private IInteractableManager _interactables;
        //private IInteractionManager _interaction;
        //private IIntentionManager _intention;
        private WidgetConfig _config;
        private ITweenConfig _tweens;
        private IColorConfig _colors;
        //private IMessageRouter _messages;
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
        /// Props.
        /// </summary>
        //private ElementSchemaProp<bool> _propHighlighted;
        //private ElementSchemaProp<bool> _propInteractionEnabled;
        //private ElementSchemaProp<int> _propHighlightPriority;

        ///// <summary>
        ///// Backing variable for Focused property.
        ///// </summary>
        //private bool _focused;

        ///// <summary>
        ///// State management for the button
        ///// </summary>
        //private FiniteStateMachine _states;

        ///// <summary>
        ///// Dependencies.
        ///// </summary>
        //public WidgetConfig Config { get; set; }

        ///// <summary>
        ///// Current State Accessor.
        ///// </summary>
        //public ActivatorState CurrentState
        //{
        //    get
        //    {
        //        return (ActivatorState)_states.Current;
        //    }
        //}

        ///// <inheritdoc cref="IInteractable"/>
        //public bool Interactable
        //{
        //    get
        //    {
        //        const float FOCUSABLE_THRESHOLD = 0.99f;
        //        return Visible && Tween > FOCUSABLE_THRESHOLD
        //               && InteractionEnabled
        //               && (!_interaction.IsOnRails || Highlighted);
        //    }
        //}

        ///// <inheritdoc cref="IInteractable"/>
        //public virtual bool Focused
        //{
        //    get
        //    {
        //        return _focused;
        //    }
        //    set
        //    {
        //        if (_focused != value)
        //        {
        //            _focused = value;

        //            if (_focused)
        //            {
        //                _messages.Publish(MessageTypes.WIDGET_FOCUS, new WidgetFocusEvent());
        //            }
        //            else
        //            {
        //                _messages.Publish(MessageTypes.WIDGET_UNFOCUS, new WidgetUnfocusEvent());
        //            }
        //        }
        //    }
        //}

        ///// <inheritdoc cref="IInteractable"/>
        //public int HighlightPriority
        //{
        //    get { return _propHighlightPriority.Value; }
        //    set { _propHighlightPriority.Value = value; }
        //}

        /// <summary>
        /// Bounding radius of the activator.
        /// TODO: Spherical bounds are an implementation detail.
        /// TODO: Refactor out of interface.
        /// </summary>
        public float Radius { get; private set; }
        
        /// <summary>
        /// Aim.
        /// </summary>
        //public float Aim { get; set; }

        ///// <summary>
        ///// (IUX PATENT)
        ///// A scalar percentage [0..1] representing targeting steadiness.
        ///// 0 = low steadiness -> may be moving over on way to something else.
        ///// 1 - high steadiness -> definitely stationary over this.
        ///// </summary>
        //public float Stability { get; set; }

        ///// <summary>
        ///// (IUX PATENT)
        ///// A scalar percentage [0..1] representing activation completion.
        ///// </summary>
        //public float Activation { get; set; }

        ///// <summary>
        ///// Invoked when the activator is activated.
        ///// </summary>
        //public event Action<ActivatorRenderer> OnActivated;

        /// <summary>
        /// Highligted Accessor/Mutator
        /// </summary>
        //public bool Highlighted
        //{
        //    get { return _propHighlighted.Value; }
        //    set { _propHighlighted.Value = value; }
        //}

        ///// <summary>
        ///// If true, can be interacted with.
        ///// </summary>
        //public bool InteractionEnabled
        //{
        //    get { return _propInteractionEnabled.Value; }
        //    set { _propInteractionEnabled.Value = value; }
        //}

        ///// <summary>
        ///// True iff Aim is enabled.
        ///// </summary>
        //public bool AimEnabled { get; set; }

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
            ActivatorPrimitive activator,
            WidgetConfig config,
            ILayerManager layers,
            ITweenConfig tweens,
            IColorConfig colors,
            IMessageRouter messages,
            IIntentionManager intention, 
            IInteractionManager interaction,
            IInteractableManager interactables)
        {
            _activator = activator;
            //_interaction = interaction;
            //_interactables = interactables;
            //_intention = intention;
            _tweens = tweens;
            _colors = colors;
            //_messages = messages;

            _config = config;

            GenerateBufferCollider();
            Radius = CalculateRadius();
            //AimEnabled = true;

            //Initialize(Config, layers, tweens, colors, messages);

            if (AimWidget != null)
            {
                AimWidget.LoadFromActivator(_activator);
            }

            if (FillWidget != null)
            {
                FillWidget.LoadFromActivator(_activator);
            }

            if (FrameWidget != null)
            {
                FrameWidget.LoadFromActivator(_activator);
            }

            _isInited = true;
        }

        /// <summary>
        /// Initialization.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="schema"></param>
        /// <param name="children"></param>
        //public override void Load(ElementData data, ElementSchema schema, IElement[] children)
        //{
        //    base.Load(data, schema, children);

            // States
            //{
                //_states = new FiniteStateMachine(new IState[]
                //{
                //    new ActivatorReadyState(this, Schema),
                //    new ActivatorActivatingState(this, _intention, Schema),
                //    new ActivatorActivatedState(this, _messages, Schema)
                //});
                //_states.Change<ActivatorReadyState>();
            //}

            // Interaction
            //{
                
            //}

            //if (AimWidget != null)
            //{
            //    AimWidget.LoadFromMonoBehaviour(this);
            //}

            //if (FillWidget != null)
            //{
            //    FillWidget.LoadFromMonoBehaviour(this);
            //}

            //if (FrameWidget != null)
            //{
            //    FrameWidget.LoadFromMonoBehaviour(this);
            //}

            //_interactables.Add(this);
        //}
        
        /// <summary>
        /// Frame based update.
        /// </summary>
        //public override void FrameUpdate()
        private void Update()
        {
            //base.FrameUpdate();

            //var deltaTime = Time.smoothDeltaTime;

            //if (!Focused
            //    || !Interactable
            //    || !AimEnabled)
            //{
            //    Aim = 0.0f;
            //    Stability = 0.0f;
            //}
            //else
            //{
            //    UpdateAim();
            //    UpdateStability(deltaTime);
            //}

            //_states.Update(deltaTime);

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

        ///// <inheritdoc cref="IRaycaster"/>
        //public bool Raycast(Vec3 origin, Vec3 direction)
        //{
        //    if (FocusCollider != null)
        //    {
        //        var ray = new Ray(origin.ToVector(), direction.ToVector());
        //        RaycastHit hitInfo;
        //        if (FocusCollider.Raycast(ray, out hitInfo, float.PositiveInfinity))
        //        {
        //            return true;
        //        }
        //    }

        //    return false;
        //}

        /// <summary>
        /// Forced activation.
        /// </summary>
        public void Activate()
        {
            if (ActivationVFX != null)
            {
                // TODO: ActivationVFX Pooling.
                var spawnGameObject = Instantiate(ActivationVFX,
                    gameObject.transform.position,
                    gameObject.transform.rotation);
                spawnGameObject.SetActive(true);
            }
        }

        ///// <summary>
        ///// Changes the state of the activator.
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        //public void ChangeState<T>() where T : ActivatorState
        //{
        //    _states.Change<T>();
        //}

        ///// <summary>
        ///// Called when the object is being destroyed.
        ///// </summary>
        //private void OnDestroy()
        //{
        //    _interactables.Remove(this);
        //}

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

        ///// <summary>
        ///// Updates the aim as a function of focus towards the center of the widget.
        ///// </summary>
        //private void UpdateAim()
        //{
        //    var eyePosition = _intention.Origin;
        //    var eyeDirection = _intention.Forward;
        //    var delta = GameObject.transform.position.ToVec() - eyePosition;
        //    var directionToButton = delta.Normalized;

        //    var eyeDistance = delta.Magnitude;
        //    var radius = Radius;

        //    var maxTheta = Mathf.Atan2(radius, eyeDistance);

        //    var cosTheta = Vec3.Dot(
        //        directionToButton,
        //        eyeDirection);
        //    var theta = Mathf.Approximately(cosTheta, 1.0f)
        //        ? 0.0f
        //        : Mathf.Acos(cosTheta);

        //    Aim = Mathf.Approximately(maxTheta, 0.0f)
        //        ? 0.0f
        //        : 1.0f - Mathf.Clamp01(Mathf.Abs(theta / maxTheta));
        //}

        ///// <summary>
        ///// Updates the steadiness feedback
        ///// </summary>
        ///// <param name="deltaTime"></param>
        //private void UpdateStability(float deltaTime)
        //{
        //    var targetStability = Focused
        //        ? _intention.Stability
        //        : 0.0f;

        //    const float STABILITY_LERP_RATE_MAGIC_NUMBER = 8.0f;
        //    var lerp = deltaTime * STABILITY_LERP_RATE_MAGIC_NUMBER;
        //    Stability = Mathf.Lerp(
        //        Stability,
        //        targetStability,
        //        lerp);
        //}

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
            var focusTween = FillWidget != null
                ? FillWidget.Tween
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
            FillWidget.LocalVisible = _activator.CurrentState is ActivatorActivatingState;
        }

        /// <summary>
        /// Sets the aim scale.
        /// </summary>
        public void UpdateAimWidget()
        {
            var aimScale = _config.GetAimScale(_activator.Aim);
            var aimColor = _config.GetAimColor(_activator.Aim);

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

            var tweenDuration = _tweens.DurationSeconds(activatorState.Tween);
            var tweenLerp = tweenDuration > Mathf.Epsilon
                ? deltaTime / tweenDuration
                : 1.0f;

            // blend the frame's color.
            var frameColor = _colors.GetColor(activatorState.FrameColor);
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