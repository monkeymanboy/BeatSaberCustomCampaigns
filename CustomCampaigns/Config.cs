using IPA.Config.Stores;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace CustomCampaigns
{
    public class Config
    {
        public virtual bool disableObjectiveParticles { get; set; } = false;
        public virtual bool floorLeaderboard { get; set; } = false;
        public virtual float floorLeaderboardPosition { get; set; } = -25;
    }
}
