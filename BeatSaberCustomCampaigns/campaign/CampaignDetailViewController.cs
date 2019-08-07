using CustomUI.BeatSaber;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberCustomCampaigns.campaign
{
    public class CampaignDetailViewController : VRUI.VRUIViewController
    {
        public Action<Campaign> clickedPlay;
        public Campaign campaign {
            get
            {
                return activeCampaign;
            }
            set
            {
                activeCampaign = value;
                if (isInViewControllerHierarchy) SetDetailsToActive();
            }
        }
        private Campaign activeCampaign;
        TextMeshProUGUI text;
        TextMeshProUGUI desc;
        Button playButton;

        protected override void DidActivate(bool firstActivation, ActivationType type)
        {
            if (firstActivation)
            {
                rectTransform.anchorMin = new Vector3(0.5f, 0, 0);
                rectTransform.anchorMax = new Vector3(0.5f, 1, 0);
                rectTransform.sizeDelta = new Vector3(65, 0, 0);
                text = BeatSaberUI.CreateText(rectTransform, "Placeholder Text", new Vector2(0, 35));
                text.alignment = TextAlignmentOptions.Center;
                text.fontSize = 6;
                desc = BeatSaberUI.CreateText(rectTransform, "Placeholder Text", new Vector2(0, 25));
                desc.alignment = TextAlignmentOptions.Top;
                desc.fontSize = 4;
                desc.lineSpacing = -15f;
                desc.paragraphSpacing = -15f;
                desc.enableWordWrapping = true;
                desc.overflowMode = TextOverflowModes.Overflow;
                playButton = BeatSaberUI.CreateUIButton(rectTransform, "PlayButton", new Vector2(0,-30), new Vector2(0, 8.8f), delegate { clickedPlay?.Invoke(campaign); }, "Play");
                SetDetailsToActive();
            }
        }

        protected void SetDetailsToActive()
        {
            text.text = campaign.info.name;
            desc.text = campaign.info.bigDesc;
        }
    }
}
