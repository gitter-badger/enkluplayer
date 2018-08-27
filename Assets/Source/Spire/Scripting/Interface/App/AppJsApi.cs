namespace CreateAR.SpirePlayer.Scripting
{
    /// <summary>
    /// Interface for an app. 
    /// </summary>
    public class AppJsApi
    {
        /// <summary>
        /// Manages the app's scenes.
        /// </summary>
        public readonly AppScenesJsApi scenes;

        /// <summary>
        /// The app's elements.
        /// </summary>
        public readonly AppElementsJsApi elements;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public AppJsApi(AppScenesJsApi scenes, AppElementsJsApi elements)
        {
            this.scenes = scenes;
            this.elements = elements;
        }
    }
}