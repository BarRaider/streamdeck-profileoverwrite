using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProfileOverwrite
{
    internal class ProfileAction
    {
        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public string UUID { get; set; }

        [JsonProperty]
        public int State { get; set; }

        [JsonProperty]
        public JObject Settings { get; set; }

        [JsonProperty]
        public List<ProfileStates> States { get; set; }
    }
}
