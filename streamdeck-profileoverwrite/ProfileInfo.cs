using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProfileOverwrite
{
    internal class ProfileInfo
    {
        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public Dictionary<string, ProfileAction> Actions { get; set; }

        [JsonIgnore]
        public string FullPath { get; set; }

        [JsonProperty]
        public string Version { get; set; }

        [JsonProperty]
        public string DeviceUUID { get; set; }

        [JsonProperty]
        public string DeviceModel { get; set; }
    }
}
