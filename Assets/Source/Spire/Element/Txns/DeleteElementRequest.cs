using LightJson;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Request to delete an element.
    /// </summary>
    public class DeleteElementRequest : ElementRequest
    {
        /// <summary>
        /// Id of the element.
        /// </summary>
        [JsonName("elementId")]
        public string ElementId;
    }
}