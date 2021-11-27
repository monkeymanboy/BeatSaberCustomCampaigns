using Newtonsoft.Json;
using System.Collections.Generic;

namespace CustomCampaigns.Campaign.Missions
{
    public class MissionInfo
    {
        public string title = "";
        public List<InfoSegment> segments = new List<InfoSegment>();
        public bool showEverytime = false;
        public CampaignLightColor lightColor = null;
    }

    public class InfoSegment
    {
        public string text = "";
        public string imageName = "";
        [JsonProperty("hasSeperator")]
        public bool hasSeparator = true;
    }
}
