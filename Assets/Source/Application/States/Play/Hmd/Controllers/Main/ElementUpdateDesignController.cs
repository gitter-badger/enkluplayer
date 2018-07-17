using System;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Base class for any menu that needs to update element TRS.
    /// </summary>
    public class ElementUpdateDesignController : ElementDesignController
    {
        /// <summary>
        /// Constants.
        /// </summary>
        private const float EPSILON = 0.05f;
        private const float TIME_EPSILON = 0.1f;
        
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
        /// The context passed in.
        /// </summary>
        protected Context _context;

        /// <summary>
        /// Transform of the element.
        /// </summary>
        private Transform _transform;

        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<Vec3> _positionProp;
        private ElementSchemaProp<Vec3> _rotationProp;
        private ElementSchemaProp<Vec3> _scaleProp;

        /// <summary>
        /// Context for this type of controller.
        /// </summary>
        public class Context
        {
            /// <summary>
            /// The delegate to push updates to.
            /// </summary>
            public IElementUpdateDelegate Delegate;
        }

        /// <inheritdoc />
        public override void Initialize(Element element, object context)
        {
            base.Initialize(element, context);

            _context = (Context) context;

            _transform = ((IUnityElement) element).GameObject.transform;
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
        protected override void Update()
        {
            base.Update();

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
            // check for position changes
            {
                if (!_transform.localPosition.Approximately(
                    _positionProp.Value.ToVector(),
                    epsilon))
                {
                    _positionProp.Value = _transform.localPosition.ToVec();

                    _context.Delegate.Update(Element, "position", _positionProp.Value);

                    _isDirty = true;
                }
            }

            // check for rotation changes
            {
                if (!_transform.localRotation.eulerAngles.Approximately(
                    _rotationProp.Value.ToVector(),
                    epsilon))
                {
                    _rotationProp.Value = _transform.localRotation.eulerAngles.ToVec();

                    _context.Delegate.Update(Element, "rotation", _rotationProp.Value);

                    _isDirty = true;
                }
            }

            // check for scale changes
            {
                if (!_transform.localScale.Approximately(
                    _scaleProp.Value.ToVector(),
                    epsilon))
                {
                    _scaleProp.Value = _transform.localScale.ToVec();

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