using CustomUI.BeatSaber;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRUI;

namespace BeatSaberCustomCampaigns
{
    public class UnlockedItemsViewController : VRUIViewController
    {
        private Button _pageLeftButton;
        private Button _pageRightButton;
        private TextMeshProUGUI _unlockText;

        public List<UnlockableItem> items;
        public int index;

        protected override void DidActivate(bool firstActivation, ActivationType type)
        {
            if (firstActivation)
            {
                _pageLeftButton = Instantiate(Resources.FindObjectsOfTypeAll<Button>().Last(x => (x.name == "PageLeftButton")), transform);
                RectTransform buttonTransform = _pageLeftButton.transform.Find("BG") as RectTransform;
                RectTransform glow = Instantiate(Resources.FindObjectsOfTypeAll<GameObject>().Last(x => (x.name == "GlowContainer")), _pageLeftButton.transform).transform as RectTransform;
                glow.localPosition = buttonTransform.localPosition;
                glow.anchoredPosition = buttonTransform.anchoredPosition;
                glow.anchorMin = buttonTransform.anchorMin;
                glow.anchorMax = buttonTransform.anchorMax;
                glow.sizeDelta = buttonTransform.sizeDelta;
                _pageLeftButton.transform.localPosition = new Vector3(-70, 0, 0);
                _pageLeftButton.interactable = true;
                _pageLeftButton.onClick.AddListener(delegate ()
                {
                    PageLeft();
                });
                _pageRightButton = Instantiate(Resources.FindObjectsOfTypeAll<Button>().Last(x => (x.name == "PageRightButton")), transform);
                buttonTransform = _pageRightButton.transform.Find("BG") as RectTransform;
                glow = Instantiate(Resources.FindObjectsOfTypeAll<GameObject>().Last(x => (x.name == "GlowContainer")), _pageRightButton.transform).transform as RectTransform;
                glow.localPosition = buttonTransform.localPosition;
                glow.anchoredPosition = buttonTransform.anchoredPosition;
                glow.anchorMin = buttonTransform.anchorMin;
                glow.anchorMax = buttonTransform.anchorMax;
                glow.sizeDelta = buttonTransform.sizeDelta;
                _pageRightButton.transform.localPosition = new Vector3(70, 0, 0);
                _pageRightButton.interactable = true;
                _pageRightButton.onClick.AddListener(delegate ()
                {
                    PageRight();
                });
                _unlockText = BeatSaberUI.CreateText(rectTransform, "", new Vector2(0, 0));
                _unlockText.alignment = TextAlignmentOptions.Center;
                _unlockText.fontSize = 6;
            }
            if (type == ActivationType.AddedToHierarchy) UpdatePageStatus();
        }
        public void PageLeft()
        {
            index--;
            if (index < 0) index = 0;
            UpdatePageStatus();
        }
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
