using Newtonsoft.Json;
using SongCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatSaberCustomCampaigns
{
    public class UnlockedMaps
    {
        private static List<string> maps;
        public static void Load()
        {
            var path = Environment.CurrentDirectory;
            path = path.Replace('\\', '/');
            if (File.Exists(path + "/CampaignMapUnlocks.json"))
            {
                maps = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(path + "/CampaignMapUnlocks.json"));
            }
            else
            {
                maps = new List<string>();
            }
            UnlockMaps();
        }
        private static void UnlockMaps()
        {
            foreach(string s in maps)
            {
                Collections.RegisterCapability("Complete Campaign Challenge - " + s);
            }
        }
        public static void CompletedChallenge(string name)
        {
            if (maps.Contains(name)) return;
            maps.Add(name);
            Collections.RegisterCapability("Complete Campaign Challenge - " + name);
            Save();
        }
        public static void Save()
        {
            var path = Environment.CurrentDirectory;
            path = path.Replace('\\', '/');
            File.WriteAllText(path + "/CampaignMapUnlocks.json", JsonConvert.SerializeObject(maps));
        }
    }
}
