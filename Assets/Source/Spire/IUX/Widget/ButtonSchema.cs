namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// TODO: Refactor with new Element Schema pipeline.
    /// </summary>
    public class ButtonSchema : InteractableSchema
    {
        /// <summary> 
        /// For buttons without captions but still activatable with voice .
        /// </summary> 
        public string VoiceActivator;
    }
}