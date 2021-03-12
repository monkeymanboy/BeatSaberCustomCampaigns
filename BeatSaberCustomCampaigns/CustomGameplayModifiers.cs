namespace BeatSaberCustomCampaigns
{
    public class CustomGameplayModifiers : GameplayModifiers
    {
        public ChallengeModifiers challengeModifiers;

        public CustomGameplayModifiers(GameplayModifiers gameplayModifiers) :
            base(gameplayModifiers.demoNoFail, gameplayModifiers.demoNoObstacles, gameplayModifiers.energyType,
                gameplayModifiers.noFailOn0Energy, gameplayModifiers.instaFail,
                gameplayModifiers.failOnSaberClash, gameplayModifiers.enabledObstacleType, gameplayModifiers.noBombs,
                gameplayModifiers.fastNotes,
                gameplayModifiers.strictAngles, gameplayModifiers.disappearingArrows, gameplayModifiers.songSpeed,
                gameplayModifiers.noArrows, gameplayModifiers.ghostNotes,
                gameplayModifiers.proMode, gameplayModifiers.zenMode, gameplayModifiers.smallCubes)
        {
        }
    }
}
