using System;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class ElementDesignController : MonoBehaviour
    {
        public Element Element { get; private set; }

        public event Action<ElementDesignController> OnDestroyed;

        public virtual void Initialize(Element element, object context)
        {
            Element = element;
        }

        public virtual void Uninitialize()
        {
            //
        }

        protected virtual void OnDestroy()
        {
            if (null != OnDestroyed)
            {
                OnDestroyed(this);
            }
        }
    }
}