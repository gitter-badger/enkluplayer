using System;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Represents a point in space with orientation and scale.
    /// </summary>
    public class Locator
    {
        /// <summary>
        /// Underlying data model.
        /// </summary>
        public HierarchyNodeLocatorData Data { get; private set; }

        /// <summary>
        /// Called when locator has been updated.
        /// </summary>
        public event Action<Locator> OnUpdated;

        /// <summary>
        /// Called when locator has been removed.
        /// </summary>
        public event Action<Locator> OnRemoved;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="data">The data.</param>
        public Locator(HierarchyNodeLocatorData data)
        {
            Data = data;
        }

        /// <summary>
        /// Updates the underlying data.
        /// </summary>
        /// <param name="data">Data.</param>
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

        /// <summary>
        /// Call when locator is removed. This method should be called internally
        /// only.
        /// </summary>
        public void Removed()
        {
            if (null != OnRemoved)
            {
                OnRemoved(this);
            }
        }
    }
}