using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Manages camera focus.
    /// </summary>
    public class FocusManager : MonoBehaviour
    {
        /// <summary>
        /// Defines the target angle between the camera and the points defining
        /// the target's Bounds object.
        /// </summary>
        [Range(0.01f, Mathf.PI / 2f)]
        public float Theta = 0.25f;
        
        /// <summary>
        /// Target of focus.
        /// </summary>
        private GameObject _target;
        
        /// <summary>
        /// Saved off bounds so we can render them.
        /// </summary>
        private Bounds _bounds;
        
        /// <summary>
        /// Focuses on a target.
        /// </summary>
        /// <param name="target">The target to focus on.</param>
        public void Focus(GameObject target)
        {
            _target = target;

            if (null != _target)
            {
                Log.Info(this, "Focus on {0}.", _target.name);

                UpdateCameraPosition();
            }
        }

        /// <summary>
        /// Updates the camera's position based on current parameters, and saves
        /// off parameters.
        /// </summary>
        private void UpdateCameraPosition()
        {
            Log.Info(this, "Update camera focus.");

            // construct a bounding box of the target
            _bounds = GetBounds(_target);

            var delta = _bounds.max - _bounds.min;
            var w = delta.magnitude;

            var mainCamera = Camera.main;
            var fov = mainCamera.fieldOfView * mainCamera.aspect;
            var theta = Mathf.Deg2Rad * (fov / 2f);
            var h = w / 2f;
            var r = h / Mathf.Sin(theta);

            var C = _bounds.center;
            var d = new Vector3(1, 1, 1).normalized;
            var K = C + r * d;

            mainCamera.transform.position = K;
            mainCamera.transform.forward = -d;
        }

        /// <summary>
        /// Retrieves the bounding box of a GameObject's renderers.
        /// </summary>
        /// <param name="instance">GameObject to find bounds for.</param>
        public static Bounds GetBounds(GameObject instance)
        {
            var minx = float.MaxValue; var miny = float.MaxValue; var minz = float.MaxValue;
            var maxx = float.MinValue; var maxy = float.MinValue; var maxz = float.MinValue;

            var verts = new List<Vector3>();
            var filters = instance.GetComponentsInChildren<MeshFilter>();
            foreach (var filter in filters)
            {
                verts.Clear();
                filter.sharedMesh.GetVertices(verts);

                var transform = filter.transform.localToWorldMatrix;
                for (int i = 0, len = verts.Count; i < len; i++)
                {
                    var vert = transform.MultiplyPoint3x4(verts[i]);

                    minx = Mathf.Min(minx, vert.x);
                    miny = Mathf.Min(miny, vert.y);
                    minz = Mathf.Min(minz, vert.z);

                    maxx = Mathf.Max(maxx, vert.x);
                    maxy = Mathf.Max(maxy, vert.y);
                    maxz = Mathf.Max(maxz, vert.z);
                }
            }

            return new Bounds(
                new Vector3(
                    (maxx - minx) / 2f,
                    (maxy - miny) / 2f,
                    (maxz - minz) / 2f),
                new Vector3(
                    maxx - minx,
                    maxy - miny,
                    maxz - minz));
        }

        /// <summary>
        /// Updates the camera position + the debug drawing.
        /// </summary>
        private void Update()
        {
            DebugDraw();
        }

        /// <summary>
        /// Draws the target's bounds.
        /// </summary>
        private void DebugDraw()
        {
            if (null == _target)
            {
                return;
            }

            var handle = Render.Handle("Hierarchy");
            if (null != handle)
            {
                handle.Draw(ctx =>
                {
                    ctx.Color(Color.red);
                    ctx.Prism(_bounds);
                });
            }
        }
    }
}