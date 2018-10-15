// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using QuickType;
//
//    var data = Request.FromJson(jsonString);
//
// For POCOs visit quicktype.io?poco
//
namespace CreateAR.Trellis.Messages.UpdateNeighborhoodKv
{
    using System;
    using System.Net;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public partial class Request
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("version")]
        public long Version { get; set; }
    }
}