// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using QuickType;
//
//    var data = Response.FromJson(jsonString);
//
// For POCOs visit quicktype.io?poco
//
namespace CreateAR.Trellis.Messages.EmailSignUp
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
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("account")]
        public Account Account { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }
    }

    public partial class Account
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
    }

    public partial class User
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}