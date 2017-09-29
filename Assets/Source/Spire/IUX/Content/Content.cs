using UnityEngine;

namespace CreateAR.Spire
{
    /// <summary>
    /// TODO: Content stub.
    /// </summary>
    public class Content
    {
        public ContentData Data { get; private set; }
        public Transform Transform { get; private set; }

        public Content(ContentData data)
        {
            Data = data;
        }
        
        public void Destroy()
        {
            
        }
    }
}