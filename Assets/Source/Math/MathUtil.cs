using System.Collections.Generic;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public static class MathUtil
    {
        /// <summary>
        /// Returns a new vector created using the euler pitch assuming forward = (0,0,1)
        /// </summary>
        /// <param name="pitch">Euler pitch specified in radians</param>
        /// <returns></returns>
        public static Vector3 FromPitch(float pitch)
        {
            return new Vector3(0, Mathf.Sin(pitch), Mathf.Cos(pitch));
        }

        /// <summary>
        /// Returns a new vector created using the eular yaw assuming forward = (0,0,1)
        /// </summary>
        /// <param name="yaw">Eular yaw specified in radians</param>
        /// <returns></returns>
        public static Vector3 FromYaw(float yaw)
        {
            return new Vector3(Mathf.Sin(yaw), 0, Mathf.Cos(yaw));
        }

        /// <summary>
        /// float point modulous
        /// </summary>
        /// <param name="num"></param>
        /// <param name="denom"></param>
        /// <returns></returns>
        public static float FMod(float num, float denom)
        {
            return num - denom * Mathf.Floor(num / denom);
        }

        /// <summary>
        /// Returns the normalized angle in radians
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static float NormalizeAngle(float radians)
        {
            const float TWO_PI = 2.0f * Mathf.PI;

            var modRadians = FMod(radians, TWO_PI); // [ -2pi .. 2pi ]

            if (!GreaterThanOrApproximately(modRadians, 0.0f))
            {
                modRadians += TWO_PI; // [ 0 .. 2pi ]
            }

            return modRadians;
        }

        /// <summary>
        /// Returns the difference in angle between two angles
        /// </summary>
        /// <param name="fromRadians"></param>
        /// <param name="toRadians"></param>
        /// <returns></returns>
        public static float DeltaRadians(float fromRadians, float toRadians)
        {
            const float TWO_PI = Mathf.PI * 2.0f;

            var toNormalized = NormalizeAngle(toRadians);
            var fromNormalized = NormalizeAngle(fromRadians);
            var deltaAngle = toNormalized - fromNormalized;

            var deltaAngleNormalized = NormalizeAngle(deltaAngle);

            if (deltaAngleNormalized < -Mathf.PI)
            {
                deltaAngleNormalized += TWO_PI;
            }
            if (deltaAngleNormalized > Mathf.PI)
            {
                deltaAngleNormalized -= TWO_PI;
            }

            return deltaAngleNormalized;
        }

        /// <summary>
        /// Lerps two angles
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="terp"></param>
        /// <returns></returns>
        public static float LerpAngle(float from, float to, float terp)
        {
            float deltaAngle = DeltaRadians(from, to);
            return Mathf.Lerp(from, from + deltaAngle, terp);
        }

        /// <summary>
        /// Returns the yaw in radians of a vector
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static float Yaw(Vector3 direction)
        {
            return Mathf.Atan2(direction.x, direction.z);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector3"></param>
        /// <returns></returns>
        public static float Pitch(Vector3 direction)
        {
            return Mathf.Atan2(direction.y, direction.ToXz().magnitude);
        }

        /// <summary>
        /// Returns the yaw in radians of a vector
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static float Yaw(Vector3 from, Vector3 to)
        {
            return DeltaRadians(Yaw(from), Yaw(to));
        }

        /// <summary>
        /// Awesome extension method
        /// </summary>
        /// <param name="line"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool GetClosestPoint(this List<Vector3> line, Vector3 point, ref Vector3 closestPoint, ref float closestPointDistSq, ref int lineIndex)
        {
            bool updatedThePoint = false;

            for (int i = 1; i < line.Count; ++i)
            {
                Vector3 p0 = line[i - 1];
                Vector3 p1 = line[i];

                Vector3 p0top1 = p1 - p0;
                float p0top1Mag = p0top1.magnitude;
                if (p0top1Mag > Mathf.Epsilon)
                {
                    Vector3 p0top1Norm = p0top1 / p0top1Mag;
                    Vector3 p0toPoint = point - p0;
                    float p0toPointMag = p0toPoint.magnitude;
                    if (p0toPointMag < Mathf.Epsilon)
                    {
                        // the point is exactly on this line segment
                        closestPointDistSq = 0;
                        closestPoint = point;
                        lineIndex = i;
                        return true;
                    }
                    else
                    {
                        Vector3 p0toPointNorm = p0toPoint / p0toPointMag;
                        float t = Mathf.Clamp(Vector3.Dot(p0top1Norm, p0toPointNorm), 0.0f, 1.0f) * Mathf.Clamp(p0toPointMag, 0.0f, p0top1Mag);
                        Vector3 pointOnLine = p0 + p0top1Norm * t;
                        float distToPointSq = (pointOnLine - point).sqrMagnitude;
                        if (distToPointSq < closestPointDistSq)
                        {
                            closestPointDistSq = distToPointSq;
                            closestPoint = pointOnLine;
                            lineIndex = i;
                            updatedThePoint = true;
                        }
                    }
                }
            }

            return updatedThePoint;
        }

        /// <summary>
        /// Awesome extension method
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static float GetLength(this List<Vector3> line)
        {
            float length = 0;
            for (int i = 1; i < line.Count; ++i)
            {
                length += (line[i] - line[i - 1]).magnitude;
            }

            return length;
        }

        /// <summary>
        /// Returns the distance from the line segment to the point
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static float DistanceToSegmentSqr(Vector2 p0, Vector2 p1, Vector2 query)
        {
            if (Mathf.Approximately(p0.x, p1.x)
                && Mathf.Approximately(p0.y, p1.y))
            {
                return (query - p0).sqrMagnitude;
            }

            Vector2 p0p1 = p1 - p0;
            Vector2 p0qp = query - p0;

            float len2 = Vector2.Dot(p0p1, p0p1);
            if (len2 < Mathf.Epsilon)
            {
                return 0;
            }

            // t is a number in [0,1] describing 
            // the closest point on the lineseg as a blend of endpoints.. 
            float t = Mathf.Max(0, Mathf.Min(len2, Vector2.Dot(p0p1, p0qp))) / len2;

            // cp is the position (i.e actual coordinates) of the closest point on the seg
            Vector2 cp = p0 + t * p0p1;
            return (cp - query).sqrMagnitude;
        }

        /// <summary>
        /// Intersection
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="intersection"></param>
        /// <returns></returns>
        public static bool Intersects(
            Vector2 p0,
            Vector2 p1,
            Vector2 center,
            float radius,
            out Vector2 intersection)
        {
            var d = p1 - p0;
            var f = p0 - center;

            float a = d.Dot(d);
            float b = 2 * f.Dot(d);
            float c = f.Dot(f) - radius * radius;

            float discriminant = b * b - 4 * a * c;
            if (discriminant < 0)
            {
                // no intersection
                intersection = Vector2.zero;
                return false;
            }

            // ray didn't totally miss sphere,
            // so there is a solution to
            // the equation.

            discriminant = Mathf.Sqrt(discriminant);

            // either solution may be on or off the ray so need to test both
            // t1 is always the smaller value, because BOTH discriminant and
            // a are nonnegative.
            float t1 = (-b - discriminant) / (2 * a);
            float t2 = (-b + discriminant) / (2 * a);

            // 3x HIT cases:
            //          -o->             --|-->  |            |  --|->
            // Impale(t1 hit,t2 hit), Poke(t1 hit,t2>1), ExitWound(t1<0, t2 hit), 

            // 3x MISS cases:
            //       ->  o                     o ->              | -> |
            // FallShort (t1>1,t2>1), Past (t1<0,t2<0), CompletelyInside(t1<0, t2>1)

            if (t1 >= 0 && t1 <= 1)
            {
                // t1 is the intersection, and it's closer than t2
                // (since t1 uses -b - discriminant)
                // Impale, Poke
                intersection = p0 + d * t1;
                return true;
            }

            // here t1 didn't intersect so we are either started
            // inside the sphere or completely past it
            if (t2 >= 0 && t2 <= 1)
            {
                // ExitWound
                intersection = p0 + d * t2;
                return true;
            }

            // no intn: FallShort, Past, CompletelyInside
            intersection = Vector2.zero;
            return false;
        }

        /// <summary>
        /// Used for floating point comparisons
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool LessThanOrApproximately(float lhs, float rhs)
        {
            float diff = rhs - lhs;
            return diff > -Mathf.Epsilon;
        }

        /// <summary>
        /// Used for floating point comparisons
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool LessThanOrApproximately(double lhs, double rhs)
        {
            double diff = rhs - lhs;
            return diff > -Mathf.Epsilon;
        }

        /// <summary>
        /// Returns true if lhs is >= to rhs. uses epsilon to make up for floating point inaccuracies
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool GreaterThanOrApproximately(float lhs, float rhs)
        {
            float diff = lhs - rhs;
            return diff > -Mathf.Epsilon;
        }

        /// <summary>
        /// Returns true if lhs is >= to rhs. uses epsilon to make up for floating point inaccuracies
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool GreaterThanOrApproximately(double lhs, double rhs)
        {
            double diff = lhs - rhs;
            return diff > -Mathf.Epsilon;
        }

        /// <summary>
        /// Checks intersection between two segments
        /// </summary>
        /// <param name="a0"></param>
        /// <param name="a1"></param>
        /// <param name="b0"></param>
        /// <param name="b1"></param>
        /// <param name="collisionOut"></param>
        /// <returns></returns>
        public static bool IntersectSegments(Vector2 a0, Vector2 a1, Vector2 b0, Vector2 b1, out Vector2 collisionOut)
        {
            // looking for p + tr = q + us
            Vector2 r = a1 - a0;
            Vector2 s = b1 - b0;
            float rCrossS = r.Cross(s);
            if (Mathf.Approximately(rCrossS, 0))
            {
                // lines are parallel
                if ((b0 - a0).Cross(r) == 0)
                {
                    // colinear
                    collisionOut = a0;
                    return true;
                }

                // never intersect
                collisionOut = Vector2.zero;
                return false;
            }

            Vector2 pToQ = b0 - a0;
            float rCrossSInverse = 1.0f / rCrossS;
            float t = pToQ.Cross(s) * rCrossSInverse;
            float u = pToQ.Cross(r) * rCrossSInverse;

            if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
            {
                collisionOut = a0 + r * t;
                return true;
            }

            collisionOut = Vector2.zero;
            return false;
        }

        /// <summary>
        /// Critically dampened spring equation
        /// </summary>
        /// <param name="curr"></param>
        /// <param name="goal"></param>
        /// <param name="velocity"></param>
        /// <param name="springConstant"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public static float UpdateSpring(
            float curr,
            float goal,
            ref float velocity,
            float springConstant,
            float deltaTime)
        {
            var dampingConstant = CalculateCriticalDampingConstant(springConstant);

            return UpdateSpring(
                curr,
                goal,
                ref velocity,
                springConstant,
                dampingConstant,
                deltaTime);
        }

        /// <summary>
        /// Critically dampened spring equation
        /// </summary>
        /// <param name="curr"></param>
        /// <param name="goal"></param>
        /// <param name="velocity"></param>
        /// <param name="springConstant"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public static Vector3 UpdateSpringDegrees(
            Vector3 curr,
            Vector3 goal,
            ref Vector3 velocity,
            float springConstant,
            float deltaTime)
        {
            var dampingConstant = CalculateCriticalDampingConstant(springConstant);

            return UpdateSpringDegrees(
                curr,
                goal,
                ref velocity,
                springConstant,
                dampingConstant,
                deltaTime);
        }

        /// <summary>
        /// Critically dampened spring equation
        /// </summary>
        /// <param name="curr"></param>
        /// <param name="goal"></param>
        /// <param name="velocity"></param>
        /// <param name="springConstant"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public static float UpdateSpringDegrees(
            float curr,
            float goal,
            ref float velocity,
            float springConstant,
            float deltaTime)
        {
            var dampingConstant = CalculateCriticalDampingConstant(springConstant);

            return UpdateSpringDegrees(
                curr,
                goal,
                ref velocity,
                springConstant,
                dampingConstant,
                deltaTime);
        }

        /// <summary>
        /// Critically dampened spring equation
        /// </summary>
        /// <param name="curr"></param>
        /// <param name="goal"></param>
        /// <param name="velocity"></param>
        /// <param name="springConstant"></param>
        /// <param name="dampingConstant"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public static Vector3 UpdateSpringDegrees(
            Vector3 curr,
            Vector3 goal,
            ref Vector3 velocity,
            float springConstant,
            float dampingConstant,
            float deltaTime)
        {
            var goalRadians = Mathf.Deg2Rad * goal;
            var currRadians = Mathf.Deg2Rad * curr;
            var deltaRadians
                = new Vector3(
                    DeltaRadians(currRadians.x, goalRadians.x),
                    DeltaRadians(currRadians.y, goalRadians.y),
                    DeltaRadians(currRadians.z, goalRadians.z));
            var deltaDegrees
                = deltaRadians
                  * Mathf.Rad2Deg;

            var accelerationDegrees
                = dampingConstant * velocity
                  + springConstant * deltaDegrees;
            velocity += accelerationDegrees * deltaTime;

            //Trace.Error(null,
            //    "Curr[(d:{0})(r:{1})], Goal[(d:{2})(r:{3})], Delta[(d:{4})(r:{5})], Acc[{6}], Vel[{7}]",
            //    curr,
            //    currRadians,
            //    goal,
            //    goalRadians,
            //    deltaDegrees,
            //    deltaRadians,
            //    accelerationDegrees,
            //    velocity);

            var stepDegrees = velocity * deltaTime;

            return curr + stepDegrees;
        }

        /// <summary>
        /// Critically dampened spring equation
        /// </summary>
        /// <param name="curr"></param>
        /// <param name="goal"></param>
        /// <param name="velocity"></param>
        /// <param name="springConstant"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public static float UpdateSpringDegrees(
            float curr,
            float goal,
            ref float velocity,
            float springConstant,
            float dampingConstant,
            float deltaTime)
        {
            var goalRadians = Mathf.Deg2Rad * goal;
            var currRadians = Mathf.Deg2Rad * curr;
            var deltaRadians = DeltaRadians(currRadians, goalRadians);
            var deltaDegrees = deltaRadians * Mathf.Rad2Deg;

            var accelerationDegrees
                = dampingConstant * velocity
                  + springConstant * deltaDegrees;
            velocity += accelerationDegrees * deltaTime;

            //Trace.Error(null,
            //    "Curr[(d:{0})(r:{1})], Goal[(d:{2})(r:{3})], Delta[(d:{4})(r:{5})], Acc[{6}], Vel[{7}]",
            //    curr,
            //    currRadians,
            //    goal,
            //    goalRadians,
            //    deltaDegrees,
            //    deltaRadians,
            //    accelerationDegrees,
            //    velocity);

            var stepDegrees = velocity * deltaTime;

            return curr + stepDegrees;
        }

        /// <summary>
        /// Critically dampened spring equation
        /// </summary>
        /// <param name="curr"></param>
        /// <param name="goal"></param>
        /// <param name="velocity"></param>
        /// <param name="springConstant"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public static Vector3 UpdateSpring(
            Vector3 curr,
            Vector3 goal,
            ref Vector3 velocity,
            float springConstant,
            float deltaTime)
        {
            var dampingConstant = CalculateCriticalDampingConstant(springConstant);

            return UpdateSpring(
                curr,
                goal,
                ref velocity,
                springConstant,
                dampingConstant,
                deltaTime);
        }

        /// <summary>
        /// Critical Damping Constant
        /// </summary>
        /// <param name="springConstant"></param>
        /// <returns></returns>
        public static float CalculateCriticalDampingConstant(float springConstant)
        {
            return -2.0f * Mathf.Sqrt(springConstant);
        }

        /// <summary>
        /// Critically dampened spring equation
        /// </summary>
        /// <param name="curr"></param>
        /// <param name="goal"></param>
        /// <param name="velocity"></param>
        /// <param name="springConstant"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public static Vector3 UpdateSpring(
            Vector3 curr,
            Vector3 goal,
            ref Vector3 velocity,
            float springConstant,
            float dampingConstant,
            float deltaTime)
        {
            var offset = goal - curr;
            var acceleration
                = dampingConstant * velocity
                  + springConstant * offset;
            velocity += acceleration * deltaTime;
            var delta = velocity * deltaTime;
            return curr + delta;
        }

        /// <summary>
        /// Critically dampened spring equation
        /// </summary>
        /// <param name="curr"></param>
        /// <param name="goal"></param>
        /// <param name="velocity"></param>
        /// <param name="springConstant"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public static float UpdateSpring(
            float curr,
            float goal,
            ref float velocity,
            float springConstant,
            float dampingConstant,
            float deltaTime)
        {
            var offset = goal - curr;
            var acceleration
                = dampingConstant * velocity
                  + springConstant * offset;
            velocity += acceleration * deltaTime;
            var delta = velocity * deltaTime;
            return curr + delta;
        }
    }
}
