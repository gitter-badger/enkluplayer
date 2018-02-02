using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    [RequireComponent(typeof(VineRawMonoBehaviour))]
    public class PlaceObjectController : InjectableMonoBehaviour
    {
        private VineRawMonoBehaviour _vine;

        [InjectElements("..(@type==ImageWidget)")]
        public ImageWidget[] Images { get; private set; }

        [InjectElements("..btn-ok")]
        public ButtonWidget BtnOk { get; private set; }

        [InjectElements("..btn-cancel")]
        public ButtonWidget BtnCancel { get; private set; }

        [InjectElements("..(@type==ContentWidget)")]
        public ContentWidget Content { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            _vine = GetComponent<VineRawMonoBehaviour>();
            _vine.OnElementCreated += Vine_OnElementCreated;
        }

        private void Vine_OnElementCreated(Element element)
        {
            InjectElementsAttribute.InjectElements(this, element);

            Debug.Assert(2 == Images.Length);
            Debug.Assert(null != BtnOk);
            Debug.Assert(null != BtnCancel);
            Debug.Assert(null != Content);
        }
    }
}