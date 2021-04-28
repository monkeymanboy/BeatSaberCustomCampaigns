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
        public bool proMode;
        public bool zenMode;
        public bool smallCubes;
        public GameplayModifiers.EnergyType energyType;
        public GameplayModifiers.EnabledObstacleType enabledObstacleType;

        public float speedMul;
        public GameplayModifiers GetGameplayModifiers()
        {
            var defaultModifiers = new GameplayModifiers();
            CustomGameplayModifiers modifiers = new CustomGameplayModifiers( new GameplayModifiers(
                defaultModifiers.demoNoFail,
                defaultModifiers.demoNoObstacles,
                energyType,
                noFail,
                instaFail,
                failOnSaberClash,
                enabledObstacleType,
                noBombs,
                fastNotes,
                strictAngles,
                disappearingArrows,
                defaultModifiers.songSpeed,
                noArrows,
                ghostNotes,
                proMode,
                zenMode,
                smallCubes
            )); ;

            modifiers.challengeModifiers = this;
            return modifiers;
        }
    }
}
