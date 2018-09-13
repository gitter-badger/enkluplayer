namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Described an object that the IUIManager implementation can manage.
    /// </summary>
    public interface IUIElement
    {
        /// <summary>
        /// Id in the stack.
        /// </summary>
        int StackId { get; }

        /// <summary>
        /// Called when the element has first been created.
        /// </summary>
        void Created();

        /// <summary>
        /// Called when the element has been added to the stack.
        /// </summary>
        void Added();

        /// <summary>
        /// Called when the element has been revealed on the top of the stack.
        /// </summary>
        void Revealed();

        /// <summary>
        /// Called when the element has been covered by another element.
        /// </summary>
        void Covered();

        /// <summary>
        /// Called when the element has been removed from the stack.
        /// </summary>
        void Removed();
    }
}