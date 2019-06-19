using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatSaberCustomCampaigns
{
    public class CustomGameplayModifiers : GameplayModifiers
    {
        public ChallengeModifiers challengeModifiers;

        public CustomGameplayModifiers(GameplayModifiers gameplayModifiers) : base(gameplayModifiers)
        {
        }
    }
}
