using BeatSaberMarkupLanguage.Attributes;
using System.ComponentModel;

namespace CustomCampaigns.UI.GameplaySetupUI
{
    public class SettingsHandler : INotifyPropertyChanged
    {
        private Config _config;
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _globalLeaderboardEnabled = false;
        [UIValue("globalLeaderboardEnabled")]
        protected bool globalLeaderboardEnabled
        {
            get => _globalLeaderboardEnabled;
            set
            {
                _globalLeaderboardEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(globalLeaderboardEnabled)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(leaderboardPositionVisible)));
            }
        }

        [UIValue("leaderboardPositionVisible")]
        protected bool leaderboardPositionVisible
        {
            get => globalLeaderboardEnabled && _config.floorLeaderboard;
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

        [UIValue("objectiveUI")]
        public bool objectiveUI
        {
            get => _config.disableObjectiveUI;
            set
            {
                _config.disableObjectiveUI = value;
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(leaderboardPositionVisible)));
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

        internal void SetFloorLeaderboardSettingVisibility(bool visible)
        {
            globalLeaderboardEnabled = visible;
        }
    }
}
