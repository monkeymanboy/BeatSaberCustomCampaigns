using HMUI;
using Newtonsoft.Json;
using SiraUtil.Affinity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomCampaigns.Managers
{
    public class CreditsManager : IAffinity
    {
        private const float SPACE_BETWEEN_TEXTS = 11.47f;
        private const float SPACE_BETWEEN_HEADERS = 22.94f;

        private bool _isCampaignCredits;
        private Campaign.Campaign _campaign;

        private List<Transform> _credits = new List<Transform>(0);

        public void OnCreditsFinish(CreditsScenesTransitionSetupDataSO creditsScenesTransitionSetupDataSO)
        {
            Plugin.logger.Debug("credits finished");
            _isCampaignCredits = false;
            creditsScenesTransitionSetupDataSO.didFinishEvent -= OnCreditsFinish;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(CreditsController), "Start")]
        public void CreditsControllerStartPrefix(CreditsController __instance)
        {
            if (!_isCampaignCredits)
            {
                return;
            }
            Plugin.logger.Debug("start");

            Transform title = __instance.gameObject.transform.GetChild(0).GetChild(0);
            Transform wrapper = __instance.gameObject.transform.GetChild(0).GetChild(1);
            
            CreateNewCredits(wrapper, title);
            HideOriginalCredits(wrapper);
        }

        internal void StartingCustomCampaignCredits(Campaign.Campaign campaign)
        {
            _isCampaignCredits = true;
            _campaign = campaign;
        }

        private void HideOriginalCredits(Transform wrapper)
        {
            Plugin.logger.Debug("hiding original credis");
            for (int i = _credits.Count; i < wrapper.childCount; i++)
            {
                wrapper.GetChild(i).gameObject.SetActive(false);
            }
        }

        private void CreateNewCredits(Transform wrapper, Transform title)
        {
            Credits credits = ParseCreditsJson();
            if (credits == null)
            {
                return;
            }

            title.GetComponent<CurvedTextMeshPro>().text = credits.name;
            title.GetComponent<CurvedTextMeshPro>().richText = true;

            Plugin.logger.Debug("creating new credits");
            var creditsTextItem = wrapper.Find("CreditsTextItem");
            var creditsHeaderItem = wrapper.Find("CreditsHeaderItem");
            //var creditsTitleItem = wrapper.Find("CreditsTitleItem");
            _credits = new List<Transform>(wrapper.childCount);
            Plugin.logger.Debug("hmm");

            var position = wrapper.GetChild(0).localPosition - new Vector3(0, SPACE_BETWEEN_TEXTS);
            foreach (var section in credits.credits)
            {
                var header = GameObject.Instantiate(creditsHeaderItem, wrapper);
                header.localPosition = position;
                header.GetComponent<CurvedTextMeshPro>().text = section.header.name;

                Plugin.logger.Debug($"created header: {section.header.name}");
                _credits.Add(header);
                foreach (var person in section.header.people)
                {
                    position -= new Vector3(0, SPACE_BETWEEN_TEXTS);
                    var text = GameObject.Instantiate(creditsTextItem, wrapper);
                    text.localPosition = position;
                    text.GetComponent<CurvedTextMeshPro>().text = person;
                    Plugin.logger.Debug($"created text: {person}");
                    _credits.Add(text);
                }

                position -= new Vector3(0, SPACE_BETWEEN_HEADERS);
            }

            Plugin.logger.Debug("reorganizing children");
            AddCustomCampaignCredits(wrapper, position);

            for (int i = _credits.Count - 1; i >= 0; i--)
            {
                _credits[i].SetAsFirstSibling();
                _credits[i].gameObject.SetActive(true);
            }
        }

        private Credits ParseCreditsJson()
        {
            Plugin.logger.Debug("parsing credits json");
            string rawJSON = File.ReadAllText(_campaign.campaignPath + "/" + "credits.json").Replace("\n", "");
            try
            {
                return JsonConvert.DeserializeObject<Credits>(rawJSON);
            }
            catch (Exception e)
            {
                Plugin.logger.Debug($"Error parsing credits: {e.Message}");
                return null;
            }
        }

        private void AddCustomCampaignCredits(Transform wrapper, Vector3 position)
        {
            var creditsTextItem = wrapper.Find("CreditsTextItem");
            var creditsHeaderItem = wrapper.Find("CreditsHeaderItem");


            var header = GameObject.Instantiate(creditsHeaderItem, wrapper);
            header.localPosition = position;
            header.GetComponent<CurvedTextMeshPro>().text = "Custom Campaigns Mod";
             _credits.Add(header);

            position -= new Vector3(0, SPACE_BETWEEN_TEXTS);
            var text1 = GameObject.Instantiate(creditsTextItem, wrapper);
            text1.localPosition = position;
            text1.GetComponent<CurvedTextMeshPro>().text = "PulseLane";
            _credits.Add(text1);

            position -= new Vector3(0, SPACE_BETWEEN_TEXTS);
            var text2 = GameObject.Instantiate(creditsTextItem, wrapper);
            text2.localPosition = position;
            text2.GetComponent<CurvedTextMeshPro>().text = "Original mod by - monkeymanboy";
            _credits.Add(text2);
        }
    }

    class Credits
    {
        public string name;
        public List<CreditsSection> credits;
    }

    class CreditsSection
    {
        public Header header;
    }

    class Header
    {
        public string name;
        public List<string> people;
    }
}
