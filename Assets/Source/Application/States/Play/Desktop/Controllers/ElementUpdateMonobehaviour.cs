using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using RTEditor;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Pushes positional updates through to the delegate.
    /// </summary>
    public class ElementUpdateMonobehaviour : MonoBehaviour, IRTEditorEventListener
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
        /// The last time a gizmo altered the element.
        /// </summary>
        private DateTime _lastAlterTime;

        /// <summary>
        /// The last time the element pushed an update.
        /// </summary>
        private DateTime _lastUpdateTime;

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
            if (_lockedProp.Value)
            {
                return false;
            }

            // search up hierarchy
            var parent = Element.Parent;
            while (null != parent)
            {
                if (parent.Schema.Get<bool>("locked").Value)
                {
                    return false;
                }

                parent = Element.Parent;
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
        public void OnAlteredByTransformGizmo(Gizmo gizmo)
        {
            _lastAlterTime = DateTime.Now;
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void Update()
        {
            var now = DateTime.Now;
            if (_lastAlterTime > _lastUpdateTime
                && now.Subtract(_lastAlterTime).TotalMilliseconds > 100)
            {
                _lastUpdateTime = now;

                UpdateDelegate();
            }
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
    }
}