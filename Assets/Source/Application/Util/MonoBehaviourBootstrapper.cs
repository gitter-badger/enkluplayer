using System.Collections;
using CreateAR.Commons.Unity.Http;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// MonoBehaviour based bootstrapper.
    /// </summary>
    public class MonoBehaviourBootstrapper : MonoBehaviour, IBootstrapper
    {
        /// <inheritdoc cref="IBootstrapper"/>
        public void BootstrapCoroutine(IEnumerator coroutine)
        {
            StartCoroutine(coroutine);
        }
    }
}