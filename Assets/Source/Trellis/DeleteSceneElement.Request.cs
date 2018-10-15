// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using QuickType;
//
//    var data = Request.FromJson(jsonString);
//
// For POCOs visit quicktype.io?poco
//
namespace CreateAR.Trellis.Messages.DeleteSceneElement
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
        [JsonProperty("elementId")]
        public string ElementId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}