// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using QuickType;
//
//    var data = Request.FromJson(jsonString);
//
// For POCOs visit quicktype.io?poco
//
namespace CreateAR.Trellis.Messages.UpdateKv
{
    using System;
    using System.Net;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public partial class Request
    {
        [JsonProperty("value")]
        public Value Value { get; set; }

        [JsonProperty("version")]
        public long Version { get; set; }
    }

    public partial class Value
    {
        [JsonProperty("hopefully")]
        public Hopefully Hopefully { get; set; }

        [JsonProperty("this")]
        public string This { get; set; }
    }

    public partial class Hopefully
    {
        [JsonProperty("prob")]
        public string Prob { get; set; }

        [JsonProperty("this")]
        public string This { get; set; }
    }
}