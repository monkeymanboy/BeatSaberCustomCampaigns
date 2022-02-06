using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace CustomCampaigns
{
    public class Config
    {
        public virtual bool disableObjectiveParticles { get; set; } = false;
        public virtual bool floorLeaderboard { get; set; } = false;
        public virtual float floorLeaderboardPosition { get; set; } = -25;

        [UseConverter(typeof(DictionaryConverter<HashSet<string>>))]
        [NonNullable]
        public virtual Dictionary<string, HashSet<string>> disabledOptionalModWarnings { get; set; } = new Dictionary<string, HashSet<string>>();

        [UseConverter(typeof(ListConverter<string>))]
        [NonNullable]
        public virtual List<string> creditsViewed { get; set; } = new List<string>();

        public virtual void Changed() { }
    }
}
