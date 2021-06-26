using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCampaigns.Campaign.Missions
{
    class CustomMissionDataSO : MissionDataSO
    {
        public Campaign campaign;
        public Mission mission;
        public CustomPreviewBeatmapLevel customLevel;
    }
}
