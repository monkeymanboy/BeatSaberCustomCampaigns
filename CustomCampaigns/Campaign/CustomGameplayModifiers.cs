using CustomCampaigns.Campaign.Missions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCampaigns.Campaign
{
    class CustomGameplayModifiers : GameplayModifiers
    {
        public MissionModifiers missionModifiers;

        public CustomGameplayModifiers(GameplayModifiers gameplayModifiers) :
                base(gameplayModifiers.demoNoFail, gameplayModifiers.demoNoObstacles, gameplayModifiers.energyType, gameplayModifiers.noFailOn0Energy, gameplayModifiers.instaFail,
                     gameplayModifiers.failOnSaberClash, gameplayModifiers.enabledObstacleType, gameplayModifiers.noBombs, gameplayModifiers.fastNotes,
                     gameplayModifiers.strictAngles, gameplayModifiers.disappearingArrows, gameplayModifiers.songSpeed, gameplayModifiers.noArrows, gameplayModifiers.ghostNotes,
                     gameplayModifiers.proMode, gameplayModifiers.zenMode, gameplayModifiers.smallCubes)
        {
        }
    }
}
