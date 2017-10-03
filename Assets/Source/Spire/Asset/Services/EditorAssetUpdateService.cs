using System;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    public class EditorAssetUpdateService : IAssetUpdateService
    {
        private readonly IHttpService _http;

        public event Action<AssetData[]> OnAdded;
        public event Action<AssetData[]> OnUpdated;
        public event Action<AssetData[]> OnRemoved;

        public EditorAssetUpdateService(IHttpService http)
        {
            _http = http;
        }

        public IAsyncToken<Void> Initialize()
        {
            var token = new AsyncToken<Void>();

            Log.Info(this, "Get Asset Manifest.");

            _http
                .Get<Trellis.Messages.GetAssets.Response>(_http.UrlBuilder.Url("/asset"))
                .OnSuccess(response =>
                {
                    Log.Info(this, "Got manifest.");

                    if (null == response.Payload.Body
                        || null == response.Payload.Body.Assets)
                    {
                        Log.Error(
                            this,
                            "Improper response. No assets on asset manifest request.");
                        return;
                    }

                    if (null != OnAdded)
                    {
                        var assetInfos = response.Payload.Body
                            .Assets
                            .Select(asset => new AssetData
                            {
                                Guid = asset.Id,
                                Uri = ""
                            })
                            .ToArray();

                        OnAdded(assetInfos);
                    }

                    token.Succeed(Void.Instance);
                })
                .OnFailure(token.Fail);

            return token;
        }
    }
}