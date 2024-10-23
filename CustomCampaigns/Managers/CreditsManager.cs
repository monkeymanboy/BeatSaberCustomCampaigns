using HMUI;
using IPA.Utilities;
using Newtonsoft.Json;
using SiraUtil.Affinity;
using SongCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace CustomCampaigns.Managers
{
    public class CreditsManager : IAffinity
    {
        private const float SPACE_BETWEEN_TEXTS = 11.47f;
        private const float SPACE_BETWEEN_HEADERS = 22.94f;

        private bool _isCampaignCredits;
        private Campaign.Campaign _campaign;

        private List<Transform> _credits = new List<Transform>(0);

        private Action<AudioClip> AudioLoaded;
        private CreditsController _creditsController;

        [AffinityPrefix]
        [AffinityPatch(typeof(CreditsController), "Start")]
        public void CreditsControllerStartPrefix(CreditsController __instance)
        {
            if (!_isCampaignCredits)
            {
                return;
            }
            _creditsController = __instance;

            Transform title = __instance.gameObject.transform.GetChild(0).GetChild(0);
            Transform wrapper = __instance.gameObject.transform.GetChild(0).GetChild(1);

            CreateNewCredits(wrapper, title);
            HideOriginalCredits(wrapper);
            _isCampaignCredits = false;
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
                //wrapper.GetChild(i).gameObject.SetActive(false);
                GameObject.Destroy(wrapper.GetChild(i).gameObject);
            }
        }

        private void CreateNewCredits(Transform wrapper, Transform title)
        {
            Credits credits = ParseCreditsJson();
            if (credits == null)
            {
                return;
            }

            PlayNewAudio(credits);

            title.GetComponent<CurvedTextMeshPro>().text = credits.name;
            title.GetComponent<CurvedTextMeshPro>().richText = true;

            Plugin.logger.Debug("creating new credits");
            var creditsTextItem = wrapper.Find("CreditsTextItem");
            var creditsHeaderItem = wrapper.Find("CreditsHeaderItem");
            var creditsTitleItem = wrapper.Find("CreditsTitleItem");
            _credits = new List<Transform>(wrapper.childCount);

            // only separate the first header by the text offset
            var position = wrapper.GetChild(0).localPosition + new Vector3(0, SPACE_BETWEEN_HEADERS - SPACE_BETWEEN_TEXTS);
            foreach (var section in credits.credits)
            {
                if (section.header != null)
                {
                    AddHeader(section.header, wrapper, creditsHeaderItem, creditsTitleItem, creditsTextItem, ref position);

                }
                else
                {
                    AddTitle(section.title, wrapper, creditsTitleItem, creditsTextItem, ref position);

                }
            }

            AddCustomCampaignCredits(wrapper, position);

            for (int i = _credits.Count - 1; i >= 0; i--)
            {
                _credits[i].SetAsFirstSibling();
                _credits[i].gameObject.SetActive(true);
            }
        }

        private void AddHeader(Header header, Transform wrapper, Transform creditsHeaderItem, Transform creditsTitleItem, Transform creditsTextItem, ref Vector3 position)
        {
            position -= new Vector3(0, SPACE_BETWEEN_HEADERS);
            var headerItem = GameObject.Instantiate(creditsHeaderItem, wrapper);
            headerItem.localPosition = position;
            headerItem.GetComponent<CurvedTextMeshPro>().text = header.name;

            _credits.Add(headerItem);
            AddGroupedTitles(header.titles, wrapper, creditsTitleItem, creditsTextItem, ref position);
        }

        private void AddGroupedTitles(List<Title> titles, Transform wrapper, Transform creditsTitleItem, Transform creditsTextItem, ref Vector3 position)
        {

            if (titles.Count == 0)
            {
                return;
            }
            else if (titles.Count == 1)
            {
                AddTitle(titles[0], wrapper, creditsTitleItem, creditsTextItem, ref position);
            }
            else if (titles.Count == 2)
            {
                var originalY = position.y;
                position -= new Vector3(50f, 0);

                AddTitle(titles[0], wrapper, creditsTitleItem, creditsTextItem, ref position);
                position += new Vector3(100f, originalY - position.y);
                AddTitle(titles[1], wrapper, creditsTitleItem, creditsTextItem, ref position);
                // reset back to original x
                position -= new Vector3(50f, 0);

            }
            else
            {
                var originalY = position.y;
                position -= new Vector3(200f, 0);
                for (int i = 0; i < 3; i++)
                {
                    position += new Vector3(100f, originalY - position.y);
                    AddTitle(titles[i], wrapper, creditsTitleItem, creditsTextItem, ref position);
                }
                // reset back to original x
                position -= new Vector3(100f, 0);
                if (titles.Count > 3)
                {
                    AddGroupedTitles(titles.GetRange(3, titles.Count - 3), wrapper, creditsTitleItem, creditsTextItem, ref position);
                }
            }
        }

        private void AddTitle(Title title, Transform wrapper, Transform creditsTitleItem, Transform creditsTextItem, ref Vector3 position)
        {
            position -= new Vector3(0, SPACE_BETWEEN_HEADERS);
            var titleItem = GameObject.Instantiate(creditsTitleItem, wrapper);
            titleItem.localPosition = position;
            titleItem.GetComponent<CurvedTextMeshPro>().text = title.name;

            _credits.Add(titleItem);
            foreach (var person in title.people)
            {
                position -= new Vector3(0, SPACE_BETWEEN_TEXTS);
                var text = GameObject.Instantiate(creditsTextItem, wrapper);
                text.localPosition = position;
                text.GetComponent<CurvedTextMeshPro>().text = person;
                _credits.Add(text);
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
            Title currentMaintainer = new Title("Current Mod", new List<string> { "PulseLane" });
            Title oldMaintainer = new Title("Original Mod", new List<string> { "monkeymanboy" });

            Header modCredits = new Header("Custom Campaigns Mod", new List<Title> { currentMaintainer, oldMaintainer });

            var creditsTextItem = wrapper.Find("CreditsTextItem");
            var creditsHeaderItem = wrapper.Find("CreditsHeaderItem");
            var creditsTitleItem = wrapper.Find("CreditsTitleItem");

            AddHeader(modCredits, wrapper, creditsHeaderItem, creditsTitleItem, creditsTextItem, ref position);
        }

        private void PlayNewAudio(Credits credits)
        {
            Plugin.logger.Debug("audio time");
            if (credits.mapAudioHash == null || credits.mapAudioHash == "")
            {
                Plugin.logger.Debug("no hash present");
                return;
            }

            List<string> levelIDs = SongCore.Collections.levelIDsForHash(credits.mapAudioHash);
            if (levelIDs.Count == 0)
            {
                Plugin.logger.Debug("no levels matching hash");
                return;
            }

            BeatmapLevel beatmapLevel = Loader.CustomLevels.Values.First(x => levelIDs.Contains(x.levelID));
            if (beatmapLevel == null)
            {
                Plugin.logger.Debug("no levels matching");
                return;
            }

            AudioLoaded -= OnAudioLoad;
            AudioLoaded += OnAudioLoad;
            LoadAudio(beatmapLevel.levelID);
        }

        private void OnAudioLoad(AudioClip audioClip)
        {
            Plugin.logger.Debug("got audio clip");
            var audioPlayer = _creditsController.GetField<AudioPlayerBase, CreditsController>("_audioPlayer") as SimpleAudioPlayer;
            if (audioPlayer == null)
            {
                Plugin.logger.Error("Wrong audio player type");
                return;
            }

            audioPlayer.SetField("_audioClip", audioClip);
            audioPlayer.GetField<AudioSource, SimpleAudioPlayer>("_audioSource").clip = audioClip;
            audioPlayer.GetField<AudioSource, SimpleAudioPlayer>("_audioSource").Play();
        }

        private async void LoadAudio(string levelID)
        {
            Plugin.logger.Debug($"Loading beatmap: {levelID}");
            AudioClip audioClip = await Loader.BeatmapLevelsModelSO.GetBeatmapLevel(levelID).previewMediaData.GetPreviewAudioClip();
            AudioLoaded?.Invoke(audioClip);
        }
    }

    class Credits
    {
        public string name;
        public List<CreditsSection> credits;
        public string mapAudioHash;
    }

    class CreditsSection
    {
        public Header header;
        public Title title;
    }

    class Header
    {
        public string name;
        public List<Title> titles;

        public Header(string name, List<Title> titles)
        {
            this.name = name;
            this.titles = titles;
        }
    }

    class Title
    {
        public string name;
        public List<string> people;

        public Title(string name, List<string> people)
        {
            this.name = name;
            this.people = people;
        }
    }
}
