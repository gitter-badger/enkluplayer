// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using QuickType;
//
//    var data = Response.FromJson(jsonString);
//
// For POCOs visit quicktype.io?poco
//
namespace CreateAR.Trellis.Messages.GetPublicAssets
{
    using System;
    using System.Net;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public partial class Response
    {
        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("body")]
        public Body[] Body { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }
    }

    public partial class Body
    {
        [JsonProperty("tags")]
        public string Tags { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("createdAt")]
        public string CreatedAt { get; set; }

        [JsonProperty("crc")]
        public string Crc { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("owner")]
        public string Owner { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("stats")]
        public Stats Stats { get; set; }

        [JsonProperty("uriThumb")]
        public string UriThumb { get; set; }

        [JsonProperty("updatedAt")]
        public string UpdatedAt { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("webgl")]
        public string Webgl { get; set; }

        [JsonProperty("version")]
        public long Version { get; set; }

        [JsonProperty("wsaplayer")]
        public string Wsaplayer { get; set; }
    }

    public partial class Stats
    {
        [JsonProperty("triCount")]
        public long TriCount { get; set; }

        [JsonProperty("bounds")]
        public Bounds Bounds { get; set; }

        [JsonProperty("vertCount")]
        public long VertCount { get; set; }
    }

    public partial class Bounds
    {
        [JsonProperty("max")]
        public Max Max { get; set; }

        [JsonProperty("min")]
        public Max Min { get; set; }
    }

    public partial class Max
    {
        [JsonProperty("y")]
        public double Y { get; set; }

        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("z")]
        public double Z { get; set; }
    }
}