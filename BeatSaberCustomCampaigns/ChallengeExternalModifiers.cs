using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatSaberCustomCampaigns
{
    public class ChallengeExternalModifiers
    {
        public static Dictionary<string, Func<string[], bool>> externalModifiers = new Dictionary<string, Func<string[], bool>>();//You don't need to touch this

        public static Action onChallengeEnd;//this is called when returning to the menu after a challenge doesn't matter if they win, lose, or manually exit the challenge this will be called
        public static Action onChallengeFailedToLoad;//This is called after all Handlers run if any of them return false or one is missing, this means a mod is missing or likely needs to be updated
        
        /* TO OTHER MODDERS
         * The function passed to RegisterHandler takes in a string array and returns a boolean
         * 
         * the string array is whatever is in the external modifier array for your challenge
         * now you can handle this array however you want, just remember that however you set it up
         * is how challenge creators have to format their information
         * 
         * the boolean MUST return true if everything was handled correctly and false if not
         * this is important because if you add functionality to your mod and a newer challenge
         * uses that functionality we don't want people on the old version of the mod to be able
         * to load the challenge
         */
        public static void RegisterHandler(string modname, Func<string[],bool> onLoadChallenge)
        {
            externalModifiers.Add(modname, onLoadChallenge);
        }
    }
}
