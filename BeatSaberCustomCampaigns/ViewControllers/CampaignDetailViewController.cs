using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberCustomCampaigns.campaign
{
    public class CampaignDetailViewController : BSMLResourceViewController
    {
        public override string ResourceName => "BeatSaberCustomCampaigns.Views.campaign-detail.bsml";

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
        
        private string descriptionText;
        [UIValue("description-text")]
        public string DescriptionText
        {
            get => descriptionText;
            set
            {
                descriptionText = value;
                NotifyPropertyChanged();
            }
        }

        public Action<Campaign> clickedPlay;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (firstActivation)
            {
                rectTransform.anchorMin = new Vector3(0.5f, 0, 0);
                rectTransform.anchorMax = new Vector3(0.5f, 1, 0);
                rectTransform.sizeDelta = new Vector3(70, 0, 0);
            }
            SetDetailsToActive();
        }

        protected void SetDetailsToActive()
        {
            DescriptionText = campaign.info.bigDesc;
        }
        [UIAction("play")]
        private void ClickedPlay()
        {
            clickedPlay?.Invoke(campaign);
        }
    }
}
