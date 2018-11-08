using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Simple interface for exposing some basic Physics functionality.
    /// Currently supporting Raycasting, and only a subset of that.
    /// </summary>
    [JsInterface("physics")]
    public class PhysicsJsInterface
    {
        /// <summary>
        /// Raycasts from <c>start</c> towards <c>dir</c> checking
        /// for collision against a ContentWidget.
        ///
        /// Note: Returns an object so that both null and a Vec3 can be returned.
        /// Not the best C# practice, but provides some nice JavaScript usage.
        ///
        /// TODO: Enable a way to configure relative world space?! Designed for untethered experiences.
        /// </summary>
        /// <param name="start">Start point</param>
        /// <param name="dir">Cast direction</param>
        /// <param name="elementJs">Cast target</param>
        /// <returns></returns>
        public object raycast(Vec3 start, Vec3 dir, ElementJs elementJs)
        {
            var target = elementJs as ContentElementJs;
            if (target == null)
            {
                Log.Error(this, "Target element is not a ContentWidget");
                return null;
            }

            var collider = target.ContentWidget.GetComponent<Collider>();
            if (!collider)
            {
                Log.Error(this, "Element has no Collider");
                return null;
            }

            var ray = new Ray(start.ToVector(), dir.ToVector());
            RaycastHit hit;
            if (!collider.Raycast(ray, out hit, Mathf.Infinity))
            {
                return null;
            }
            
            return hit.point.ToVec();
        }
    }
}