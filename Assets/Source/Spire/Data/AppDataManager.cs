using System.Collections.Generic;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer;

namespace CreateAR.Spire
{
    public class AppDataManager
    {
        private readonly IFileManager _files;

        private AppData _app;
        private readonly List<SceneData> _scenes = new List<SceneData>();
        private readonly List<ContentData> _content = new List<ContentData>();

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
                    _app = file.Data;
                    
                    // now get accompanying scenes
                    LoadScenes(_app)
                        .OnSuccess(_ =>
                        {
                            Log.Info(this, "Loaded scenes.");
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

        private IAsyncToken<Void> LoadScenes(AppData app)
        {
            var token = new AsyncToken<Void>();

            var scenes = app.Scenes;
            var len = scenes.Count;

            // load and save
            var tokens = new IAsyncToken<Void>[len];
            for (var i = 0; i < len; i++)
            {
                tokens[i] = LoadScene(app, scenes[i]);
            }

            // wait for all of them
            Async
                .All(tokens)
                .OnSuccess(_ => token.Succeed(Void.Instance))
                .OnFailure(token.Fail);

            return token;
        }

        private IAsyncToken<Void> LoadScene(AppData app, string id)
        {
            var token = new AsyncToken<Void>();

            var uri = FileProtocols.APP + app.Name + "/SceneData/" + id;
            _files
                .Get<SceneData>(uri)
                .OnSuccess(file =>
                {
                    var scene = file.Data;

                    _scenes.Add(scene);

                    // load all content before succeeding token
                    LoadContent(app, scene)
                        .OnSuccess(token.Succeed)
                        .OnFailure(token.Fail);
                })
                .OnFailure(token.Fail);

            return token;
        }

        private IAsyncToken<Void> LoadContent(AppData app, SceneData scene)
        {
            var token = new AsyncToken<Void>();

            // load and save each piece
            var content = scene.Content;
            var len = content.Count;
            var tokens = new IAsyncToken<File<ContentData>>[len];
            for (var i = 0; i < len; i++)
            {
                var id = content[i];
                var uri = FileProtocols.APP + app.Name + "/ContentData/" + id;

                tokens[i] = _files
                    .Get<ContentData>(uri)
                    .OnSuccess(file =>
                    {
                        _content.Add(file.Data);
                    });
            }

            // listen for all of them
            Async
                .All(tokens)
                .OnSuccess(_ => token.Succeed(Void.Instance))
                .OnFailure(token.Fail);

            return token;
        }
    }
}