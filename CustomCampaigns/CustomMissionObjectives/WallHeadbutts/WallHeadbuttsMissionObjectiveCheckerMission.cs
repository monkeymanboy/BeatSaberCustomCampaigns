using Zenject;

namespace CustomCampaigns.CustomMissionObjectives.WallHeadbutts
{
    public class WallHeadbuttsMissionObjectiveCheckerMission : WallHeadbuttsMissionObjectiveChecker, IInitializable
    {
        [Inject]
        private CustomMissionObjectivesManager _customMissionObjectivesManager;

        public void Initialize()
        {
            _customMissionObjectivesManager.Register(this);
        }
    }
}
