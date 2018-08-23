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
        public Action<ElementJs, ElementJs> OnEnter;

        /// <summary>
        /// Invoked every frame when a trigger has entered but not exited a listening element.
        /// The first parameter is the listening element, the second is the trigger.
        /// </summary>
        public Action<ElementJs, ElementJs> OnStay;

        /// <summary>
        /// Invoked when a trigger exits a listening element.
        /// The first parameter is the listening element, the second is the trigger.
        /// </summary>
        public Action<ElementJs, ElementJs> OnExit;

        /// <summary>
        /// Helper class used to represent an element and its current configuration for proximity checking
        /// </summary>
        private class ElementConfig
        {
            /// <summary>
            /// Element to represent
            /// </summary>
            public ElementJs Element;

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
            public ElementConfig A;

            /// <summary>
            /// Configuration for the other Element in a collision.
            /// </summary>
            public ElementConfig B;

            public Collision(ElementConfig a, ElementConfig b)
            {
                A = a;
                B = b;
            }
        }

        /// <summary>
        /// List of all elements that have been updated as listening or a trigger.
        /// </summary>
        private List<ElementConfig> _activeElements = new List<ElementConfig>();

        /// <summary>
        /// List of all collisions, so we can determine if any collision is an extry or a stay event.
        /// </summary>
        private List<Collision> _collisions = new List<Collision>();


        /// <summary>
        /// Updates an Element with regards to its current listening state, and whether it is a trigger or not.
        /// </summary>
        /// <param name="element"></param>
        public void SetElementState(ElementJs element, bool isListening, bool isTrigger)
        {
            // Find an existing config, or make a new one
            ElementConfig config = FindElementConfig(element);
            if (config == null)
            {
                config = new ElementConfig();
                config.Element = element;
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
                int collisionsLen = _collisions.Count;
                for (int i = 0; i < collisionsLen; i++)
                {
                    Collision collision = _collisions[i];
                    if (collision.A.Element == element || collision.B.Element == element)
                    {
                        InvokeCallbacks(OnExit, collision.A, collision.B);
                        _collisions.RemoveAt(i--);
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
        public void SetElementRadii(ElementJs element, float innerRadius, float outerRadius)
        {
            ElementConfig config = FindElementConfig(element);
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
            int elementCount = _activeElements.Count;
            for (int i = 0; i < elementCount; i++)
            {
                for (int j = i + 1; j < elementCount; j++)
                {
                    ElementConfig configA = _activeElements[i];
                    ElementConfig configB = _activeElements[j];

                    // Early out if the collision wouldn't invoke callbacks
                    if (!ShouldProcessCollision(configA, configB)) continue;

                    // Check for prior collision
                    Collision collision = null;
                    foreach (Collision cachedCollision in _collisions)
                    {
                        if ((configA.Element == cachedCollision.A.Element || configA.Element == cachedCollision.B.Element) &&
                            (configB.Element == cachedCollision.A.Element || configB.Element == cachedCollision.B.Element))
                        {
                            collision = cachedCollision;
                            break;
                        }
                    }

                    // Calculate distance
                    float distance = Vec3.Distance(configA.Element.transform.position, configB.Element.transform.position);

                    // Determine proximity change
                    if (collision == null)
                    {
                        // If we're not already in collision, use inner radii to check for enter
                        float radiiSum = configA.InnerRadius + configB.InnerRadius;

                        if (distance - radiiSum < 0)
                        {
                            InvokeCallbacks(OnEnter, configA, configB);
                            _collisions.Add(new Collision(configA, configB));
                        }
                    }
                    else
                    {
                        // Otherwise, use outer to check for exit
                        float radiiSum = configA.OuterRadius + configB.OuterRadius;

                        if (distance - radiiSum < 0)
                        {
                            InvokeCallbacks(OnStay, configA, configB);
                        }
                        else
                        {
                            InvokeCallbacks(OnExit, configA, configB);
                            _collisions.Remove(collision);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finds an ElementConfig for a given Element
        /// </summary>
        /// <param name="element">Element to get Configuration for.</param>
        /// <returns>Valid ElementConfig, or null.</returns>
        private ElementConfig FindElementConfig(ElementJs element)
        {
            int elementsLen = _activeElements.Count;
            for (int i = 0; i < elementsLen; i++)
            {
                if (_activeElements[i].Element == element)
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
        private bool ShouldProcessCollision(ElementConfig a, ElementConfig b)
        {
            // A valid collision requires at least 1 listener & trigger between non-nested Elements
            return ((a.IsListening && b.IsTrigger) || (b.IsListening && a.IsTrigger))
                && !a.Element.isChildOf(b.Element) && !b.Element.isChildOf(a.Element);

        }

        /// <summary>
        /// Invokes a given Action for the provided ElementConfigs, depending on their listener/trigger relationship(s).
        /// </summary>
        /// <param name="action"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        private void InvokeCallbacks(Action<ElementJs, ElementJs> action, ElementConfig a, ElementConfig b)
        {
            if (action == null) return;
            if (a.IsListening && b.IsTrigger) action(a.Element, b.Element);
            if (b.IsListening && a.IsTrigger) action(b.Element, a.Element);
        }
    }
}
