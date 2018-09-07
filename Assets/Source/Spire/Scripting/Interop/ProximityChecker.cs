using CreateAR.Commons.Unity.Logging;
using System;
using System.Collections.Generic;

namespace CreateAR.SpirePlayer.Scripting
{
    /// <summary>
    /// Responsible for calculating distances between elements and emitting proximity events as needed.
    /// Elements are tracked when their state is updated, and proximity events are based on their set radii values.
    /// 
    /// Elements have a distinct difference between "listening" and being a "trigger". This is so that multiple listeners
    ///     can all be in a scene overlapping each other, but not constantly emitting events off of each other. Triggers
    ///     colliding with listening elements cause proximity events. An element can be listening and/or a trigger.
    /// </summary>
    public class ProximityChecker
    {
        /// <summary>
        /// Invoked when a trigger comes in range of a listening element. 
        /// The first parameter is the listening element, the second is the trigger.
        /// </summary>
        public Action<IEntityJs, IEntityJs> OnEnter;

        /// <summary>
        /// Invoked every frame when a trigger has entered but not exited a listening element.
        /// The first parameter is the listening element, the second is the trigger.
        /// </summary>
        public Action<IEntityJs, IEntityJs> OnStay;

        /// <summary>
        /// Invoked when a trigger exits a listening element.
        /// The first parameter is the listening element, the second is the trigger.
        /// </summary>
        public Action<IEntityJs, IEntityJs> OnExit;

        /// <summary>
        /// Helper class used to represent an element and its current configuration for proximity checking
        /// </summary>
        private class EntityConfig
        {
            /// <summary>
            /// Element to represent
            /// </summary>
            public IEntityJs Element;

            /// <summary>
            /// Whether the Element should react to triggers
            /// </summary>
            public bool IsListening;

            /// <summary>
            /// Whether the Element should trigger proximity events with listeners
            /// </summary>
            public bool IsTrigger;

            /// <summary>
            /// Radius where triggers will cause enter events
            /// </summary>
            public float InnerRadius;
            
            /// <summary>
            /// Radius where colliding triggers will cause exit events
            /// </summary>
            public float OuterRadius;
        }

        /// <summary>
        /// Helper class used to represent ongoing collisions. This is used to determine if events should be enters vs stays.
        /// Stored ElementConfigs are not directional.
        /// </summary>
        private class Collision
        {
            /// <summary>
            /// Configuration for one Element in a collision.
            /// </summary>
            public EntityConfig A;

            /// <summary>
            /// Configuration for the other Element in a collision.
            /// </summary>
            public EntityConfig B;

            public Collision(EntityConfig a, EntityConfig b)
            {
                A = a;
                B = b;
            }
        }

        /// <summary>
        /// List of all elements that have been updated as listening or a trigger.
        /// </summary>
        private readonly List<EntityConfig> _activeElements = new List<EntityConfig>();

        /// <summary>
        /// List of all collisions, so we can determine if any collision is an extry or a stay event.
        /// </summary>
        private readonly List<Collision> _collisions = new List<Collision>();

        /// <summary>
        /// Updates an Element with regards to its current listening state, and whether it is a trigger or not.
        /// </summary>
        /// <param name="element">The element to update.</param>
        /// <param name="isListening">True iff the element should receive events.</param>
        /// <param name="isTrigger">True iff the element should trigger events.</param>
        public void SetElementState(IEntityJs element, bool isListening, bool isTrigger)
        {
            // Find an existing config, or make a new one
            var config = FindElementConfig(element);
            if (config == null)
            {
                // Just early out if it wouldn't be tracked anyway. This occurs commonly during scene building
                if (!isListening && !isTrigger)
                {
                    return;
                }

                config = new EntityConfig
                {
                    Element = element
                };
                _activeElements.Add(config);
            }

            // Update values
            config.IsListening = isListening;
            config.IsTrigger = isTrigger;

            // Remove if needed
            if (!config.IsListening && !config.IsTrigger)
            {
                _activeElements.Remove(config);

                // Remove any active collisions
                var collisionsLen = _collisions.Count;
                for (var i = 0; i < collisionsLen; i++)
                {
                    var collision = _collisions[i];
                    if (collision.A.Element == element || collision.B.Element == element)
                    {
                        _collisions.RemoveAt(i--);

                        InvokeCallbacks(OnExit, collision.A, collision.B);
                    }
                }
            }
        }

        /// <summary>
        /// Updates an Elements' radii values
        /// </summary>
        /// <param name="element">Element to update</param>
        /// <param name="innerRadius">Radius that'll trigger enter events</param>
        /// <param name="outerRadius">Raadius that'll trigger exit events</param>
        public void SetElementRadii(IEntityJs element, float innerRadius, float outerRadius)
        {
            var config = FindElementConfig(element);
            if (config != null)
            {
                config.InnerRadius = Math.Max(0, innerRadius);
                config.OuterRadius = Math.Max(innerRadius + 0.25f, outerRadius);
            }
            else
            {
                Log.Warning(this, "Attempting to set radii for untracked element");
            }
        }

