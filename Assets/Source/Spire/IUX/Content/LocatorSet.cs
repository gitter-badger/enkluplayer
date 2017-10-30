using System;
using System.Collections.Generic;

namespace CreateAR.SpirePlayer
{
    public class LocatorSet
    {
        private readonly List<Locator> _locators = new List<Locator>();

        public event Action<Locator> OnAdded;

        public LocatorSet(IList<HierarchyNodeLocatorData> locators)
        {
            Update(locators);

            if (null == Self())
            {
                throw new ArgumentException("Cannot create LocatorSet without self locator!");
            }
        }

        public Locator Self()
        {
            return ByName("__self__");
        }

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