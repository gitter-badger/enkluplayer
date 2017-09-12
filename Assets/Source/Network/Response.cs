namespace CreateAR.SpirePlayer
{
    public class Response<T>
    {
        public bool success;
        public string error;
        public T body;
    }
}