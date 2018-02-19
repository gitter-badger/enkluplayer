using LightJson;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Request for creating an element.
    /// </summary>
    public class CreateElementRequest : ElementRequest
    {
        /// <summary>
        /// Id of element to create.
        /// </summary>
        [JsonName("id")]
        public string ElementId;

        /// <summary>
        /// Id of parent.
        /// </summary>
        [JsonName("parent")]
        public string ParentId;
    }
}