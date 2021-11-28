using CustomCampaigns.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
