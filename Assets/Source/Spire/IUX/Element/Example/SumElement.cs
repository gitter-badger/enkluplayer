using System;

namespace CreateAR.SpirePlayer.IUX
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

            OnChildAdded += This_OnChildUpdated;
            OnChildRemoved += This_OnChildUpdated;
        }

        /// <inheritdoc />
        protected override void UnloadInternalBeforeChildren()
        {
            base.UnloadInternalBeforeChildren();

            OnChildAdded -= This_OnChildUpdated;
            OnChildRemoved -= This_OnChildUpdated;
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
        private void This_OnChildUpdated(Element parent, Element child)
        {
            if (parent == this)
            {
                SumChildren();
            }
        }
    }
}
