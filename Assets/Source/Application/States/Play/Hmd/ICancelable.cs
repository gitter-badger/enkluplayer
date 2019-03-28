namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Describes an object you can cancel.
    /// </summary>
    public interface ICancelable
    {
        /// <summary>
        /// Cancels the action.
        /// </summary>
        void Cancel();
    }
}