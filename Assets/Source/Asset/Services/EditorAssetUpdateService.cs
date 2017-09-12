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

        public event Action<AssetInfo[]> OnAdded;
        public event Action<AssetInfo[]> OnUpdated;
        public event Action<AssetInfo[]> OnRemoved;

        public EditorAssetUpdateService(IHttpService http)
        {
            _http = http;
        }

        public IAsyncToken<Void> Initialize()
        {
            var token = new AsyncToken<Void>();

            Log.Info(this, "Get Asset Manifest.");

            _http
                .Get<Response<GetAssetManifestBody>>(_http.UrlBuilder.Url("/asset"))
                .OnSuccess(response =>
                {
                    Log.Info(this, "Got manifest.");

                    if (null == response.Payload.body
                        || null == response.Payload.body.assets)
                    {
                        Log.Error(
                            this,
                            "Improper response. No assets on asset manifest request.");
                        return;
                    }

                    if (null != OnAdded)
                    {
                        var assetInfos = response.Payload.body
                            .assets
                            .Select(asset => new AssetInfo
                            {
                                Guid = asset.id,
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