using System;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer
{
    public class Locator
    {
        public HierarchyNodeLocatorData Data { get; private set; }

        public event Action<Locator> OnUpdated;
        public event Action<Locator> OnRemoved;

        public Locator(HierarchyNodeLocatorData data)
        {
            Data = data;
        }

        public void Update(HierarchyNodeLocatorData data)
        {
            if (null == data)
            {
                throw new ArgumentNullException("data");
            }

            // TODO: Check if changed

            Data = data;

            Log.Info(this, "Locator updated.");

            if (null != OnUpdated)
            {
                OnUpdated(this);
            }
        }

        public void Removed()
        {
            if (null != OnRemoved)
            {
                OnRemoved(this);
            }
        }
    }
}