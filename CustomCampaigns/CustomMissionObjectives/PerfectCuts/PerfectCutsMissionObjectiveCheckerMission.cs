using Zenject;

namespace CustomCampaigns.CustomMissionObjectives.PerfectCuts
{
    public class PerfectCutsMissionObjectiveCheckerMission : PerfectCutsMissionObjectiveChecker, IInitializable
    {
        [Inject]
        private CustomMissionObjectivesManager _customMissionObjectivesManager;

        public void Initialize()
        {
            _customMissionObjectivesManager.Register(this);
        }
    }
}
