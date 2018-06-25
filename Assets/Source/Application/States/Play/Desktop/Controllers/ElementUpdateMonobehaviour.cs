using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using RTEditor;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Piushes positional updates through to the delegate.
    /// </summary>
    public class ElementUpdateMonobehaviour : MonoBehaviour, IRTEditorEventListener
    {
        /// <summary>
        /// Constants.
        /// </summary>
        private const float EPSILON = 0.05f;
        private const float TIME_EPSILON = 0.1f;
        
        /// <summary>
        /// The delegate to push updates to.
        /// </summary>
        private IElementUpdateDelegate _delegate;
        
        /// <summary>
        /// The element to watch.
        /// </summary>
        private Element _element;

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

        /// <summary>
        /// Retrieves the element.
        /// </summary>
        public Element Element
        {
            get { return _element; }
        }
        
        /// <summary>
        /// Preps the behaviour.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="delegate">The delegate to push updates to.</param>
        public void Initialize(Element element, IElementUpdateDelegate @delegate)
        {
            _element = element;
            _delegate = @delegate;
            
            _positionProp = element.Schema.Get<Vec3>("position");
            _rotationProp = element.Schema.Get<Vec3>("rotation");
            _scaleProp = element.Schema.Get<Vec3>("scale");
        }

        /// <inheritdoc />
        public bool OnCanBeSelected(ObjectSelectEventArgs selectEventArgs)
        {
            return true;
        }

        /// <inheritdoc />
        public void OnSelected(ObjectSelectEventArgs selectEventArgs)
        {
            _updatesEnabled = true;

            Log.Info(this, "Enabling updates.");
        }

        /// <inheritdoc />
        public void OnDeselected(ObjectDeselectEventArgs deselectEventArgs)
        {
            Log.Info(this, "Disabling updates.");
            
            FinalizeState();
            _updatesEnabled = false;
        }

        /// <inheritdoc />
        public void OnAlteredByTransformGizmo(Gizmo gizmo)
        {
            // 
        }
        
        /// <summary>
        /// Pushes a final, exact state.
        /// </summary>
        private void FinalizeState()
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

                    _delegate.Update(_element, "position", _positionProp.Value);

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

                    _delegate.Update(_element, "rotation", _rotationProp.Value);

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

                    _delegate.Update(_element, "scale", _scaleProp.Value);

                    _isDirty = true;
                }
            }

            if (_isDirty)
            {
                _isDirty = false;
                _lastFinalize = DateTime.Now;

                _delegate.FinalizeUpdate(_element);
            }
        }
    }
}