        /// <summary>
        /// Checks whether any proximity events should be dispatched or not.
        /// </summary>
        public void Update()
        {
            var elementCount = _activeElements.Count;
            for (var i = 0; i < elementCount; i++)
            {
                for (var j = i + 1; j < elementCount; j++)
                {
                    var configA = _activeElements[i];
                    var configB = _activeElements[j];

                    // Early out if the collision wouldn't invoke callbacks
                    if (!ShouldProcessCollision(configA, configB))
                    {
                        continue;
                    }

                    // Check for prior collision
                    Collision collision = null;
                    var collisionsCount = _collisions.Count;
                    for (var k = 0; k < collisionsCount; k++)
                    {
                        var cachedCollision = _collisions[k];
                        if ((configA.Element == cachedCollision.A.Element || configA.Element == cachedCollision.B.Element) &&
                            (configB.Element == cachedCollision.A.Element || configB.Element == cachedCollision.B.Element))
                        {
                            collision = cachedCollision;
                            break;
                        }
                    }

                    // Calculate distance
                    var distanceSq = Vec3.DistanceHorizontalSqr(configA.Element.transform.position, configB.Element.transform.position);

                    // Determine proximity change
                    if (collision == null)
                    {
                        // If we're not already in collision, use inner radii to check for enter
                        var radiiSumSq = (float) Math.Pow(configA.InnerRadius + configB.InnerRadius, 2);
                        if (distanceSq - radiiSumSq < 0)
                        {
                            Log.Info(this, "No existing collision, so dispatching enter event.");

                            _collisions.Add(new Collision(configA, configB));

                            InvokeCallbacks(OnEnter, configA, configB);
                        }
                    }
                    else
                    {
                        // Otherwise, use outer to check for exit
                        var radiiSum = (float) Math.Pow(configA.OuterRadius + configB.OuterRadius, 2);
                        if (distanceSq - radiiSum < 0)
                        {
                            InvokeCallbacks(OnStay, configA, configB);
                        }
                        else
                        {
                            Log.Info(this, "Collision exists already but we're leaving the area, so dispatch an exit.");

                            _collisions.Remove(collision);

                            InvokeCallbacks(OnExit, configA, configB);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Teardown this instance. Clears callback Actions
        /// </summary>
        public void TearDown()
        {
            OnEnter = null;
            OnStay = null;
            OnExit = null;
        }

        /// <summary>
        /// Finds an ElementConfig for a given Element
        /// </summary>
        /// <param name="element">Element to get Configuration for.</param>
        /// <returns>Valid ElementConfig, or null.</returns>
        private EntityConfig FindElementConfig(IEntityJs entity)
        {
            // TODO: Remove this abomination when there's one JS Engine for everything.
            //  without casting to ElementJs, ElementJs' custom equality operators never get run
            //  and different ElementJs instances that point to the same Element will cause duplicate
            //  configurations, leading to multiple enter/exit events per Element.
            var entityAsElement = entity as ElementJs;
            var useCast = entityAsElement != null;

            var elementsLen = _activeElements.Count;
            for (var i = 0; i < elementsLen; i++)
            {
                bool equivalent = false;
                if (useCast)
                {
                    equivalent = entityAsElement == _activeElements[i].Element;
                }
                else 
                {
                    equivalent = entity == _activeElements[i].Element;
                }

                if (equivalent)
                {
                    return _activeElements[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Determines if two ElementConfigs are a valid combination capable of invoking proximity events
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private bool ShouldProcessCollision(EntityConfig a, EntityConfig b)
        {
            // A valid collision requires at least 1 listener & trigger between non-nested Elements
            var validCollision = (a.IsListening && b.IsTrigger) || (b.IsListening && a.IsTrigger);

            // A little gross, would be nice to fully remove ElementJs knowledge. But ensure the camera won't pass this check
            var aChildOfB = a.Element.isChildOf(b.Element);
            var bChildOfA = b.Element.isChildOf(a.Element);
            var hierarchyOkay = !aChildOfB && !bChildOfA;

            return validCollision && hierarchyOkay;
        }

        /// <summary>
        /// Invokes a given Action for the provided ElementConfigs, depending on their listener/trigger relationship(s).
        /// </summary>
        /// <param name="action"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        private void InvokeCallbacks(Action<IEntityJs, IEntityJs> action, EntityConfig a, EntityConfig b)
        {
            if (action == null)
            {
                return;
            }

            if (a.IsListening && b.IsTrigger)
            {
                action(a.Element, b.Element);
            }

            if (b.IsListening && a.IsTrigger)
            {
                action(b.Element, a.Element);
            }
        }
    }
}
