// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using QuickType;
//
//    var data = Request.FromJson(jsonString);
//
// For POCOs visit quicktype.io?poco
//
namespace CreateAR.Trellis.Messages.CreateScript
{
    using System;
    using System.Net;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public partial class Request
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("script")]
        public string Script { get; set; }

        [JsonProperty("tags")]
        public string Tags { get; set; }
    }
}