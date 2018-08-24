using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer.Scripting
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

        /// <summary>
        /// Position.
        /// </summary>
        public Vec3 position
        {
            get { return _positionProp.Value; }
            set { _positionProp.Value = value; }
        }
        
        /// <summary>
        /// Rotation.
        /// </summary>
        public Quat rotation
        {
            get { return Quat.Euler(_rotationProp.Value); }
            set { _rotationProp.Value = value.ToQuaternion().eulerAngles.ToVec(); }
        }
        
        /// <summary>
        /// Scale.
        /// </summary>
        public Vec3 scale
        {
            get { return _scaleProp.Value; }
            set { _scaleProp.Value = value; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="element">The element.</param>
        public ElementTransformJsApi(Element element)
        {
            _element = element;

            _positionProp = _element.Schema.Get<Vec3>("position");
            _rotationProp = _element.Schema.Get<Vec3>("rotation");
            _scaleProp = _element.Schema.Get<Vec3>("scale");
        }
    }
}