namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Defines an implementation capable of calculating back off values.
    /// </summary>
    public interface IBackOff
    {
        /// <summary>
        /// Calculates the next values in the back off.
        /// </summary>
        double Next();

        /// <summary>
        /// Resets the back off tracking to it's initial state.
        /// </summary>
        void Reset();
    }
}