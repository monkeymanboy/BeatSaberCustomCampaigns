using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatSaberCustomCampaigns
{
    [Serializable]
    public class ChallengeModifiers
    {
        public bool fastNotes;
        public GameplayModifiers.SongSpeed songSpeed;
        public bool noBombs;
        public bool disappearingArrows;
        public bool strictAngles;
        public bool noObstacles;
        public bool batteryEnergy;
        public bool failOnSaberClash;
        public bool instaFail;
        public bool noFail;
        public bool noArrows;
        public bool ghostNotes;
        public GameplayModifiers.EnergyType energyType;
        public GameplayModifiers.EnabledObstacleType enabledObstacleType;

        public float speedMul;
        public GameplayModifiers GetGameplayModifiers()
        {
            CustomGameplayModifiers modifiers = new CustomGameplayModifiers(GameplayModifiers.defaultModifiers);
            modifiers.challengeModifiers = this;
            modifiers.fastNotes = fastNotes;
            modifiers.noBombs = noBombs;
            modifiers.disappearingArrows = disappearingArrows;
            modifiers.strictAngles = strictAngles;
            modifiers.noObstacles = noObstacles;
            modifiers.batteryEnergy = batteryEnergy;
            modifiers.failOnSaberClash = failOnSaberClash;
            modifiers.instaFail = instaFail;
            modifiers.noFail = noFail;
            modifiers.energyType = energyType;
            modifiers.enabledObstacleType = enabledObstacleType;
            modifiers.noArrows = noArrows;
            modifiers.ghostNotes = ghostNotes;
            return modifiers;
        }
    }
}
