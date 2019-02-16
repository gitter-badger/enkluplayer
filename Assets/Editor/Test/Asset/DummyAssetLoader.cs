using System;
using System.Collections.Generic;
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
        public int QueueLength { get; private set; }
        public List<StandardAssetLoader.AssetLoadFailure> LoadFailures { get; private set; }

        public DummyAssetLoader() : this(string.Empty)
        {
            
        }

        public DummyAssetLoader(string error)
        {
            _error = error;

            LoadFailures = new List<StandardAssetLoader.AssetLoadFailure>();
        }

        public IAsyncToken<Object> Load(AssetData data, int version, out LoadProgress progress)
        {
            var token = new AsyncToken<Object>();
            QueueLength++;

            if (!string.IsNullOrEmpty(_error))
            {
                progress = new LoadProgress();
                var exception = new Exception(_error);

                QueueLength--;
                LoadFailures.Add(new StandardAssetLoader.AssetLoadFailure
                {
                    AssetData = data,
                    Exception = exception
                });
                token.Fail(exception);
                return token;
            }

            var asset = AssetDatabase.LoadAssetAtPath<Object>(data.Uri);
            if (null == asset)
            {
                progress = new LoadProgress
                {
                    Value = 0f
                };

                var exception = new Exception("Could not load asset at " + data.Uri + ".");
                
                QueueLength--;
                LoadFailures.Add(new StandardAssetLoader.AssetLoadFailure
                {
                    AssetData = data,
                    Exception = exception
                });
                token.Fail(exception);
                token.Fail(exception);
            }
            else
            {
                progress = new LoadProgress
                {
                    Value = 1f
                };

                QueueLength--;
                token.Succeed(asset);
            }

            return token;
        }

        public void Clear()
        {
            QueueLength = 0;
            LoadFailures.Clear();
        }

        public void Destroy()
        {
            //
        }
    }
}