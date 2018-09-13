using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Vine;

namespace CreateAR.EnkluPlayer.Test.Vine
{
    /// <summary>
    /// Dummy implementation.
    /// </summary>
    public class DummyVinePreProcessor : IVinePreProcessor
    {
        /// <inheritdoc cref="IVinePreProcessor"/>
        public ElementSchema DataStore { get; set; }

        /// <inheritdoc cref="IVinePreProcessor"/>
        public string Execute(string data)
        {
            return data;
        }
    }
}