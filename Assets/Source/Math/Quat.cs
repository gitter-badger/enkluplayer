using System;
using System.Net.NetworkInformation;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Simple quaternion class.
    /// </summary>
    public struct Quat
    {
        /// <summary>
        /// X component.
        /// </summary>
        public float x;

        /// <summary>
        /// Y component.
        /// </summary>
        public float y;

        /// <summary>
        /// Z component.
        /// </summary>
        public float z;
        
        /// <summary>
        /// W component.
        /// </summary>
        public float w;

        /// <summary>
        /// Creates a Quat from a Quat.
        /// </summary>
        /// <param name="quat">Source quat.</param>
        public Quat(Quat quat)
        {
            x = quat.x;
            y = quat.y;
            z = quat.z;
            w = quat.w;
        }

        /// <summary>
        /// Creates a Quat from components.
        /// </summary>
        /// <param name="x">X component.</param>
        /// <param name="y">Y component.</param>
        /// <param name="z">Z component.</param>
        /// <param name="w">W component.</param>
        public Quat(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        /// <summary>
        /// Sets the components of this Quat from values.
        /// </summary>
        /// <param name="x">X component.</param>
        /// <param name="y">Y component.</param>
        /// <param name="z">Z component.</param>
        /// <param name="w">W component.</param>
        /// <returns></returns>
        public Quat Set(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
            return this;
        }
        
        /// <summary>
        /// Returns true when component-wise approximately equal.
        /// </summary>
        /// <param name="lhs">Another vec3.</param>
        /// <returns></returns>
        public bool Approximately(Quat lhs)
        {
            return Math.Abs(x - lhs.x) < float.Epsilon
               && Math.Abs(y - lhs.y) < float.Epsilon
               && Math.Abs(z - lhs.z) < float.Epsilon
               && Math.Abs(w - lhs.w) < float.Epsilon;
        }

        /// <summary>
        /// Creates a Quat from Euler angles.
        /// </summary>
        /// <param name="x">Heading.</param>
        /// <param name="y">Attitude.</param>
        /// <param name="z">Bank.</param>
        /// <returns></returns>
        public static Quat Euler(float x, float y, float z)
        {
            return Euler(new Vec3(x, y, z));
        }
        
        /// <summary>
        /// Create a Quat from Euler angles.
        /// 
        /// From: http://www.euclideanspace.com/maths/geometry/rotations/conversions/eulerToQuaternion/index.htm
        /// </summary>
        /// <param name="euler">The euler angles in querstion.</param>
        /// <returns></returns>
        public static Quat Euler(Vec3 euler)
        {
            var c1 = Math.Cos(euler.x / 2.0);
            var s1 = Math.Sin(euler.x / 2.0);
            
            var c2 = Math.Cos(euler.y / 2.0);
            var s2 = Math.Sin(euler.y / 2.0);
            
            var c3 = Math.Cos(euler.z / 2.0);
            var s3 = Math.Sin(euler.z / 2.0);
            
            var c1c2 = c1 * c2;
            var s1s2 = s1 * s2;
            
            return new Quat(
                (float) (c1c2 * c3 - s1s2 * s3),
                (float) (c1c2 * s3 + s1s2 * c3),
                (float) (s1 * c2 * c3 + c1 * s2 * s3),
                (float) (c1 * s2 * c3 - s1 * c2 * s3));
        }

        /// <summary>
        /// Identity.
        /// </summary>
        public static Quat Identity
        {
            get { return new Quat(1, 1, 1, 1); }
        }

        /// <summary>
        /// Quat string representation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0:0.00}, {1:0.00}, {2:0.00}, {3:0.00}", x, y, z, w);
        }
    }
}