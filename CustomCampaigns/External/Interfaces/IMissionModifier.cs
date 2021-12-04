using CustomCampaigns.Campaign.Missions;
using CustomCampaigns.External.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCampaigns.External
{
    public interface IMissionModifier
    {
        public void OnMissionStart();
        public void OnMissionEnd();
        public void OnMissionFail();
    }
}
