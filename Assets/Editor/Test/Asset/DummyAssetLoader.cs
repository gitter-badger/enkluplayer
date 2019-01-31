using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.EnkluPlayer.Assets;
using UnityEditor;
using Object = UnityEngine.Object;

namespace CreateAR.EnkluPlayer.Test.Assets
{
    public class DummyAssetLoader : IAssetLoader
    {
        private string _error;

        public UrlFormatterCollection Urls { get; private set; }

        public DummyAssetLoader()
        {

        }

        public DummyAssetLoader(string error)
        {
            _error = error;
        }

        public IAsyncToken<Object> Load(AssetData data, int version, out LoadProgress progress)
        {
            var token = new AsyncToken<Object>();

            if (!string.IsNullOrEmpty(_error))
            {
                progress = new LoadProgress();
                token.Fail(new Exception(_error));

                return token;
            }

            var asset = AssetDatabase.LoadAssetAtPath<Object>(data.Uri);
            if (null == asset)
            {
                progress = new LoadProgress
                {
                    Value = 0f
                };

                token.Fail(new Exception("Could not load asset at " + data.Uri + "."));
            }
            else
            {
                progress = new LoadProgress
                {
                    Value = 1f
                };

                token.Succeed(asset);
            }

            return token;
        }

        public void ClearDownloadQueue()
        {
            
        }

        public void Destroy()
        {
            //
        }
    }
}