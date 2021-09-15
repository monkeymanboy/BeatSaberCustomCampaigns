using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatSaberCustomCampaigns.campaign
{
    public class CampainMapPosition
    {
        public int[] childNodes = new int[0];
        public float x;
        public float y;
        public float scale = 1; 
        public string letterPortion = "";
        public int numberPortion = 0;
        public string nodeDefaultColor = null;
        public string nodeHighlightColor = null;
        public string nodeOutlineLocation = null;
        public string nodeBackgroundLocation = null;
        [JsonIgnore]
        public Sprite nodeOutline = null;
        [JsonIgnore]
        public Sprite nodeBackground = null;
    }
}
