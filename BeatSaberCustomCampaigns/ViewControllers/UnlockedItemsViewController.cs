using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

namespace BeatSaberCustomCampaigns
{
    public class UnlockedItemsViewController : BSMLResourceViewController
    {
        public override string ResourceName => "BeatSaberCustomCampaigns.Views.unlocked-items.bsml";

        [UIComponent("left-button")]
        private Button _pageLeftButton;
        [UIComponent("right-button")]
        private Button _pageRightButton;
        [UIComponent("text")]
        private TextMeshProUGUI _unlockText;

        public List<UnlockableItem> items;
        public int index;

        
        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (addedToHierarchy) UpdatePageStatus();
        }

        [UIAction("left-click")]
        public void PageLeft()
        {
            index--;
            if (index < 0) index = 0;
            UpdatePageStatus();
        }
        [UIAction("right-click")]
        public void PageRight()
        {
            index++;
            if (index >= items.Count) index = items.Count - 1;
            UpdatePageStatus();
        }
        public void UpdatePageStatus()
        {
            _pageLeftButton.gameObject.SetActive(index > 0);
            _pageRightButton.gameObject.SetActive(index < items.Count - 1);
            _unlockText.text = "Unlocked new " + items[index].type.ToString().ToLower() + " \"" + items[index].name + "\"\n<color=\"red\">You may need to restart your game in order to use this item";
        }
    }
}
