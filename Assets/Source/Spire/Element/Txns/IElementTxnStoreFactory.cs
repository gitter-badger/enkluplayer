namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Interface for creating <c>IElementTxnStore</c> implementations.
    /// </summary>
    public interface IElementTxnStoreFactory
    {
        /// <summary>
        /// Creates an instance.
        /// </summary>
        /// <param name="strategy">The strategy.</param>
        /// <returns></returns>
        IElementTxnStore Instance(IElementActionStrategy strategy);
    }
}