namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Passed from host to application when an asset preview has been requested.
    /// </summary>
    public class PreviewEvent
    {
        public string id;
        public string name;
        public string uri;
        public string crc;
        public string owner;
        public string status;
        public string tags;
        public int version;

        public string createdAt;
        public string updatedAt;
    }
}