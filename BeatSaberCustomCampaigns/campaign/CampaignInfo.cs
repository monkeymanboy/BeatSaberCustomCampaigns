using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatSaberCustomCampaigns.campaign
{
    public class CampaignInfo
    {
        public string name;
        public string desc;
        public bool allUnlocked;
        public List<CampainMapPosition> mapPositions = new List<CampainMapPosition>();
        public List<CampaignUnlockGate> unlockGate = new List<CampaignUnlockGate>();
        public int mapHeight = 500;
        public float backgroundAlpha = 1;
    }
}
