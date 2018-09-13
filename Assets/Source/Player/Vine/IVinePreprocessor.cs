using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer.Vine
{
    /// <summary>
    /// Interface for vine preprocessing.
    /// </summary>
    public interface IVinePreProcessor
    {
        /// <summary>
        /// Sets the optional data store.
        /// </summary>
        ElementSchema DataStore { get; set; }

        /// <summary>
        /// Transforms an input stream.
        /// </summary>
        /// <param name="data">Source data.</param>
        /// <returns></returns>
        string Execute(string data);
    }
}