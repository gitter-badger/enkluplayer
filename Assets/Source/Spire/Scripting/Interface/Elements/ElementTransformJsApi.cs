using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    public class ElementTransformJsApi
    {
        private readonly Element _element;
        private readonly ElementSchemaProp<Vec3> _positionProp;
        private readonly ElementSchemaProp<Quat> _rotationProp;
        private readonly ElementSchemaProp<Vec3> _scaleProp;

        public Vec3 localPosition
        {
            get { return _positionProp.Value; }
            set { _positionProp.Value = value; }
        }
        
        public Quat localRotation
        {
            get { return _rotationProp.Value; }
            set { _rotationProp.Value = value; }
        }
        
        public Vec3 localScale
        {
            get { return _scaleProp.Value; }
            set { _scaleProp.Value = value; }
        }

        public ElementTransformJsApi(Element element)
        {
            _element = element;
        }
    }
}