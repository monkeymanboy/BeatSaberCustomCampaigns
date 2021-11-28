using CustomCampaigns.Managers;
using Zenject;

namespace CustomCampaigns.CustomMissionObjectives.Accuracy
{
    public class AccuracyMissionObjectiveCheckerStandard : AccuracyMissionObjectiveChecker, IInitializable
    {
        [Inject]
        private CustomMissionObjectivesStandardLevelManager _customMissionObjectivesManager;

        public void Initialize()
        {
            Plugin.logger.Debug("initialize acc");
            _customMissionObjectivesManager.Register(this);
        }
    }
}
