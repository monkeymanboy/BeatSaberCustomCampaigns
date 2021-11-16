using BeatSaberCustomCampaigns.campaign;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatSaberCustomCampaigns
{
    public class ChallengeInfo
    {
        public string title = "";
        public bool showEverytime = true;
        public List<InfoSegment> segments = new List<InfoSegment>();

        public class InfoSegment
        {
            public string text = "";
            public string imageName = "";
            public bool hasSeperator = true;
        }

        public CampaignLightColor lightColor = null;
    }
}
