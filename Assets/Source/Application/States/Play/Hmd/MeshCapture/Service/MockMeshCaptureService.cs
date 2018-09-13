using System.Collections;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Http;
using UnityEngine;

namespace CreateAR.EnkluPlayer
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
        /// Root of all fake meshes.
        /// </summary>
        private GameObject _root;

        /// <summary>
        /// True iff alive.
        /// </summary>
        private bool _isAlive;

        /// <summary>
        /// Backing variable for prop.
        /// </summary>
        private bool _isVisible;
        
        /// <inheritdoc />
        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                _isVisible = value;

                foreach (var surface in _surfaces.Values)
                {
                    surface.gameObject.SetActive(_isVisible);
                }
            }
        }

        /// <inheritdoc />
        public bool IsRunning { get; private set; }

        /// <inheritdoc />
        public IMeshCaptureObserver Observer { get; set; }

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
        public void Start()
        {
            _root = new GameObject("Mesh Capture Root (Mock)");
            
            _bootstrapper.BootstrapCoroutine(Loop());

            IsRunning = true;
        }

        /// <inheritdoc />
        public void Stop()
        {
            IsRunning = false;

            _isAlive = false;

            _surfaces.Clear();
            Object.Destroy(_root);
        }

        /// <summary>
        /// Loops and generates.
        /// </summary>
        private IEnumerator Loop()
        {
            _isAlive = true;

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
                    var fake = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    fake.transform.parent = _root.transform;
                    fake.SetActive(_isVisible);
                    filter = fake.GetComponent<MeshFilter>();

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
                if (null != Observer)
                {
                    Observer.OnData(id, filter);
                }

                // wait
                yield return new WaitForSecondsRealtime(
                    Random.Range(
                        _config.MockGenerateMinMillis,
                        _config.MockGenerateMaxMillis) / 1000f);
            }
        }
    }
}