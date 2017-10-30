using System;
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
        /// The Theta we used to find the camera's current position. This is
        /// saved off so that we can preview what different Theta look like
        /// in the editor.
        /// </summary>
        private float _bakedTheta;

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
            if (_target == target)
            {
                return;
            }
            
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

            /**
             * General approach:
             * 
             * 1. Find the bounds of the object based on renderers.
             * 2. Construct 8 vectors from camera's current position (O) to the
             * verts of the bounds object.
             * 3. Construct the direction vector, d, from O to the target object.
             * 4. Find which vector has the greatest angle between itself and d.
             * 5. Find R, the point along the line O + td, such that the angle
             * from R to that maximum point is equal to Theta.
             */

            // construct a bounding box of the target
            var renderers = _target.GetComponentsInChildren<Renderer>(true);
            if (0 == renderers.Length)
            {
                _bounds = new Bounds(_target.transform.position, Vector3.one);
            }
            else
            {
                _bounds = renderers[0].bounds;
            }

            for (int i = 1, len = renderers.Length; i < len; i++)
            {
                _bounds.Encapsulate(renderers[i].bounds);
            }

            // calculate target position of camera
            var cameraTransform = Camera.main.transform;
            var targetTransform = _target.transform;

            // knowns
            var O = cameraTransform.position;
            var d = (_bounds.center - cameraTransform.position).normalized;
            
            // collect verts of the Bounds object
            var size = _bounds.size;
            var P = new Vector3[8];
            P[0] = _bounds.min;
            P[1] = _bounds.min + new Vector3(size.x, 0, 0);
            P[2] = _bounds.min + new Vector3(size.x, 0, size.z);
            P[3] = _bounds.min + new Vector3(0, 0, size.z);

            P[4] = _bounds.min + new Vector3(0, size.y, 0);
            P[5] = _bounds.min + new Vector3(size.x, size.y, 0);
            P[6] = _bounds.min + new Vector3(size.x, size.y, size.z);
            P[7] = _bounds.min + new Vector3(0, size.y, size.z);

            // find vert with max theta delta
            var P_max = Vector3.zero;
            var theta_max = float.MinValue;
            for (var i = 0; i < 8; i++)
            {
                var dir = (P[i] - O).normalized;
                var theta_p = Mathf.Deg2Rad * Vector3.Angle(d, dir);
                if (theta_p > theta_max)
                {
                    theta_max = theta_p;
                    P_max = P[i];
                }
            }

            // drop perp from P_max to line defined by O + td and solve for R
            var a = P_max - O;
            var OP_max = Vector3.Dot(d, a) * d;
            var P_perpP_max = P_max - (O + OP_max);
            var P_perp = O + a.magnitude * Mathf.Cos(theta_max) * d;
            var b = P_perpP_max.magnitude / Mathf.Tan(Theta);
            var R = P_perp - b * d;
            
            // TODO: Animate camera to R.
            cameraTransform.position = R;
            cameraTransform.forward = d;

            // save state
            _bakedTheta = Theta;
        }

        /// <summary>
        /// Updates the camera position + the debug drawing.
        /// </summary>
        private void Update()
        {
            if (null != _target
                && Math.Abs(Theta - _bakedTheta) > Mathf.Epsilon)
            {
                UpdateCameraPosition();
            }

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
                var position = _bounds.center;
                handle.Draw(ctx =>
                {
                    ctx.Color(Color.white);
                    ctx.Cube(position, 1f);
                    ctx.Color(Color.green);
                    ctx.Cube(position, 10f);
                    ctx.Color(Color.blue);
                    ctx.Cube(position, 100f);
                    ctx.Color(Color.yellow);
                    ctx.Cube(position, 1000f);

                    ctx.Color(Color.red);
                    ctx.Prism(_bounds);
                });
            }
        }
    }
}