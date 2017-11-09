using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.SpirePlayer.Assets;
using UnityEditor;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer.Test.Assets
{
    public class DummyAssetLoader : IAssetLoader
    {
        public IAsyncToken<Object> Load(AssetData data, out LoadProgress progress)
        {
            var token = new AsyncToken<Object>();

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

        public void Destroy()
        {
            //
        }
    }
}