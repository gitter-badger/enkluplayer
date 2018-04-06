namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Interface fo an app. 
    /// </summary>
    public class AppJsApi
    {
        /// <summary>
        /// The app's elements.
        /// </summary>
        public readonly AppElementsJsApi elements;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public AppJsApi(AppElementsJsApi elements)
        {
            this.elements = elements;
        }
    }
}