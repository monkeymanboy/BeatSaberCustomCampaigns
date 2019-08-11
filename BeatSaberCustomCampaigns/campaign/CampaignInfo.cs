using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatSaberCustomCampaigns.campaign
{
    public class CampaignInfo
    {
        public string name;
        public string desc;
        public string bigDesc = "";
        public bool allUnlocked;
        public List<CampainMapPosition> mapPositions = new List<CampainMapPosition>();
        public List<CampaignUnlockGate> unlockGate = new List<CampaignUnlockGate>();
        public int mapHeight = 500;
        public float backgroundAlpha = 1;
        public CampaignLightColor lightColor = new CampaignLightColor(0.188f, 0.620f, 1);//if you're wondering what this is it's the games default light color
    }
}
