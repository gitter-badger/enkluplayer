// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using QuickType;
//
//    var data = Response.FromJson(jsonString);
//
// For POCOs visit quicktype.io?poco
//
namespace CreateAR.Trellis.Messages.GetApp
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
        public Body Body { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }
    }

    public partial class Body
    {
        [JsonProperty("ispublic")]
        public bool Ispublic { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("createdAt")]
        public string CreatedAt { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("owner")]
        public string Owner { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("scenes")]
        public string[] Scenes { get; set; }

        [JsonProperty("updatedAt")]
        public string UpdatedAt { get; set; }
    }
}