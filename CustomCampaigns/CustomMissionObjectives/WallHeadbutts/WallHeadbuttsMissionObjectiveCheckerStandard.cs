using CustomCampaigns.Managers;
using Zenject;

namespace CustomCampaigns.CustomMissionObjectives.WallHeadbutts
{
    public class WallHeadbuttsMissionObjectiveCheckerStandard : WallHeadbuttsMissionObjectiveChecker, IInitializable
    {
        [Inject]
        private CustomMissionObjectivesStandardLevelManager _customMissionObjectivesManager;

        public void Initialize()
        {
            _customMissionObjectivesManager.Register(this);
        }
    }
}
