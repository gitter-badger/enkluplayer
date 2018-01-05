using System.Collections;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Http;

namespace CreateAR.SpirePlayer.Editor
{
    /// <summary>
    /// <c>IBootstrapper</c> implementation for the editor.
    /// </summary>
    public class EditorBootstrapper : IBootstrapper
    {
        /// <summary>
        /// List of all coroutines.
        /// </summary>
        private readonly List<IEnumerator> _coroutines = new List<IEnumerator>();
        
        /// <inheritdoc cref="IBootstrapper"/>
        public void BootstrapCoroutine(IEnumerator coroutine)
        {
            _coroutines.Insert(0, coroutine);
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        public void Update()
        {
            for (var i = _coroutines.Count - 1; i >= 0; i--)
            {
                if (!_coroutines[i].MoveNext())
                {
                    _coroutines.RemoveAt(i);
                }
            }
        }
    }
}