using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using Jint.Native;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Transform shortcuts.
    /// </summary>
    public class ElementTransformJsApi : IElementTransformJsApi
    {
        /// <summary>
        /// Element that we're wrapping.
        /// </summary>
        private readonly Element _element;

        /// <summary>
        /// Element as a ContentWidget.
        /// TODO: Remove this when world positioning better handled.
        /// </summary>
        private readonly Widget _widget;
        
        /// <summary>
        /// Backing position prop.
        /// </summary>
        private readonly ElementSchemaProp<Vec3> _positionProp;
        
        /// <summary>
        /// Backing rotation prop.
        /// </summary>
        private readonly ElementSchemaProp<Vec3> _rotationProp;
        
        /// <summary>
        /// Backing scale prop.
        /// </summary>
        private readonly ElementSchemaProp<Vec3> _scaleProp;

        /// <inheritdoc />
        public Vec3 position
        {
            get { return _positionProp.Value; }
            set { _positionProp.Value = value; }
        }

        /// <inheritdoc />
        public Quat rotation
        {
            get { return Quat.Euler(_rotationProp.Value); }
            set { _rotationProp.Value = value.ToQuaternion().eulerAngles.ToVec(); }
        }

        /// <inheritdoc />
        public Vec3 scale
        {
            get { return _scaleProp.Value; }
            set { _scaleProp.Value = value; }
        }

        /// <summary>
        /// Forward.
        /// </summary>
        public Vec3 forward
        {
            get { return Quat.Mult(rotation, Vec3.Forward); }
        }

        /// <inheritdoc />
        [DenyJsAccess]
        public Vec3 worldPosition
        {
            get
            {
                if (_widget != null)
                {
                    return _widget.GameObject.transform.position.ToVec();
                }
                Log.Warning(this, "Trying to get worldPosition for non-widget. Tell us your use-case!");
                return position;
            }
        }

        /// <inheritdoc />
        public Vec3 positionRelativeTo(IEntityJs entity)
        {
            return worldPosition - entity.transform.worldPosition;
        }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="element">The element.</param>
        public ElementTransformJsApi(Element element)
        {
            _element = element;
            _widget = element as Widget;

            _positionProp = _element.Schema.Get<Vec3>("position");
            _rotationProp = _element.Schema.Get<Vec3>("rotation");
            _scaleProp = _element.Schema.Get<Vec3>("scale");
        }

        /// <summary>
        /// Turns this transform to face a direction.
        /// </summary>
        /// <param name="direction"></param>
        public void lookAt(Vec3 direction)
        {
            rotation = Quat.FromToRotation(Vec3.Forward, direction);
        }
    }
}