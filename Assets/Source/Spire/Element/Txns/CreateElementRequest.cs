using CreateAR.SpirePlayer.IUX;
using LightJson;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Request for creating an element.
    /// </summary>
    public class CreateElementRequest : ElementRequest
    {
        /// <summary>
        /// Id of parent.
        /// </summary>
        [JsonName("parent")]
        public string ParentId;

        /// <summary>
        /// ElementData.
        /// </summary>
        [JsonName("data")]
        public ElementData Data;
    }
}