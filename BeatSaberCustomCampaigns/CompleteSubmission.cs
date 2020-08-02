using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace BeatSaberCustomCampaigns
{
    class CompleteSubmission
    {
        public string userID;
        public string challengeHash;
        public int score;
        public List<Requirement> requirements = new List<Requirement>();

        internal IEnumerator Submit(string completionPost)
        {
            UnityWebRequest www = UnityWebRequest.Post(completionPost, JsonConvert.SerializeObject(this));
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
        }
    }
    class Requirement
    {
        public string name;
        public int value;
    }
}
