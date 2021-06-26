using CustomCampaigns.Campaign;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
