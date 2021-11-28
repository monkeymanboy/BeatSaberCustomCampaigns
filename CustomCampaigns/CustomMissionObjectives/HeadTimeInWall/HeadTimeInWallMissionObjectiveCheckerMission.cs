using Zenject;

namespace CustomCampaigns.CustomMissionObjectives.HeadTimeInWall
{
    public class HeadTimeInWallMissionObjectiveCheckerMission : HeadTimeInWallMissionObjectiveChecker, IInitializable
    {
        [Inject]
        private CustomMissionObjectivesManager _customMissionObjectivesManager;

        public void Initialize()
        {
            _customMissionObjectivesManager.Register(this);
        }
    }
}
