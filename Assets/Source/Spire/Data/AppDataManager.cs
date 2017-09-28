using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer;

namespace CreateAR.Spire
{
    public class AppDataManager
    {
        private readonly IFileManager _files;

        public AppDataManager(IFileManager files)
        {
            _files = files;
        }

        public IAsyncToken<AppData> Load(string name)
        {
            var token = new AsyncToken<AppData>();

            _files
                .Get<AppData>(FileProtocols.APP + "AppData/" + name)
                .OnSuccess(file =>
                {
                    Log.Info(this, "Loaded AppData for {0}.", name);

                    token.Succeed(file.Data);
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not load AppData for {0} : {1}.",
                        name,
                        exception);

                    token.Fail(exception);
                });

            return token;
        }
    }
}