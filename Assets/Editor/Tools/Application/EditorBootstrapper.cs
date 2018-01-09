using System.Collections;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;

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

        /// <summary>
        /// List of enumerators to remove.
        /// </summary>
        private readonly List<IEnumerator> _toRemove = new List<IEnumerator>();

        /// <inheritdoc cref="IBootstrapper"/>
        public void BootstrapCoroutine(IEnumerator coroutine)
        {
            _coroutines.Add(coroutine);
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        public void Update()
        {
            for (var i = 0; i < _coroutines.Count; i++)
            {
                var coroutine = _coroutines[i];
                if (!coroutine.MoveNext())
                {
                    _toRemove.Add(coroutine);
                }
            }

            var len = _toRemove.Count;
            if (len > 0)
            {
                for (var i = 0; i < len; i++)
                {
                    _coroutines.Remove(_toRemove[i]);
                }

                _toRemove.Clear();
            }
        }
    }
}