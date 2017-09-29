namespace CreateAR.Spire.Test
{
    public class DummyContentManager : IContentFactory
    {
        public Content Instance(ContentData data)
        {
            return new Content(data);
        }
    }
}