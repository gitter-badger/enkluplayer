namespace CreateAR.EnkluPlayer.Vine
{
    /// <summary>
    /// Simple lookup for vines.
    /// </summary>
    public interface IVineTable
    {
        /// <summary>
        /// Get a vine.
        /// </summary>
        /// <param name="identifier">Unique identifier for vine.</param>
        /// <returns></returns>
        VineReference Vine(string identifier);
    }
}