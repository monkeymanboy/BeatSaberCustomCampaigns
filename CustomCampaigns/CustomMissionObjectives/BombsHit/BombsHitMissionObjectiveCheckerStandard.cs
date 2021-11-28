using CustomCampaigns.Managers;
using Zenject;

namespace CustomCampaigns.CustomMissionObjectives.BombsHit
{
    public class BombsHitMissionObjectiveCheckerStandard : BombsHitMissionObjectiveChecker, IInitializable
    {
        [Inject]
        private CustomMissionObjectivesStandardLevelManager _customMissionObjectivesManager;

        public void Initialize()
        {
            _customMissionObjectivesManager.Register(this);
        }
    }
}
