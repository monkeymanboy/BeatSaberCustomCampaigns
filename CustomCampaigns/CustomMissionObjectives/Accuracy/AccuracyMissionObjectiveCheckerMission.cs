using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
