using CustomCampaigns.Managers;
using Zenject;

namespace CustomCampaigns.CustomMissionObjectives.MaintainAccuracy
{
    public class MaintainAccuracyMissionObjectiveCheckerStandard : MaintainAccuracyMissionObjectiveChecker, IInitializable
    {
        [Inject]
        private CustomMissionObjectivesStandardLevelManager _customMissionObjectivesManager;

        public void Initialize()
        {
            _customMissionObjectivesManager.Register(this);
        }
    }
}
