// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using QuickType;
//
//    var data = Request.FromJson(jsonString);
//
// For POCOs visit quicktype.io?poco
//
namespace CreateAR.Trellis.Messages.EmailSignUpWithLicense
{
    using System;
    using System.Net;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public partial class Request
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("licenseKey")]
        public string LicenseKey { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("eulaVersion")]
        public string EulaVersion { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }
}