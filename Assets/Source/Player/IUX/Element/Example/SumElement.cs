using System;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Example element that sums child elements by their "value" property.
    /// </summary>
    public class SumElement : Element
    {
        /// <summary>
        /// Integral sum.
        /// </summary>
        public int Sum { get; private set; }
        
        /// <summary>
        /// Called when the sum changes.
        /// </summary>
        public event Action OnSumChanged;

        /// <inheritdoc />
        protected override void LoadInternalAfterChildren()
        {
            base.LoadInternalAfterChildren();

            SumChildren();

            OnDescendentAdded += ThisOnDescendentUpdated;
            OnDescendentRemoved += ThisOnDescendentUpdated;
        }

        /// <inheritdoc />
        protected override void UnloadInternalBeforeChildren()
        {
            base.UnloadInternalBeforeChildren();

            OnDescendentAdded -= ThisOnDescendentUpdated;
            OnDescendentRemoved -= ThisOnDescendentUpdated;
        }

        /// <summary>
        /// Sums children by their "value" property.
        /// </summary>
        private void SumChildren()
        {
            var total = 0;
            foreach (var child in Children)
            {
                total += child.Schema.Get<int>("value").Value;
            }

            if (Sum != total)
            {
                Sum = total;

                if (OnSumChanged != null)
                {
                    OnSumChanged();
                }
            }
        }

        /// <summary>
        /// Called when a child has been added or removed.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="child">The child.</param>
        private void ThisOnDescendentUpdated(Element parent, Element child)
        {
            if (parent == this)
            {
                SumChildren();
            }
        }
    }
}
