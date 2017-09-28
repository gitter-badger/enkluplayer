using CreateAR.Commons.Unity.Logging;
using CreateAR.Spire;

namespace CreateAR.SpirePlayer
{
    public class PlayApplicationState : IState
    {
        private readonly IFileManager _files;
        private readonly AppDataManager _appDataManager;

        public PlayApplicationState(
            IFileManager files,
            AppDataManager appDataManager)
        {
            _files = files;
            _appDataManager = appDataManager;
        }

        public void Enter()
        {
            _files.Register(
                FileProtocols.APP,
                new SystemXmlSerializer(),
                new LocalFileSystem("Assets/StreamingAssets/App"));

            // TODO: pull off of state
            var appName = "SpireDemo";
            _appDataManager
                .Load(appName)
                .OnSuccess(data =>
                {
                    Log.Info(this, "Loaded {0}.", data.Name);
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not load {0} : {1}.", appName, exception);
                });
        }

        public void Update(float dt)
        {
            
        }

        public void Exit()
        {
            _files.Unregister(FileProtocols.APP);
        }
    }
}