using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using RLD;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Pushes positional updates through to the delegate.
    /// </summary>
    public class ElementUpdateMonobehaviour : MonoBehaviour, IRTObjectSelectionListener, IRTTransformGizmoListener, IRTDragGizmoListener
    {
        /// <summary>
        /// The delegate to push updates to.
        /// </summary>
        private IElementUpdateDelegate _delegate;
        
        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<Vec3> _positionProp;
        private ElementSchemaProp<Vec3> _rotationProp;
        private ElementSchemaProp<Vec3> _scaleProp;
        private ElementSchemaProp<bool> _lockedProp;
        
        /// <summary>
        /// Retrieves the element.
        /// </summary>
        public Element Element { get; private set; }

        /// <summary>
        /// Preps the behaviour.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="delegate">The delegate to push updates to.</param>
        public void Initialize(Element element, IElementUpdateDelegate @delegate)
        {
            Element = element;
            _delegate = @delegate;
            
            _positionProp = element.Schema.Get<Vec3>("position");
            _rotationProp = element.Schema.Get<Vec3>("rotation");
            _scaleProp = element.Schema.Get<Vec3>("scale");
            _lockedProp = element.Schema.Get<bool>("bool");
        }

        /// <inheritdoc />
        public bool OnCanBeSelected(ObjectSelectEventArgs selectEventArgs)
        {
            if (IsLocked())
            {
                return false;
            }

            // special handling for world anchors
            var anchor = Element as WorldAnchorWidget;
            if (null != anchor)
            {
                // exported anchors cannot be selected
                var src = Element.Schema.GetOwn("src", "").Value;
                if (!string.IsNullOrEmpty(src))
                {
                    return false;
                }

                // anchors that are currently exporting cannot be selected
                var exportTime = Element.Schema.GetOwn("export.time", "").Value;
                if (!string.IsNullOrEmpty(exportTime))
                {
                    return false;
                }
            }

            return true;
        }
        
        /// <inheritdoc />
        public void OnSelected(ObjectSelectEventArgs selectEventArgs)
        {
            
        }

        /// <inheritdoc />
        public void OnDeselected(ObjectDeselectEventArgs deselectEventArgs)
        {
            
        }

        /// <inheritdoc />
        public bool OnCanBeTransformed(Gizmo transformGizmo)
        {
            return OnCanBeSelected(null);
        }

        /// <inheritdoc />
        public void OnTransformed(Gizmo transformGizmo)
        {
            
        }

        /// <inheritdoc />
        public void OnStartDrag()
        {
            
        }

        /// <inheritdoc />
        public void OnEndDrag()
        {
            UpdateDelegate();
        }
        
        /// <summary>
        /// Pushes an update through the delegate.
        /// </summary>
        private void UpdateDelegate()
        {
            var isDirty = false;
            
            // check for position changes
            {
                if (!transform.localPosition.Approximately(_positionProp.Value))
                {
                    _positionProp.Value = transform.localPosition.ToVec();

                    _delegate.Update(Element, "position", _positionProp.Value);

                    isDirty = true;
                }
            }

            // check for rotation changes
            {
                if (!transform.localRotation.eulerAngles.Approximately(_rotationProp.Value))
                {
                    _rotationProp.Value = transform.localRotation.eulerAngles.ToVec();

                    _delegate.Update(Element, "rotation", _rotationProp.Value);

                    isDirty = true;
                }
            }

            // check for scale changes
            {
                if (!transform.localScale.Approximately(_scaleProp.Value.ToVector()))
                {
                    _scaleProp.Value = transform.localScale.ToVec();

                    _delegate.Update(Element, "scale", _scaleProp.Value);

                    isDirty = true;
                }
            }

            if (isDirty)
            {
                _delegate.FinalizeUpdate(Element);
            }
        }

        /// <summary>
        /// Returns true iff the element or any ancestor of the element is locked.
        /// </summary>
        private bool IsLocked()
        {
            if (_lockedProp.Value)
            {
                return true;
            }

            // search up hierarchy
            var parent = Element.Parent;
            while (null != parent)
            {
                if (!(parent is WorldAnchorWidget)
                    && parent.Schema.Get<bool>("locked").Value)
                {
                    return true;
                }

                parent = parent.Parent;
            }

            return false;
        }
    }
}