namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Representation of an Audio source.
    /// </summary>
    public interface IAudioSource
    {
        /// <summary>
        /// The volume the source uses.
        /// </summary>
        float Volume { get; set; }
        
        /// <summary>
        /// Loop.
        /// </summary>
        bool Loop { get; set; }
        
        /// <summary>
        /// Mute.
        /// </summary>
        bool Mute { get; set; }
        
        /// <summary>
        /// Whether the audio plays on awake or not.
        /// </summary>
        bool PlayOnAwake { get; set; }
        
        /// <summary>
        /// Spatial Blend. 0: 2D, 1:3D
        /// </summary>
        float SpatialBlend { get; set; }
        
        /// <summary>
        /// Minimum distance.
        /// </summary>
        float MinDistance { get; set; }
        
        /// <summary>
        /// Maximum distance.
        /// </summary>
        float MaxDistance { get; set; }
        
        /// <summary>
        /// Doppler level.
        /// </summary>
        float DopplerLevel { get; set; }
    }
}