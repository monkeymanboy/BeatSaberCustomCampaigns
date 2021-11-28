using Zenject;

namespace CustomCampaigns.CustomMissionObjectives.MaintainAccuracy
{
    public class MaintainAccuracyMissionObjectiveCheckerMission : MaintainAccuracyMissionObjectiveChecker, IInitializable
    {
        [Inject]
        private CustomMissionObjectivesManager _customMissionObjectivesManager;

        public void Initialize()
        {
            _customMissionObjectivesManager.Register(this);
        }
    }
}
