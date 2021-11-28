using CustomCampaigns.Managers;
using Zenject;

namespace CustomCampaigns.CustomMissionObjectives.SaberTimeInWall
{
    public class SaberTimeInwallMissionObjectiveCheckerStandard : SaberTimeInWallMissionObjectiveChecker, IInitializable
    {
        [Inject]
        private CustomMissionObjectivesStandardLevelManager _customMissionObjectivesManager;

        public void Initialize()
        {
            _customMissionObjectivesManager.Register(this);
        }
    }
}
