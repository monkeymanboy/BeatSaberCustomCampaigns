using CustomCampaigns.Campaign.Missions;
using System.Threading.Tasks;

namespace CustomCampaigns.External.Interfaces
{
    public interface IModifierHandler
    {
        public Task<bool> OnLoadMission(string[] inArray, Mission mission);

        public void OnMissionFailedToLoad(Mission mission);
        public void OnMissionEnd(Mission mission);
    }
}
