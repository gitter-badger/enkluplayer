using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class ElementDesignController : MonoBehaviour
    {
        public Element Element { get; private set; }

        public virtual void Initialize(Element element, object context)
        {
            Element = element;
        }

        public virtual void Uninitialize()
        {
            Element = null;
        }
    }
}