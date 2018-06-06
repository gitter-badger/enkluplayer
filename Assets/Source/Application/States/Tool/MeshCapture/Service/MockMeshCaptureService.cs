using System.Collections;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Http;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Outputs fake data.
    /// </summary>
    public class MockMeshCaptureService : IMeshCaptureService
    {
        /// <summary>
        /// Bootstraps coroutines.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;

        /// <summary>
        /// Configuration.
        /// </summary>
        private readonly MeshCaptureConfig _config;

        /// <summary>
        /// Lookup from surface id to mesh filter.
        /// </summary>
        private readonly Dictionary<int, MeshFilter> _surfaces = new Dictionary<int, MeshFilter>();

        /// <summary>
        /// The observer.
        /// </summary>
        private IMeshCaptureObserver _observer;

        /// <summary>
        /// Root of all fake meshes.
        /// </summary>
        private GameObject _root;

        /// <summary>
        /// True iff alive.
        /// </summary>
        private bool _isAlive;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MockMeshCaptureService(
            IBootstrapper bootstrapper,
            MeshCaptureConfig config)
        {
            _bootstrapper = bootstrapper;
            _config = config;
        }

        /// <inheritdoc />
        public void Start(IMeshCaptureObserver observer)
        {
            _observer = observer;
            _root = new GameObject("Mesh Capture Root (Mock)");

            _bootstrapper.BootstrapCoroutine(Loop());
        }

        /// <inheritdoc />
        public void Stop()
        {
            _isAlive = false;

            _surfaces.Clear();
            Object.Destroy(_root);
        }

        /// <summary>
        /// Loops and generates.
        /// </summary>
        private IEnumerator Loop()
        {
            var acc = 0.9f;
            var ids = 0;

            while (_isAlive)
            {
                int id;
                MeshFilter filter;

                // new surface
                if (0 == ids || Random.value < acc)
                {
                    // create new surface
                    var fake = new GameObject("Fake Mesh Capture");
                    filter = fake.AddComponent<MeshFilter>();
                    fake.transform.parent = _root.transform;

                    id = ids++;
                    _surfaces[id] = filter;

                    // exponentially more likely to see a surface _update_ next time
                    acc *= acc;
                }
                // previous surface
                else
                {
                    id = Random.Range(0, ids);
                    filter = _surfaces[id];
                }
                
                // TODO: generate some triangles for surface

                // pass off
                _observer.OnData(id, filter);

                // wait
                yield return new WaitForSecondsRealtime(
                    Random.Range(
                        _config.MockGenerateMinMillis,
                        _config.MockGenerateMaxMillis) / 1000f);
            }
        }
    }
}