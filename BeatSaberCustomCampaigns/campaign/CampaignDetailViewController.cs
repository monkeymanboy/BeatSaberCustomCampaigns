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
        TextPageScrollView scrollView;
        Button playButton;

        protected override void DidActivate(bool firstActivation, ActivationType type)
        {
            if (firstActivation)
            {
                rectTransform.anchorMin = new Vector3(0.5f, 0, 0);
                rectTransform.anchorMax = new Vector3(0.5f, 1, 0);
                rectTransform.sizeDelta = new Vector3(70, 0, 0);
                scrollView = Instantiate(Resources.FindObjectsOfTypeAll<TextPageScrollView>().First(x => x.transform.parent.gameObject.name == "ReleaseInfoViewController"), transform);
                scrollView.enabled = true;
                (scrollView.transform as RectTransform).anchorMin = new Vector3(0, 0.15f, 0);
                playButton = BeatSaberUI.CreateUIButton(rectTransform, "PlayButton", new Vector2(0,-34), new Vector2(0, 8.8f), delegate { clickedPlay?.Invoke(campaign); }, "Play");
            }
            SetDetailsToActive();
        }

        protected void SetDetailsToActive()
        {
            scrollView.SetText(campaign.info.bigDesc);
        }
    }
}
