namespace CreateAR.SpirePlayer.Vine
{
    /// <summary>
    /// Interface for vine preprocessing.
    /// </summary>
    public interface IVinePreProcessor
    {
        /// <summary>
        /// Transforms an input stream.
        /// </summary>
        /// <param name="data">Source data.</param>
        /// <returns></returns>
        string Execute(string data);
    }
}