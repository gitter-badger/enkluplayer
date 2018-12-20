using System;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Provides helper methods for dealing with assets.
    /// </summary>
    public static class AssetUrlHelper
    {
        /// <summary>
        /// Generates a URI for an asset.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static string Uri(AssetData data, int version)
        {
            var uri = data.Uri;

            // try to insert version
            var pieces = data.Uri.Split('.');
            if (pieces.Length == 3)
            {
                uri = string.Format("{0}.{1}.{2}",
                    pieces[0],
                    version,
                    pieces[2]);
            }

            return "assets://" + uri;
        }


        /// <summary>
        /// Formats URI.
        ///
        ///  TODO: remove when we deprecate v1 and v2 assets.
        /// </summary>
        /// <param name="asset">The asset.</param>
        public static string FormatUri(AssetData asset)
        {
            if (string.IsNullOrEmpty(asset.Uri))
            {
                return asset.Uri;
            }

            var index = asset.Uri.IndexOf(".bundle", StringComparison.Ordinal);
            if (-1 == index)
            {
                Log.Warning(null, "Invalid AssetData Uri : {0}.", asset.Uri);
                return asset.Uri;
            }

            // v3
            if (asset.Uri.StartsWith("v3"))
            {
                var uri = asset.Uri.Replace("v3:", "");

                var pieces = uri.Split('.');
                if (3 == pieces.Length)
                {
                    return string.Format("{0}.{1}.{2}",
                        pieces[0],
                        asset.Version,
                        pieces[2]).Replace("{{platform}}", GetBuildTarget());
                }

                Log.Warning(null, "Invalid formatting for v3 asset URI: {0}.", asset.Uri);

                return asset.Uri;
            }

            // v2
            if (asset.Uri.Contains("{{platform}}"))
            {
                return asset.Uri.Replace(
                    "{{platform}}",
                    GetBuildTarget());
            }

            // v1
            var urlSplit = string.Format(
                "{0}_{1}.bundle",
                asset.Uri.Substring(0, index),
                GetBuildTarget()).Split('/');
            return urlSplit[urlSplit.Length - 1];
        }

        /// <summary>
        /// Formats the URI to the thumb nail.
        ///
        /// TODO: remove upon deprecating of v1 and v2 assets.
        /// </summary>
        /// <param name="asset">The asset.</param>
        public static string FormatUriThumb(AssetData asset)
        {
            if (string.IsNullOrEmpty(asset.UriThumb))
            {
                return asset.UriThumb;
            }

            // v3
            if (asset.UriThumb.StartsWith("v3"))
            {
                var uri = asset.UriThumb.Replace("v3:", "");
                var pieces = uri.Split('.');
                if (3 == pieces.Length)
                {
                    return string.Format("{0}.{1}.{2}",
                        pieces[0],
                        asset.Version,
                        pieces[2]);
                }

                Log.Warning(null, "Invalid formatting for v3 asset thumb URI: {0}.", asset.Uri);

                return asset.UriThumb;
            }

            // v1
            if (asset.UriThumb.StartsWith("/thumb"))
            {
                return string.Format("{0}.png", asset.Guid);
            }

            // v2 - do nothing
            return asset.UriThumb;
        }

        /// <summary>
        /// Retrieves the build target we wish to download bundles for.
        /// </summary>
        /// <returns></returns>
        private static string GetBuildTarget()
        {
            switch (UnityEngine.Application.platform)
            {
                case RuntimePlatform.WebGLPlayer:
                {
                    return "webgl";
                }
                case RuntimePlatform.WSAPlayerX86:
                case RuntimePlatform.WSAPlayerX64:
                case RuntimePlatform.WSAPlayerARM:
                {
                    return "wsaplayer";
                }
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.LinuxEditor:
                {
                    return "webgl";
                }
                case RuntimePlatform.IPhonePlayer:
                {
                    return "ios";
                }
                default:
                {
                    return "UNKNOWN";
                }
            }
        }
    }
}