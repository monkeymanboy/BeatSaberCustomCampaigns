using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace CustomCampaigns.Managers
{
    public class UnlockableSongsManager : IInitializable
    {
        private const string UNLOCK_FILE = "CampaignMapUnlocks.json";
        private const string USER_DATA = "UserData";
        private const string CAMPAIGN_FOLDER = "Custom Campaigns";
        private HashSet<string> _unlockedMaps;

        public void Initialize()
        {
            var oldPath = Path.Combine(Environment.CurrentDirectory, UNLOCK_FILE);
            var newDirectory = Path.Combine(Environment.CurrentDirectory, USER_DATA, CAMPAIGN_FOLDER);

            if (!Directory.Exists(newDirectory))
            {
                Directory.CreateDirectory(newDirectory);
            }

            var path = Path.Combine(newDirectory, UNLOCK_FILE);
            TransferFile(oldPath, path);

            if (File.Exists(path))
            {
                _unlockedMaps = JsonConvert.DeserializeObject<HashSet<string>>(File.ReadAllText(path));
            }
            else
            {
                _unlockedMaps = new HashSet<string>();
            }

            UnlockMaps();
        }

        public void CompleteMission(string map)
        {
            if (_unlockedMaps.Contains(map))
            {
                return;
            }

            _unlockedMaps.Add(map);
            UnlockMap(map);
            Save();
        }

        private void UnlockMaps()
        {
            foreach (var map in _unlockedMaps)
            {
                UnlockMap(map);
            }
        }

        private void UnlockMap(string map)
        {
            Plugin.logger.Debug($"Unlocking map: {map}");
            SongCore.Collections.RegisterCapability($"Complete Campaign Mission - {map}");
        }

        private void Save()
        {
            var path = Path.Combine(Environment.CurrentDirectory, USER_DATA, CAMPAIGN_FOLDER, UNLOCK_FILE);
            File.WriteAllText(path, JsonConvert.SerializeObject(_unlockedMaps));
        }

        private void TransferFile(string oldPath, string newPath)
        {
            if (File.Exists(oldPath) && !File.Exists(newPath))
            {
                Plugin.logger.Debug("Transferring unlock file to new path");
                try
                {
                    File.Copy(oldPath, newPath);
                    File.Delete(oldPath);
                }
                catch (Exception e)
                {
                    Plugin.logger.Error($"Error replacing old file: {e.Message}");
                }
            }
        }
    }
}
