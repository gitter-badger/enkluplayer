using System.Linq;
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

            // first, get app data
            _files
                .Get<AppData>(FileProtocols.APP + name + "/App")
                .OnSuccess(file =>
                {
                    var appData = file.Data;

                    Log.Info(this, "Loaded AppData for {0}.", name);

                    // now get accompanying scenes
                    Async
                        .All(LoadScenes(appData))
                        .OnSuccess(scenes =>
                        {
                            Log.Info(this, "Loaded {0} scenes.", scenes.Length);
                        })
                        .OnFailure(token.Fail);
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

        private IAsyncToken<File<SceneData>>[] LoadScenes(AppData appData)
        {
            return appData
                .Scenes
                .Select(scene =>
                {
                    var uri = FileProtocols.APP + appData.Name + "/SceneData/" + scene;
                    return _files.Get<SceneData>(uri);
                })
                .ToArray();
        }
    }
}