namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Wraps a response body in the usual structure.
    /// </summary>
    /// <typeparam name="T">Body type.</typeparam>
    public class Response<T>
    {
        public bool success;
        public string error;
        public T body;
    }
}