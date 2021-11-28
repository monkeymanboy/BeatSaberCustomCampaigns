using Zenject;

namespace CustomCampaigns.CustomMissionObjectives.SaberTimeInWall
{
    public class SaberTimeInWallMissionObjectiveCheckerMission : SaberTimeInWallMissionObjectiveChecker, IInitializable
    {
        [Inject]
        private CustomMissionObjectivesManager _customMissionObjectivesManager;

        public void Initialize()
        {
            _customMissionObjectivesManager.Register(this);
        }
    }
}
