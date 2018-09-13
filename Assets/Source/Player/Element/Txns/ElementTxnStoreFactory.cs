namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Creates <c>ElementTxnStore</c> instances.
    /// </summary>
    public class ElementTxnStoreFactory : IElementTxnStoreFactory
    {
        /// <inheritdoc />
        public IElementTxnStore Instance(IElementActionStrategy strategy)
        {
            return new ElementTxnStore(strategy);
        }
    }
}