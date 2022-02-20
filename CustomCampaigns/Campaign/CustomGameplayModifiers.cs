using CustomCampaigns.Campaign.Missions;

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
