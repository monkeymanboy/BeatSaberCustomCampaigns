using Zenject;

namespace CustomCampaigns.CustomMissionObjectives.Accuracy
{
    public class AccuracyMissionObjectiveCheckerMission : AccuracyMissionObjectiveChecker, IInitializable
    {
        [Inject]
        private CustomMissionObjectivesManager _customMissionObjectivesManager;

        public void Initialize()
        {
            _customMissionObjectivesManager.Register(this);
        }
    }
}
