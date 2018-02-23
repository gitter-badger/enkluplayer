using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Keeps track of prev + next state.
    /// </summary>
    public class ElementActionUpdateRecord
    {
        /// <summary>
        /// The element.
        /// </summary>
        public Element Element;

        /// <summary>
        /// Type of data for update.
        /// </summary>
        public string SchemaType;
        
        /// <summary>
        /// Key into schema.
        /// </summary>
        public string Key;
        
        /// <summary>
        /// Previous value.
        /// </summary>
        public object PrevValue;

        /// <summary>
        /// Next value.
        /// </summary>
        public object NextValue;
    }
}