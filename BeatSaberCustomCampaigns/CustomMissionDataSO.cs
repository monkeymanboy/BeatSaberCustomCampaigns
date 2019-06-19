using BeatSaberCustomCampaigns.campaign;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatSaberCustomCampaigns
{
    class CustomMissionDataSO : MissionDataSO
    {
        public Campaign campaign;
        public Challenge challenge;
        public CustomPreviewBeatmapLevel customLevel;
    }
}
