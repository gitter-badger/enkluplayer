namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// A generic renderer. Copying Unity's naming for now until we need more.
    /// </summary>
    public interface IRenderer
    {
        /// <summary>
        /// The shared material used.
        /// </summary>
        IMaterial SharedMaterial { get; set; }
    }
}