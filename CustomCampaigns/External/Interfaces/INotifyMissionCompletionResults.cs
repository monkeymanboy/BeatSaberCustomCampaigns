using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCampaigns.External.Interfaces
{
    public interface INotifyMissionCompletionResults
    {
        public void OnMissionCompletionResultsAvailable(MissionCompletionResults missionCompletionResults);
    }
}
