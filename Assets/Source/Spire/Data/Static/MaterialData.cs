using Newtonsoft.Json;

namespace CreateAR.SpirePlayer
{
    public class MaterialData : StaticData
    {
        [JsonProperty("shaderId")]
        public string ShaderId { get; set; }
    }
}