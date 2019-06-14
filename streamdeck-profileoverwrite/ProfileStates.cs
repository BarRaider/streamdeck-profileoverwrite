using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfileOverwrite
{
    internal class ProfileStates
    {
        [JsonProperty]
        public string FFamily { get; set; }

        [JsonProperty]
        public string FSize { get; set; }

        [JsonProperty]
        public string FStyle { get; set; }

        [JsonProperty]
        public string FUnderline { get; set; }

        [JsonProperty]
        public string Image { get; set; }

        [JsonProperty]
        public string Title { get; set; }

        [JsonProperty]
        public string TitleAlignment { get; set; }

        [JsonProperty]
        public string TitleColor { get; set; }

        [JsonProperty]
        public string TitleShow { get; set; }
    }
}
