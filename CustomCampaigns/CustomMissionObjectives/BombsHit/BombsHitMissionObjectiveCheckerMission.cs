using Zenject;

namespace CustomCampaigns.CustomMissionObjectives.BombsHit
{
    public class BombsHitMissionObjectiveCheckerMission : BombsHitMissionObjectiveChecker, IInitializable
    {
        [Inject]
        private CustomMissionObjectivesManager _customMissionObjectivesManager;

        public void Initialize()
        {
            _customMissionObjectivesManager.Register(this);
        }
    }
}
