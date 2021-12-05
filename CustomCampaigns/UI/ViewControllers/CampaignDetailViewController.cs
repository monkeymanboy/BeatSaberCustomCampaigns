using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using System;
using UnityEngine;

namespace CustomCampaigns.UI.ViewControllers
{
    [ViewDefinition("CustomCampaigns.UI.Views.campaign-detail.bsml")]
    [HotReload(RelativePathToLayout = @"..\Views\campaign-detail.bsml")]
    public class CampaignDetailViewController : BSMLAutomaticViewController
    {
        private Campaign.Campaign _selectedCampaign;
        private string _descriptionText;

        public Action<Campaign.Campaign> DidClickPlayButton;

        public Campaign.Campaign SelectedCampaign
        {
            get => _selectedCampaign;
            set
            {
                _selectedCampaign = value;
                DescriptionText = _selectedCampaign.info.bigDesc;
            }
        }

        [UIValue("description-text")]
        public string DescriptionText
        {
            get => _descriptionText;
            set
            {
                _descriptionText = value;
                NotifyPropertyChanged();
            }
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (firstActivation)
            {
                rectTransform.anchorMin = new Vector3(0.5f, 0, 0);
                rectTransform.anchorMax = new Vector3(0.5f, 1, 0);
                rectTransform.sizeDelta = new Vector3(70, 0, 0);
            }

            DescriptionText = _selectedCampaign.info.bigDesc;
        }

        [UIAction("clickedPlay")]
        private void OnClickedPlay()
        {
            DidClickPlayButton?.Invoke(_selectedCampaign);
        }
    }
}
