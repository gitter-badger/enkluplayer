using CreateAR.SpirePlayer.IUX;
using CreateAR.SpirePlayer.Vine;

namespace CreateAR.SpirePlayer.Test.Vine
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