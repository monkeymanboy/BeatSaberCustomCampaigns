using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CustomCampaigns.External
{
    internal class ExternalModifier
    {
        [JsonProperty(Required = Required.Always)]
        public string Name;
        [JsonProperty(Required = Required.Always)]
        public List<ExternalModifierInfo> Infos;
        [JsonProperty(Required = Required.Always)]
        internal string HandlerLocation;
        [JsonProperty(Required = Required.AllowNull)]
        internal string ModifierLocation;

        public class ExternalModifierInfo
        {
            [JsonProperty(Required = Required.AllowNull)]
            public string Name;
            [JsonProperty(Required = Required.AllowNull)]
            public string Description;
            [JsonProperty(Required = Required.DisallowNull)]
            public string Icon;
        }

        public Type HandlerType;
        public Type ModifierType;
    }
}
