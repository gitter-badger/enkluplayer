namespace CreateAR.SpirePlayer
{
    public interface IQueryResolver
    {
        bool Resolve(string query, ref string[] tags);
    }
}