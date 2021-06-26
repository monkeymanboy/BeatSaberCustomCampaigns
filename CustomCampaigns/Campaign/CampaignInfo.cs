using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CustomCampaigns.Campaign
{
    public class CampaignInfo
    {
        public string name;
        public string desc;
        public string bigDesc = "";
        public bool allUnlocked;
        public List<CampaignMapPosition> mapPositions = new List<CampaignMapPosition>();
        public List<CampaignUnlockGate> unlockGate = new List<CampaignUnlockGate>();
        public int mapHeight = 500;
        public float backgroundAlpha = 1;
        public CampaignLightColor lightColor = new CampaignLightColor(0.188f, 0.620f, 1);//if you're wondering what this is it's the games default light color
        public bool useStandardLevel = false;
    }

    public class CampaignMapPosition
    {
        public int[] childNodes = new int[0];
        public float x;
        public float y;
        public float scale = 1;
        public string letterPortion = "";
        public int numberPortion = 0;
        public string nodeDefaultColor;
        public string nodeHighlightColor;
        public string nodeOutlineLocation = null;
        public string nodeBackgroundLocation = null;
        [JsonIgnore]
        public Sprite nodeOutline = null;
        [JsonIgnore]
        public Sprite nodeBackground = null;
    }

    public class CampaignLightColor
    {
        public float r;
        public float g;
        public float b;
        public CampaignLightColor(float r, float g, float b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }
    }

    public struct CampaignUnlockGate
    {
        public int clearsToPass;
        public float x;
        public float y;
    }
}
