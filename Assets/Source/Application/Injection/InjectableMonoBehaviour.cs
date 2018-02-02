using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Base class for MonoBehaviours that need to have bindings injected into
    /// them.
    /// </summary>
    public class InjectableMonoBehaviour : MonoBehaviour
    {
        /// <inheritdoc cref="MonoBehaviour"/>
        protected virtual void Awake()
        {
            Main.Inject(this);
        }
    }
}