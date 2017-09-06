using System;
using CreateAR.Commons.Unity.Async;
using UnityEditor;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer.Test
{
    public class DummyAssetLoader : IAssetLoader
    {
        public IAsyncToken<Object> Load(AssetInfo info)
        {
            var token = new AsyncToken<Object>();

            var asset = AssetDatabase.LoadAssetAtPath<Object>(info.Uri);
            if (null == asset)
            {
                token.Fail(new Exception("Could not load asset at " + info.Uri + "."));
            }
            else
            {
                token.Succeed(asset);
            }

            return token;
        }
    }
}