using CreateAR.Commons.Unity.Async;

namespace CreateAR.Spire
{
    /// <summary>
    /// Loads <c>AppData</c> and provides mechanisms for querying it.
    /// </summary>
    public interface IAppDataManager
    {
        /// <summary>
        /// Retrieves a type of data.
        /// </summary>
        /// <typeparam name="T">The type of data to retrieve.</typeparam>
        /// <param name="id">The id of the data.</param>
        /// <returns></returns>
        T Get<T>(string id);

        /// <summary>
        /// Retrieves all of a type of data.
        /// </summary>
        /// <typeparam name="T">The type of data to retrieve.</typeparam>
        /// <returns></returns>
        T[] GetAll<T>();

        /// <summary>
        /// Loads all data for an app by name.
        /// 
        /// TODO: Transition to id.
        /// </summary>
        /// <param name="name">Readable name.</param>
        /// <returns></returns>
        IAsyncToken<Void> Load(string name);
    }
}