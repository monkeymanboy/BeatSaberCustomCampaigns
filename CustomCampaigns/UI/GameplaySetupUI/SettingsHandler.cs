using BeatSaberMarkupLanguage.Attributes;
using System.ComponentModel;

namespace CustomCampaigns.UI.GameplaySetupUI
{
    public class SettingsHandler : INotifyPropertyChanged
    {
        private Config _config;
        public event PropertyChangedEventHandler PropertyChanged;

        [UIValue("ssInstalled")]
        protected bool isLoaded
        {
            get => Plugin.isScoreSaberInstalled;
        }

        [UIValue("objectiveParticles")]
        public bool objectiveParticles
        {
            get => _config.disableObjectiveParticles;
            set
            {
                _config.disableObjectiveParticles = value;
            }
        }

        [UIValue("floorLeaderboard")]
        public bool floorLeaderboard
        {
            get => _config.floorLeaderboard;
            set
            {
                _config.floorLeaderboard = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(floorLeaderboard)));
            }
        }

        [UIValue("floorLeaderboardPosition")]
        public float floorLeaderboardPosition
        {
            get => _config.floorLeaderboardPosition;
            set
            {
                _config.floorLeaderboardPosition = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(floorLeaderboardPosition)));
            }
        }

        public SettingsHandler(Config config)
        {
            _config = config;
        }
    }
}
