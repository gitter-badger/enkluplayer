// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using QuickType;
//
//    var data = Request.FromJson(jsonString);
//
// For POCOs visit quicktype.io?poco
//
namespace CreateAR.Trellis.Messages.CreateSceneElement
{
    using System;
    using System.Net;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public partial class Request
    {
        [JsonProperty("actions")]
        public Action[] Actions { get; set; }
    }

    public partial class Action
    {
        [JsonProperty("parent")]
        public string Parent { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public partial class Data
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}