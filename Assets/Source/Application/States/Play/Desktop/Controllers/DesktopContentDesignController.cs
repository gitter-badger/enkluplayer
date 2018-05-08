using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Content controller for desktop designer.
    /// </summary>
    public class DesktopContentDesignController : ElementDesignController
    {
        /// <summary>
        /// Context for this type of controller.
        /// </summary>
        public class DesktopContentDesignControllerContext
        {
            /// <summary>
            /// The delegate to push updates to.
            /// </summary>
            public IElementUpdateDelegate Delegate;
        }

        /// <summary>
        /// Constants.
        /// </summary>
        private const float EPSILON = 0.05f;
        private const float TIME_EPSILON = 0.1f;

        /// <summary>
        /// The context passed in.
        /// </summary>
        private DesktopContentDesignControllerContext _context;
        
        /// <summary>
        /// Time of last finalize.
        /// </summary>
        private DateTime _lastFinalize;

        /// <summary>
        /// True iff needs to save.
        /// </summary>
        private bool _isDirty;

        /// <summary>
        /// True iff updates should be pushed to Schema.
        /// </summary>
        private bool _updatesEnabled = false;

        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<Vec3> _positionProp;
        private ElementSchemaProp<Vec3> _rotationProp;
        private ElementSchemaProp<Vec3> _scaleProp;

        /// <inheritdoc />
        public override void Initialize(Element element, object context)
        {
            base.Initialize(element, context);

            _context = (DesktopContentDesignControllerContext) context;

            _positionProp = Element.Schema.Get<Vec3>("position");
            _rotationProp = Element.Schema.Get<Vec3>("rotation");
            _scaleProp = Element.Schema.Get<Vec3>("scale");
        }

        /// <inheritdoc />
        public override void Uninitialize()
        {
            base.Uninitialize();
            
            _updatesEnabled = false;
        }
        
        /// <summary>
        /// Disables pushing updates to schema.
        /// </summary>
        public void DisableUpdates()
        {
            FinalizeState();
            _updatesEnabled = false;
        }

        /// <summary>
        /// Enables pushing updates to schema.
        /// </summary>
        public void EnableUpdates()
        {
            _updatesEnabled = true;
        }

        /// <summary>
        /// Pushes a final, exact state.
        /// </summary>
        public void FinalizeState()
        {
            UpdateDelegate(float.Epsilon);
        }
        
        /// <inheritdoc cref="MonoBehaviour"/>
        private void Update()
        {
            if (!_updatesEnabled)
            {
                return;
            }

            var now = DateTime.Now;
            var isUpdateable = now.Subtract(_lastFinalize).TotalSeconds > TIME_EPSILON;

            if (isUpdateable)
            {
                UpdateDelegate(EPSILON);
            }
        }

        /// <summary>
        /// Pushes an update through the delegate.
        /// </summary>
        private void UpdateDelegate(float epsilon)
        {
            var trans = gameObject.transform;

            // check for position changes
            {
                if (!trans.localPosition.Approximately(
                    _positionProp.Value.ToVector(),
                    epsilon))
                {
                    _positionProp.Value = trans.localPosition.ToVec();

                    _context.Delegate.Update(Element, "position", _positionProp.Value);

                    _isDirty = true;
                }
            }

            // check for rotation changes
            {
                if (!trans.localRotation.eulerAngles.Approximately(
                    _rotationProp.Value.ToVector(),
                    epsilon))
                {
                    _rotationProp.Value = trans.localRotation.eulerAngles.ToVec();

                    _context.Delegate.Update(Element, "rotation", _rotationProp.Value);

                    _isDirty = true;
                }
            }

            // check for scale changes
            {
                if (!trans.localScale.Approximately(
                    _scaleProp.Value.ToVector(),
                    epsilon))
                {
                    _scaleProp.Value = trans.localScale.ToVec();

                    _context.Delegate.Update(Element, "scale", _scaleProp.Value);

                    _isDirty = true;
                }
            }

            if (_isDirty)
            {
                _isDirty = false;
                _lastFinalize = DateTime.Now;

                _context.Delegate.FinalizeUpdate(Element);
            }
        }
    }
}