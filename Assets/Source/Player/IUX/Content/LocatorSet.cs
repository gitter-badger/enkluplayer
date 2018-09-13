using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Manages a set of locators.
    /// </summary>
    public class LocatorSet
    {
        /// <summary>
        /// List of locators.
        /// </summary>
        private readonly List<Locator> _locators = new List<Locator>();

        /// <summary>
        /// Called when a locator has been added. Update and Remove events on
        /// are locators themselves.
        /// </summary>
        public event Action<Locator> OnAdded;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="locators">List of locators.</param>
        public LocatorSet(IList<HierarchyNodeLocatorData> locators)
        {
            Update(locators);

            Assert.IsNotNull(Self(), "Created LocatorSet without self locator!");
        }

        /// <summary>
        /// Retrieves the self locator.
        /// </summary>
        /// <returns></returns>
        public Locator Self()
        {
            return ByName("__self__");
        }

        /// <summary>
        /// Retrieves a locator by name.
        /// </summary>
        /// <param name="name">The name of the locator.</param>
        /// <returns></returns>
        public Locator ByName(string name)
        {
            for (int i = 0, len = _locators.Count; i < len; i++)
            {
                var locator = _locators[i];
                if (locator.Data.Name == name)
                {
                    return locator;
                }
            }

            return null;
        }

        /// <summary>
        /// Adds/Updates/Removes locators.
        /// </summary>
        /// <param name="locators">The locators to reconcile with.</param>
        public void Update(IList<HierarchyNodeLocatorData> locators)
        {
            // add or update
            for (int i = 0, len = locators.Count; i < len; i++)
            {
                var data = locators[i];
                var locator = ByName(data.Name);
                if (null == locator)
                {
                    locator = new Locator(data);
                    _locators.Add(locator);

                    if (null != OnAdded)
                    {
                        OnAdded(locator);
                    }
                }
                else
                {
                    locator.Update(data);
                }
            }

            // remove
            for (var i = _locators.Count - 1; i >= 0; i--)
            {
                var locator = _locators[i];
                var name = locator.Data.Name;

                var found = false;
                for (int j = 0, jlen = locators.Count; j < jlen; j++)
                {
                    if (name == locators[j].Name)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    _locators.RemoveAt(i);

                    locator.Removed();
                }
            }
        }
    }
}