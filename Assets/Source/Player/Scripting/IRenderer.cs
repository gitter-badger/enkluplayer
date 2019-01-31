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
        
        /// <summary>
        /// The unique material used. Using this will break shared usages!!
        /// </summary>
        IMaterial Material { get; set; }
    }
}