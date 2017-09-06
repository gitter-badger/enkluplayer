using System;
using CreateAR.Commons.Unity.Async;
using UnityEditor;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer.Test
{
    public class DummyAssetLoader : IAssetLoader
    {
        public IAsyncToken<Object> Load(string url)
        {
            var token = new AsyncToken<Object>();

            var asset = AssetDatabase.LoadAssetAtPath<Object>(url);
            if (null == asset)
            {
                token.Fail(new Exception("Could not load asset at " + url + "."));
            }
            else
            {
                token.Succeed(asset);
            }

            return token;
        }
    }
}