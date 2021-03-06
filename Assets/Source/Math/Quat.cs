﻿using System;
using Enklu.Data;

namespace CreateAR.EnkluPlayer
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
        /// <param name="threshold">Optional value to use for comparison.</param>
        /// <returns></returns>
        public bool Approximately(Quat lhs, float threshold = float.Epsilon)
        {
            return Math.Abs(x - lhs.x) < threshold
               && Math.Abs(y - lhs.y) < threshold
               && Math.Abs(z - lhs.z) < threshold
               && Math.Abs(w - lhs.w) < threshold;
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
        /// References: http://www.euclideanspace.com/maths/geometry/rotations/conversions/eulerToQuaternion/index.htm
        ///           : https://stackoverflow.com/questions/11492299/quaternion-to-euler-angles-algorithm-how-to-convert-to-y-up-and-between-ha
        /// Notes: Adapted from references. They are in radians
        /// and have mixed coordinate systems / x, y, z, w order
        /// </summary>
        /// <param name="euler">The euler angles in question.</param>
        /// <returns></returns>
        public static Quat Euler(Vec3 euler)
        {
            var radians = (float) Math.PI / 180 * euler;

            // Yaw
            var sy = (float) Math.Sin(radians.x / 2);
            var cy = (float) Math.Cos(radians.x / 2);

            // Pitch
            var sp = (float) Math.Sin(radians.y / 2);
            var cp = (float) Math.Cos(radians.y / 2);

            // Roll
            var sr = (float) Math.Sin(radians.z / 2);
            var cr = (float) Math.Cos(radians.z / 2);

            return new Quat(
                sy * cp * cr + cy * sp * sr,
                cy * sp * cr - sy * cp * sr,
                cy * cp * sr - sy * sp * cr,
                cy * cp * cr + sy * sp * sr
            );
        }

        /// <summary>
        /// Multiples a Vec3 by a Quat.
        ///
        /// References: http://www.euclideanspace.com/maths/algebra/realNormedAlgebra/quaternions/transforms/derivations/vectors/index.htm
        /// </summary>
        /// <param name="rotation"></param>
        /// <param name="forward"></param>
        /// <returns></returns>
        public static Vec3 Mult(Quat rotation, Vec3 forward)
        {
            var xSq2 = rotation.x * rotation.x * 2f;
            var ySq2 = rotation.y * rotation.y * 2f;
            var zSq2 = rotation.z * rotation.z * 2f;
            var xy2 = rotation.x * rotation.y * 2f;
            var xz2 = rotation.x * rotation.z * 2f;
            var yz2 = rotation.y * rotation.z * 2f;
            var wx2 = rotation.w * rotation.x * 2f;
            var wy2 = rotation.w * rotation.y * 2f;
            var wz2 = rotation.w * rotation.z * 2f;
            
            return new Vec3(
                (float) ((1.0 - (ySq2 + zSq2)) * forward.x + (xy2 - wz2) * forward.y + (xz2 + wy2) * forward.z),
                (float) ((xy2 + wz2) * forward.x + (1.0 - (xSq2 + zSq2)) * forward.y + (yz2 - wx2) * forward.z),
                (float) ((xz2 - wy2) * forward.x + (yz2 + wx2) * forward.y + (1.0 - (xSq2 + ySq2)) * forward.z)
            );
        }

        /// <summary>
        /// Returns the rotation from one Vec3 to another Vec3.
        ///
        /// References: https://stackoverflow.com/questions/1171849/finding-quaternion-representing-the-rotation-from-one-vector-to-another
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static Quat FromToRotation(Vec3 from, Vec3 to)
        {
            var dot = Vec3.Dot(from, to);
            var cross = Vec3.Cross(from, to);

            // Edge case 
            if (dot == -1 && cross.MagnitudeSqr == 0)
            {
                return new Quat(1, 0, 0, 0);
            }

            var w = (float) Math.Sqrt(from.MagnitudeSqr * to.MagnitudeSqr) + dot;
            var len = (float) Math.Sqrt(cross.x * cross.x + cross.y * cross.y + cross.z * cross.z + w * w);

            // Normalize the Quat
            Quat rot = new Quat(
                cross.x / len,
                cross.y / len,
                cross.z / len,
                w / len
            );
            return rot;
        }

        /// <summary>
        /// Identity.
        /// </summary>
        public static Quat Identity
        {
            get { return new Quat(0, 0, 0, 1); }
        }

        /// <summary>
        /// Returns the inverse of a Quat.
        /// </summary>
        public static Quat Inverse(Quat q)
        {
            return new Quat(-q.x, -q.y, -q.z, q.w);
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