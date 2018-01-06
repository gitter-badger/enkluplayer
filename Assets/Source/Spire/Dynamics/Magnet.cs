using UnityEngine;

namespace CreateAR.SpirePlayer.Dynamics
{
    public class Magnet : MonoBehaviour
    {
        /// <summary>
        /// To prevent re-tweens with minute target variations.
        /// </summary>
        public float TARGET_TOLERANCE = 0.0001f;

        /// <summary>
        /// To prevent re-tweens with minute target variations.
        /// </summary>
        public float NEAR_TOLERANCE = 0.1f;

        /// <summary>
        /// Fixes up the position of the thing.
        /// </summary>
        public float FixupFactor = 2.0f;

        /// <summary>
        /// Root to transform.
        /// </summary>
        public Transform Root;

        /// <summary>
        /// How fast does this thing move.
        /// </summary>
        public float SpringConstant = 1.0f;

        /// <summary>
        /// Magnet Target.
        /// </summary>
        public Target Target { get { return _target; } }

        /// <summary>
        /// Returns true if the magnet is close to the tween target.
        /// </summary>
        public bool IsAtTarget
        {
            get
            {
                var delta = Root.position - Target.Position;
                var distanceSqr = delta.sqrMagnitude;
                if (distanceSqr < TARGET_TOLERANCE)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Returns true if the magnet is close to the tween target.
        /// </summary>
        public bool IsNearTarget
        {
            get
            {
                var delta = Root.position - Target.Position;
                var distanceSqr = delta.sqrMagnitude;
                if (distanceSqr < NEAR_TOLERANCE)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// returns true if in motion.
        /// </summary>
        public bool IsInMotion
        {
            get
            {
                return !_velocity.Approximately(Vector3.zero, TARGET_TOLERANCE);
            }
        }

        /// <summary>
        /// Frame-based update.
        /// </summary>
        public void Update()
        {
            if (Root == null)
            {
                Root = transform;
            }

            if (_target.Transform != null)
            {
                _target.Position = Target.Transform.position;
            }

            var deltaTime = Time.fixedDeltaTime * Time.timeScale;

            if (!IsInMotion
                && IsAtTarget)
            {
                Root.position = Target.Position;
                _velocity = Vector3.zero;

                enabled = false;
                return;
            }
            else
            {
                var currPosition = Root.position;
                var goalPosition = Target.Position;

                MathUtil
                    .UpdateSpring(
                        currPosition,
                        goalPosition,
                        ref _velocity,
                        SpringConstant,
                        deltaTime);

                var newPosition
                    = currPosition
                        + _velocity
                        * deltaTime;

                Root.position = newPosition;

                var delta = Root.position - Target.Position;
                var distanceSqr = delta.sqrMagnitude;
                var nearToleranceSqr = NEAR_TOLERANCE * NEAR_TOLERANCE;
                if (distanceSqr < nearToleranceSqr)
                {
                    var distance = Mathf.Sqrt(distanceSqr);
                    var lerp = (1.0f - distance / NEAR_TOLERANCE) * FixupFactor;
                    Root.position
                        = Vector3.Lerp(
                            Root.position,
                            Target.Position,
                            lerp);
                }
            }
        }

        /// <summary>
        /// Target Mutator.
        /// </summary>
        public void SetTarget(Target target)
        {
            _target = target;
            if (!IsAtTarget)
            {
                enabled = true;
            }
        }

        /// <summary>
        /// Target Accessor/Mutator
        /// </summary>
        private Target _target;

        /// <summary>
        /// Velocity component
        /// </summary>
        private Vector3 _velocity = Vector3.zero;
    }
}
