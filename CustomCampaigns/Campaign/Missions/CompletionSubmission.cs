using BeatSaberCustomCampaigns;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace CustomCampaigns.Campaign.Missions
{
    public class CompletionSubmission
    {
        public string userID;
        public string missionHash;
        public int score;
        public List<Requirement> requirements = new List<Requirement>();

        [JsonIgnore]
        private string _userAgent;

        public CompletionSubmission(string missionHash, int score, List<Requirement> requirements)
        {
            this.userID = APITools.UserID;
            this.missionHash = missionHash;
            this.score = score;
            this.requirements = requirements;

            _userAgent = $"CustomCampaigns/v{Plugin.version}";
        }

        internal async void Submit(string completionPost)
        {
            await SubmitInternal(completionPost);
        }

        private async Task SubmitInternal(string completionPost)
        {
            var www = UnityWebRequest.Post(completionPost, JsonConvert.SerializeObject(this));
            www.SetRequestHeader("User-Agent", _userAgent);

            www.SendWebRequest();

            while (!www.isDone)
            {
                await Task.Yield();
            }

            if (www.isNetworkError || www.isHttpError)
            {
                Plugin.logger.Debug($"Error submitting completion post: {www.error}");
            }
        }

        public class Requirement
        {
            public string name;
            public int value;

            public Requirement(string name, int value)
            {
                this.name = name;
                this.value = value;
            }
        }
    }
}
