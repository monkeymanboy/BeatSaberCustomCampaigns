using Zenject;

namespace CustomCampaigns.CustomMissionObjectives.Spins
{
    public class SpinsMissionObjectiveCheckerMission : SpinsMissionObjectiveChecker, IInitializable
    {
        [Inject]
        private CustomMissionObjectivesManager _customMissionObjectivesManager;

        public void Initialize()
        {
            _customMissionObjectivesManager.Register(this);
        }
    }
}
