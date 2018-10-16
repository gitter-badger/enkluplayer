// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using QuickType;
//
//    var data = Response.FromJson(jsonString);
//
// For POCOs visit quicktype.io?poco
//
namespace CreateAR.Trellis.Messages.CreateSubscription
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
        [JsonProperty("manager")]
        public string Manager { get; set; }

        [JsonProperty("entitlements")]
        public Entitlements Entitlements { get; set; }

        [JsonProperty("createdAt")]
        public string CreatedAt { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("plan")]
        public string Plan { get; set; }

        [JsonProperty("paymentNeeded")]
        public bool PaymentNeeded { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("updatedAt")]
        public string UpdatedAt { get; set; }
    }

    public partial class Entitlements
    {
        [JsonProperty("collaboration")]
        public Collaboration Collaboration { get; set; }

        [JsonProperty("assets")]
        public Assets Assets { get; set; }

        [JsonProperty("publishing")]
        public Publishing Publishing { get; set; }
    }

    public partial class Collaboration
    {
        [JsonProperty("max")]
        public long Max { get; set; }
    }

    public partial class Assets
    {
        [JsonProperty("maxDisk")]
        public long MaxDisk { get; set; }

        [JsonProperty("advancedDecimate")]
        public bool AdvancedDecimate { get; set; }

        [JsonProperty("priority")]
        public bool Priority { get; set; }
    }

    public partial class Publishing
    {
        [JsonProperty("standalone")]
        public bool Standalone { get; set; }
    }
}