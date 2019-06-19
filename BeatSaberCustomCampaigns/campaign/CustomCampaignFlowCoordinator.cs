using BS_Utils.Gameplay;
using CustomUI.BeatSaber;
using IPA;
using Newtonsoft.Json;
using SongCore;
using SongCore.Data;
using SongCore.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using VRUI;
using static SongCore.Data.ExtraSongData;

namespace BeatSaberCustomCampaigns.campaign
{
    public class CustomCampaignFlowCoordinator : FlowCoordinator
    {
        public const float EDITOR_TO_GAME_UNITS = 30f / 111;
        public const float HEIGHT_OFFSET = 20;

        public MainFlowCoordinator _mainFlowCoordinator;
        public CampaignFlowCoordinator _campaignFlowCoordinator;

        public MissionMapAnimationController _missionMapAnimationController;
        public MissionNodesManager _missionNodesManager;
        public MissionStagesManager _missionStagesManager;
        public MissionConnectionsGenerator _missionConnectionsGenerator;
        public MissionSelectionMapViewController _missionSelectionMapViewController;
        public MissionNodeSelectionManager _missionNodeSelectionManager;
        public DismissableNavigationController _navigationController;
        public MissionLevelDetailViewController _missionLevelDetailViewController;
        public MissionResultsViewController _missionResultsViewController;
        public Button _playButton;
        public ScrollView _mapScrollView;
        public ScrollViewItemsVisibilityController _mapScrollViewItemsVisibilityController;
        public Image _backgroundImage;
        public GameplayModifierInfoListItemsList _gameplayModifierInfoListItemsList;
        public GameObject _modifiersPanelGO;
        public GameplayModifiersModelSO _gameplayModifiersModel;

        public Button _pageUpModifiersButton;
        public Button _pageDownModifiersButton;
        public TextMeshProUGUI _challengeName;

        public CampaignProgressModelSO _campaignProgressModel;

        protected CampaignListViewController _campaignListViewController;
        protected CampaignChallengeLeaderboardViewController _campaignChallengeLeaderbaordViewController;
        protected UnlockedItemsViewController _unlockedItemsViewController;

        MissionNode[] baseNodes;
        MissionNode baseRoot;
        MissionNode baseFinal;
        MissionStage[] baseMissionStages;
        Sprite baseBackground;
        float baseBackAlpha;
        float baseMapHeight;

        MissionNode[] curCampaignNodes;
        MissionStage[] curMissionStages;

        bool isDownloading = false;
        MissionNode downloadingNode;

        List<GameplayModifierParamsSO> modifierParamsList;
        int modifierParamsPageNumber;

        public static bool unlockAllMissions = false;

        protected override void DidActivate(bool firstActivation, ActivationType activationType)
        {
            if (firstActivation)
            {
                base.title = "Custom Campaigns";
                _campaignFlowCoordinator = Resources.FindObjectsOfTypeAll<CampaignFlowCoordinator>().First();
                _missionMapAnimationController = Resources.FindObjectsOfTypeAll<MissionMapAnimationController>().First();
                _missionNodesManager = Resources.FindObjectsOfTypeAll<MissionNodesManager>().First();
                _missionStagesManager = Resources.FindObjectsOfTypeAll<MissionStagesManager>().First();
                _missionConnectionsGenerator = Resources.FindObjectsOfTypeAll<MissionConnectionsGenerator>().First();
                _missionSelectionMapViewController = Resources.FindObjectsOfTypeAll<MissionSelectionMapViewController>().First();
                _missionNodeSelectionManager = Resources.FindObjectsOfTypeAll<MissionNodeSelectionManager>().First();
                _missionLevelDetailViewController = Resources.FindObjectsOfTypeAll<MissionLevelDetailViewController>().First();
                _missionResultsViewController = Resources.FindObjectsOfTypeAll<MissionResultsViewController>().First();

                _playButton = _missionLevelDetailViewController.GetPrivateField<Button>("_playButton");
                _mapScrollView = _missionSelectionMapViewController.GetPrivateField<ScrollView>("_mapScrollView");
                _mapScrollViewItemsVisibilityController = _mapScrollView.GetComponent<ScrollViewItemsVisibilityController>();
                _backgroundImage = _mapScrollView.GetComponentsInChildren<Image>().First(x => x.name == "Map");
                _navigationController = _campaignFlowCoordinator.GetPrivateField<DismissableNavigationController>("_navigationController");
                _gameplayModifierInfoListItemsList = _missionLevelDetailViewController.GetPrivateField<GameplayModifierInfoListItemsList>("_gameplayModifierInfoListItemsList");
                _modifiersPanelGO = _missionLevelDetailViewController.GetPrivateField<GameObject>("_modifiersPanelGO");
                _gameplayModifiersModel = _missionLevelDetailViewController.GetPrivateField<GameplayModifiersModelSO>("_gameplayModifiersModel");

                _pageUpModifiersButton = Instantiate(Resources.FindObjectsOfTypeAll<Button>().Last(x => (x.name == "PageUpButton")), _modifiersPanelGO.transform.parent.parent);

                _pageUpModifiersButton.transform.localPosition = new Vector3(0, 1f, 0);
                _pageUpModifiersButton.transform.localScale = new Vector3(1, 0.5f, 1);
                _pageUpModifiersButton.interactable = true;
                _pageUpModifiersButton.onClick.AddListener(delegate ()
                {
                    ModifiersPageUp();
                });
                _pageDownModifiersButton = Instantiate(Resources.FindObjectsOfTypeAll<Button>().Last(x => (x.name == "PageDownButton")), _modifiersPanelGO.transform.parent.parent);
                _pageDownModifiersButton.transform.localPosition = new Vector3(0, -17f, 0);
                _pageDownModifiersButton.transform.localScale = new Vector3(1, 0.5f, 1);
                _pageDownModifiersButton.interactable = true;
                _pageDownModifiersButton.onClick.AddListener(delegate ()
                {
                    ModifiersPageDown();
                });
                _challengeName = BeatSaberUI.CreateText((_modifiersPanelGO.transform.parent.parent as RectTransform), "Challenge Name", new Vector2(0, 38));
                _challengeName.alignment = TextAlignmentOptions.Bottom;

                _campaignProgressModel = _campaignFlowCoordinator.GetPrivateField<CampaignProgressModelSO>("_campaignProgressModel");

                _campaignListViewController = BeatSaberUI.CreateViewController<CampaignListViewController>();
                _campaignChallengeLeaderbaordViewController = BeatSaberUI.CreateViewController<CampaignChallengeLeaderboardViewController>();
                _unlockedItemsViewController = BeatSaberUI.CreateViewController<UnlockedItemsViewController>();

                _campaignListViewController.clickedCampaign += OpenCampaign;
                _campaignListViewController.backPressed += Dismiss;
            }
            if (activationType == ActivationType.AddedToHierarchy)
            {
                SetBaseCampaignEnabled(false);
                ProvideInitialViewControllers(_campaignListViewController);
            }
        }

        protected override void DidDeactivate(DeactivationType deactivationType)
        {
            if (deactivationType == DeactivationType.RemovedFromHierarchy)
            {
                SetBaseCampaignEnabled(true);
            }
        }

        public void SetBaseCampaignEnabled(bool enabled)
        {
            try
            {
                if (!enabled)
                {
                    _mapScrollView.OnDestroy();
                    _mapScrollView.Awake();
                    if (!_missionNodesManager.IsInitialized)_missionNodesManager.Awake();
                    baseNodes = _missionNodesManager.GetPrivateField<MissionNode[]>("_allMissionNodes");
                    baseRoot = _missionNodesManager.GetPrivateField<MissionNode>("_rootMissionNode");
                    baseFinal = _missionNodesManager.GetPrivateField<MissionNode>("_finalMissionNode");
                    baseMissionStages = _missionStagesManager.GetPrivateField<MissionStage[]>("_missionStages");
                    baseBackground = _backgroundImage.sprite;
                    baseBackAlpha = _backgroundImage.color.a;
                    baseMapHeight = _mapScrollView.GetPrivateField<RectTransform>("_contentRectTransform").sizeDelta.y;
                }
                foreach (MissionNode node in baseNodes)
                {
                    node.transform.localPosition += new Vector3(enabled ? 10000 : -10000, 0, 0);
                    node.gameObject.SetActive(true);
                }
                if (enabled)
                {
                    _missionNodesManager.SetPrivateField("_rootMissionNode", baseRoot);
                    _missionNodesManager.SetPrivateField("_finalMissionNode", baseFinal);
                    _missionStagesManager.SetPrivateField("_missionStages", baseMissionStages);
                     _backgroundImage.sprite = baseBackground;
                    _backgroundImage.color = new Color(1, 1, 1, baseBackAlpha);
                    _mapScrollView.GetPrivateField<RectTransform>("_contentRectTransform").SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, baseMapHeight);
                    CampaignInit();
                }
                _pageUpModifiersButton.gameObject.SetActive(!enabled);
                _pageDownModifiersButton.gameObject.SetActive(!enabled);
                _challengeName.gameObject.SetActive(!enabled);
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
        public void CloseCampaign(DismissableNavigationController navigationController)
        {
            _campaignFlowCoordinator.SetProperty("title", "Campaign");
            _navigationController.didFinishEvent += _campaignFlowCoordinator.HandleNavigationControllerDidFinish;
            _navigationController.didFinishEvent -= CloseCampaign;
            _missionNodeSelectionManager.didSelectMissionNodeEvent -= HandleMissionNodeSelectionManagerDidSelectMissionNode;
            _missionLevelDetailViewController.didPressPlayButtonEvent += _campaignFlowCoordinator.HandleMissionLevelDetailViewControllerDidPressPlayButton;
            _missionResultsViewController.retryButtonPressedEvent -= HandleMissionResultsViewControllerRetryButtonPressed;
            _missionSelectionMapViewController.didSelectMissionLevelEvent -= HandleMissionSelectionMapViewControllerDidSelectMissionLevel;
            _missionResultsViewController.continueButtonPressedEvent -= HandleMissionResultsViewControllerContinueButtonPressed;
            _missionLevelDetailViewController.didPressPlayButtonEvent -= HandleMissionLevelDetailViewControllerDidPressPlayButtonPlay;
            _missionLevelDetailViewController.didPressPlayButtonEvent -= HandleMissionLevelDetailViewControllerDidPressPlayButtonDownload;
            _mapScrollView.GetPrivateField<RectTransform>("_contentRectTransform").SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, baseMapHeight);
            unlockAllMissions = false;
            foreach (MissionNode node in curCampaignNodes)
            {
                Destroy(node.gameObject);
            }
            foreach (MissionStage stage in curMissionStages)
            {
                Destroy(stage.gameObject);
            }
            DismissFlowCoordinator(_campaignFlowCoordinator);
        }
        public void CampaignInit()
        {
            if (_missionNodeSelectionManager.GetPrivateField<MissionNode[]>("_missionNodes") == null) _missionNodeSelectionManager.Start();
            _missionNodeSelectionManager.OnDestroy();
            ResetProgressIds();
            _missionNodesManager.Awake();
            _missionNodeSelectionManager.Start();
            _missionConnectionsGenerator.CreateNodeConnections();
            _missionNodesManager.ResetAllNodes();
            _missionNodesManager.SetupNodeConnections();
            _missionMapAnimationController.ScrollToTopMostNotClearedMission();
            _mapScrollView.OnDestroy();
            _mapScrollView.Awake();
            _mapScrollViewItemsVisibilityController.Start();
        }
        public void OpenCampaign(Campaign campaign)
        {
            try
            {
                unlockAllMissions = campaign.info.allUnlocked;
                _backgroundImage.color = new Color(1, 1, 1, campaign.info.backgroundAlpha);
                _backgroundImage.sprite = campaign.background;
                MissionNode[] missionNodes = new MissionNode[campaign.info.mapPositions.Count];
                curCampaignNodes = missionNodes;
                MissionStage[] missionStages;
                if (campaign.info.unlockGate.Count == 0 || unlockAllMissions)
                {
                    //campaigns require an unlock gate so I make a fake one above the visible map area
                    missionStages = new MissionStage[1];
                    missionStages[0] = Instantiate(baseMissionStages[0], _missionNodesManager.GetPrivateField<GameObject>("_missionNodesParentObject").transform);
                    missionStages[0].SetPrivateField("_minimumMissionsToUnlock", 0);
                    missionStages[0].GetPrivateField<RectTransform>("_rectTransform").localPosition = new Vector3(0, -baseMapHeight / 2 + campaign.info.mapHeight * EDITOR_TO_GAME_UNITS + HEIGHT_OFFSET / 2 + 1000, 0);
                } else
                {
                    missionStages = new MissionStage[campaign.info.unlockGate.Count + 1];
                    for (int i = 0; i < missionStages.Length-1; i++)
                    {
                        missionStages[i] = Instantiate(baseMissionStages[0], _missionNodesManager.GetPrivateField<GameObject>("_missionNodesParentObject").transform);
                        missionStages[i].SetPrivateField("_minimumMissionsToUnlock", campaign.info.unlockGate[i].clearsToPass);
                        missionStages[i].GetPrivateField<RectTransform>("_rectTransform").localPosition = new Vector3(campaign.info.unlockGate[i].x * EDITOR_TO_GAME_UNITS, -baseMapHeight/ 2 + campaign.info.mapHeight * EDITOR_TO_GAME_UNITS +HEIGHT_OFFSET/2 - campaign.info.unlockGate[i].y * EDITOR_TO_GAME_UNITS, 0);
                    }
                    //ghost unlock gate required for some reason as last unlock gate never gets cleared
                    missionStages[campaign.info.unlockGate.Count] = Instantiate(baseMissionStages[0], _missionNodesManager.GetPrivateField<GameObject>("_missionNodesParentObject").transform);
                    missionStages[campaign.info.unlockGate.Count].SetPrivateField("_minimumMissionsToUnlock", campaign.info.mapPositions.Count+1);
                    missionStages[campaign.info.unlockGate.Count].GetPrivateField<RectTransform>("_rectTransform").localPosition = new Vector3(0, -baseMapHeight / 2 + campaign.info.mapHeight * EDITOR_TO_GAME_UNITS + HEIGHT_OFFSET / 2 + 1000, 0);
                }
                curMissionStages = missionStages;
                _missionStagesManager.SetPrivateField("_missionStages" , (from stage in missionStages orderby stage.minimumMissionsToUnlock select stage).ToArray());
                for (int i = 0; i < missionNodes.Length; i++)
                {
                    CampainMapPosition mapPosition = campaign.info.mapPositions[i];
                    missionNodes[i] = Instantiate(baseNodes[0], _missionNodesManager.GetPrivateField<GameObject>("_missionNodesParentObject").transform);
                    missionNodes[i].gameObject.SetActive(true);
                    missionNodes[i].SetPrivateField("_missionDataSO", campaign.challenges[i].GetMissionData(campaign));
                    missionNodes[i].GetPrivateField<RectTransform>("_rectTransform").localPosition = new Vector3(mapPosition.x * EDITOR_TO_GAME_UNITS, -baseMapHeight / 2 + campaign.info.mapHeight*EDITOR_TO_GAME_UNITS+HEIGHT_OFFSET/2 - mapPosition.y * EDITOR_TO_GAME_UNITS, 0);
                    missionNodes[i].GetPrivateField<RectTransform>("_rectTransform").sizeDelta = new Vector2(12*mapPosition.scale, 12*mapPosition.scale);
                    missionNodes[i].SetPrivateField("_letterPartName", mapPosition.letterPortion);
                    missionNodes[i].SetPrivateField("_numberPartName", mapPosition.numberPortion);
                }
                for (int i = 0; i < missionNodes.Length; i++)
                {   
                    MissionNode[] children = new MissionNode[campaign.info.mapPositions[i].childNodes.Length];
                    for(int j = 0; j < children.Length; j++)
                    {
                        children[j] = missionNodes[campaign.info.mapPositions[i].childNodes[j]];
                    }
                    missionNodes[i].SetPrivateField("_childNodes", children);
                }
                _missionNodesManager.SetPrivateField("_rootMissionNode", missionNodes[0]);
                _mapScrollView.GetPrivateField<RectTransform>("_contentRectTransform").SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, campaign.info.mapHeight * EDITOR_TO_GAME_UNITS+HEIGHT_OFFSET);
                CampaignInit();
                PresentFlowCoordinator(_campaignFlowCoordinator, delegate()
                {
                    _campaignFlowCoordinator.SetProperty("title", campaign.info.name);
                    _missionNodeSelectionManager.didSelectMissionNodeEvent -= _missionSelectionMapViewController.HandleMissionNodeSelectionManagerDidSelectMissionNode;
                    _missionLevelDetailViewController.didPressPlayButtonEvent -= _campaignFlowCoordinator.HandleMissionLevelDetailViewControllerDidPressPlayButton;
                    _missionResultsViewController.retryButtonPressedEvent += HandleMissionResultsViewControllerRetryButtonPressed;
                    _missionResultsViewController.retryButtonPressedEvent -= _campaignFlowCoordinator.HandleMissionResultsViewControllerRetryButtonPressed;
                    _navigationController.didFinishEvent -= _campaignFlowCoordinator.HandleNavigationControllerDidFinish;
                    _navigationController.didFinishEvent += CloseCampaign;
                    _missionSelectionMapViewController.didSelectMissionLevelEvent += HandleMissionSelectionMapViewControllerDidSelectMissionLevel;
                    _missionResultsViewController.continueButtonPressedEvent += HandleMissionResultsViewControllerContinueButtonPressed;
                    _missionMapAnimationController.ScrollToTopMostNotClearedMission();
                    _playButton.interactable = true;

                    //for some reason highlighting doesn't work the first time the campaign opens so this fixes that
                    foreach(MissionNode missionNode in missionNodes)
                    {
                        missionNode.missionNodeVisualController.nodeWasSelectEvent += delegate
                        {
                            missionNode.missionNodeVisualController.GetPrivateField<MissionToggle>("_missionToggle").SetPrivateField("_selected", true);
                            missionNode.missionNodeVisualController.GetPrivateField<MissionToggle>("_missionToggle").InvokeMethod("RefreshUI");
                        };
                    }
                });
                _missionNodeSelectionManager.didSelectMissionNodeEvent += HandleMissionNodeSelectionManagerDidSelectMissionNode;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
        public void HandleMissionNodeSelectionManagerDidSelectMissionNode(MissionNodeVisualController missionNodeVisualController)
        {
            if (isDownloading) return;
            _missionLevelDetailViewController.didPressPlayButtonEvent -= _campaignFlowCoordinator.HandleMissionLevelDetailViewControllerDidPressPlayButton;
            _missionLevelDetailViewController.didPressPlayButtonEvent -= HandleMissionLevelDetailViewControllerDidPressPlayButtonPlay;
            _missionLevelDetailViewController.didPressPlayButtonEvent -= HandleMissionLevelDetailViewControllerDidPressPlayButtonDownload;
            CustomPreviewBeatmapLevel song = ((CustomMissionDataSO)missionNodeVisualController.missionNode.missionData).customLevel;
            if (song==null)
            {
                _missionSelectionMapViewController.HandleMissionNodeSelectionManagerDidSelectMissionNode(missionNodeVisualController);
                _playButton.SetButtonText("DOWNLOAD");
                _missionLevelDetailViewController.didPressPlayButtonEvent += HandleMissionLevelDetailViewControllerDidPressPlayButtonDownload;
                downloadingNode = missionNodeVisualController.missionNode;
            }
            else
            {
                _playButton.SetButtonText("PLAY");
                LoadBeatmap(missionNodeVisualController, (missionNodeVisualController.missionNode.missionData as CustomMissionDataSO).customLevel.levelID);
            }
        }
        public async void LoadBeatmap(MissionNodeVisualController missionNodeVisualController, string songid)
        {
            await Loader.BeatmapLevelsModelSO.GetBeatmapLevelAsync(songid, CancellationToken.None);
            _missionLevelDetailViewController.didPressPlayButtonEvent += HandleMissionLevelDetailViewControllerDidPressPlayButtonPlay;
            _missionSelectionMapViewController.HandleMissionNodeSelectionManagerDidSelectMissionNode(missionNodeVisualController);
        }

        public void Dismiss()
        {
            (_mainFlowCoordinator as FlowCoordinator).InvokePrivateMethod("DismissFlowCoordinator", new object[] { this, null, false });
        }
        public void HandleMissionLevelDetailViewControllerDidPressPlayButtonDownload(MissionLevelDetailViewController viewController)
        {
            Challenge challenge = ((CustomMissionDataSO)viewController.missionNode.missionData).challenge;
            StartCoroutine(DownloadSongCoroutine(challenge.songid, challenge.GetDownloadURL(), challenge.customDownloadURL == ""));

        }
        public void HandleMissionLevelDetailViewControllerDidPressPlayButtonPlay(MissionLevelDetailViewController viewController)
        {
            Challenge challenge = ((CustomMissionDataSO)viewController.missionNode.missionData).challenge;
            MissionDataSO missionData = viewController.missionNode.missionData;
            String failedMods = LoadExternalModifiers(challenge);
            List<GameplayModifierParamsSO> errorList = new List<GameplayModifierParamsSO>();
            if (failedMods.Length>0)
            {
                foreach(string s in failedMods.Split(' '))
                {
                    GameplayModifierParamsSO errorParam = ScriptableObject.CreateInstance<GameplayModifierParamsSO>();
                    errorParam.SetPrivateField("_modifierName", "Error - External Mod");
                    errorParam.SetPrivateField("_hintText", "Please install or update the following mod: " + s);
                    errorParam.SetPrivateField("_icon", Assets.ErrorIcon);
                    errorList.Add(errorParam);
                }
            }
            if (viewController.missionNode.missionData.beatmapCharacteristic.hintText == "ERROR NOT FOUND")
            {
                GameplayModifierParamsSO errorParam = ScriptableObject.CreateInstance<GameplayModifierParamsSO>();
                errorParam.SetPrivateField("_modifierName", "Error - Characteristic Not Found");
                errorParam.SetPrivateField("_hintText", "Could not find the characteristic \"" + challenge.characteristic + "\" for this map");
                errorParam.SetPrivateField("_icon", Assets.ErrorIcon);
                errorList.Add(errorParam);
            }
            else if (BeatmapLevelDataExtensions.GetDifficultyBeatmap(Loader.BeatmapLevelsModelSO.GetBeatmapLevelIfLoaded((missionData as CustomMissionDataSO).customLevel.levelID).beatmapLevelData, missionData.beatmapCharacteristic, missionData.beatmapDifficulty) == null)
            {
                GameplayModifierParamsSO errorParam = ScriptableObject.CreateInstance<GameplayModifierParamsSO>();
                errorParam.SetPrivateField("_modifierName", "Error - Difficulty Not Found");
                errorParam.SetPrivateField("_hintText", "Could not find the difficulty \"" + challenge.difficulty.ToString() + "\" for this map");
                errorParam.SetPrivateField("_icon", Assets.ErrorIcon);
                errorList.Add(errorParam);
            }
            else
            {
                DifficultyData difficultyData = Collections.RetrieveDifficultyData(BeatmapLevelDataExtensions.GetDifficultyBeatmap(Loader.BeatmapLevelsModelSO.GetBeatmapLevelIfLoaded((missionData as CustomMissionDataSO).customLevel.levelID).beatmapLevelData, missionData.beatmapCharacteristic, missionData.beatmapDifficulty));
                foreach (string requirement in difficultyData.additionalDifficultyData._requirements)
                {
                    if (Collections.capabilities.Contains(requirement) || requirement.StartsWith("Complete Campaign Challenge - ")) continue;
                    GameplayModifierParamsSO errorParam = ScriptableObject.CreateInstance<GameplayModifierParamsSO>();
                    errorParam.SetPrivateField("_modifierName", "Error - Missing Level Requirement");
                    errorParam.SetPrivateField("_hintText", "Could not find the capability to play levels with \"" + requirement + "\"");
                    errorParam.SetPrivateField("_icon", Assets.ErrorIcon);
                    errorList.Add(errorParam);
                }
            }
            if (errorList.Count==0)
            {
                Gamemode.NextLevelIsIsolated("Challenges");

                _campaignFlowCoordinator.HandleMissionLevelDetailViewControllerDidPressPlayButton(viewController);
            } else
            {
                LoadModifiersPanel(errorList);
            }
        }
        public void HandleMissionResultsViewControllerRetryButtonPressed(MissionResultsViewController viewController)
        {
            Challenge challenge = ((CustomMissionDataSO)_missionLevelDetailViewController.missionNode.missionData).challenge;
            String failedMods = LoadExternalModifiers(challenge);
            if (failedMods.Length == 0)
            {
                Gamemode.NextLevelIsIsolated("Challenges");
                _campaignFlowCoordinator.HandleMissionResultsViewControllerRetryButtonPressed(_missionResultsViewController);
            }
        }
        public string LoadExternalModifiers(Challenge challenge)
        {
            bool failedToLoad = false;
            string modIssues = "";
            foreach (KeyValuePair<string, Func<string[], bool>> pair in ChallengeExternalModifiers.externalModifiers)
            {
                if (challenge.externalModifiers.ContainsKey(pair.Key))
                {
                    if (!pair.Value(challenge.externalModifiers[pair.Key]))
                    {
                        modIssues += pair.Key + " ";
                        failedToLoad = true;
                    }
                }
                else
                {
                    if (!pair.Value(new string[0]))
                    {
                        modIssues += pair.Key + " ";
                        failedToLoad = true;
                    }
                }
            }
            foreach (string modname in challenge.externalModifiers.Keys)
            {
                if (!ChallengeExternalModifiers.externalModifiers.ContainsKey(modname))
                {
                    modIssues += modname + " ";
                    failedToLoad = true;
                }
            }
            if (failedToLoad) ChallengeExternalModifiers.onChallengeFailedToLoad?.Invoke();
            return modIssues;
        }
        public IEnumerator DownloadSongCoroutine(string songid, string url, bool isBeatSaver)
        {
            _playButton.SetButtonText("DOWNLOADING...");
            _playButton.interactable = false;
            var www = UnityWebRequest.Get(url);

            var timeout = false;
            var time = 0f;

            var asyncRequest = www.SendWebRequest();

            while (!asyncRequest.isDone || asyncRequest.progress < 1f)
            {
                yield return null;

                time += Time.deltaTime;

                if (!(time >= 15f) || asyncRequest.progress != 0f) continue;
                www.Abort();
                timeout = true;
            }

            if (www.isNetworkError || www.isHttpError || timeout)
            {
                www.Abort();
                _playButton.interactable = true;
                _playButton.SetButtonText("DOWNLOAD");
                isDownloading = false;
            }
            else
            {
                string docPath = "";
                string customSongsPath = "";

                byte[] data = www.downloadHandler.data;


                string extra = "";
                if (isBeatSaver)
                {
                    var www2 = UnityWebRequest.Get("https://beatsaver.com/api/maps/detail/" + songid);

                    var timeout2 = false;
                    var time2 = 0f;

                    var asyncRequest2 = www2.SendWebRequest();

                    while (!asyncRequest2.isDone || asyncRequest2.progress < 1f)
                    {
                        yield return null;

                        time2 += Time.deltaTime;

                        if (!(time2 >= 15f) || asyncRequest2.progress != 0f) continue;
                        www2.Abort();
                        timeout2 = true;
                    }

                    if (www2.isNetworkError || www2.isHttpError || timeout2)
                    {
                        www2.Abort();
                    }
                    else
                    {
                        DetailResponse response = JsonConvert.DeserializeObject<DetailResponse>(www2.downloadHandler.text);
                        extra = " (" + response.metadata.songName + " - " + response.metadata.levelAuthorName + ")";
                    }
                }

                Stream zipStream = null;
                try
                {
                    docPath = Application.dataPath;
                    docPath = docPath.Substring(0, docPath.Length - 5);
                    docPath = docPath.Substring(0, docPath.LastIndexOf("/"));
                    customSongsPath = docPath + "/Beat Saber_Data/CustomLevels/" + songid + extra + "/";
                    if (!Directory.Exists(customSongsPath))
                    {
                        Directory.CreateDirectory(customSongsPath);
                    }
                    zipStream = new MemoryStream(data);
                }
                catch (Exception e)
                {
                    yield break;
                }
                Task extract = ExtractZipAsync(songid, zipStream, customSongsPath);
                yield return new WaitWhile(() => !extract.IsCompleted);
                Loader.SongsLoadedEvent += OnSongsLoaded;
                Loader.Instance.RefreshSongs();
            }
        }
        private async Task ExtractZipAsync(string songid, Stream zipStream, string customSongsPath)
        {
            try
            {
                ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
                await Task.Run(() => archive.ExtractToDirectory(customSongsPath)).ConfigureAwait(false);
                archive.Dispose();
                zipStream.Close();
            }
            catch (Exception e)
            {
                return;
            }
        }
        private void OnSongsLoaded(Loader songLoader, Dictionary<string, CustomPreviewBeatmapLevel> list)
        {
            isDownloading = false;
            _playButton.interactable = true;
            Loader.SongsLoadedEvent -= OnSongsLoaded;
            foreach (MissionNode node in curCampaignNodes)
            {
                node.SetPrivateField("_missionDataSO", ((CustomMissionDataSO)node.missionData).challenge.GetMissionData(((CustomMissionDataSO)node.missionData).campaign));
            }
            _missionNodeSelectionManager.GetPrivateField<Action<MissionNodeVisualController>>("didSelectMissionNodeEvent")(downloadingNode.missionNodeVisualController);
        }
        public void ResetProgressIds()
        {
            _campaignProgressModel.SetPrivateField("_missionIds", new HashSet<string>());
            _campaignProgressModel.SetPrivateField("_numberOfClearedMissionsDirty", true);
        }
        public void HandleMissionSelectionMapViewControllerDidSelectMissionLevel(MissionSelectionMapViewController viewController, MissionNode missionNode)
        {
            Challenge challenge = (missionNode.missionData as CustomMissionDataSO).challenge;
            _campaignChallengeLeaderbaordViewController.lastClicked = challenge;
            _campaignFlowCoordinator.InvokePrivateMethod("SetRightScreenViewController", new object[]{_campaignChallengeLeaderbaordViewController, false});
            _campaignChallengeLeaderbaordViewController.UpdateLeaderboards();
            _challengeName.text = challenge.name;
            _challengeName.alignment = TextAlignmentOptions.Bottom;
            List<GameplayModifierParamsSO> modParams = _gameplayModifiersModel.GetModifierParams(missionNode.missionData.gameplayModifiers);
            foreach(UnlockableItem item in challenge.unlockableItems)
            {
                modParams.Add(item.GetModifierParam());
            }
            if (challenge.unlockMap)
            {
                GameplayModifierParamsSO modifierParam = ScriptableObject.CreateInstance<GameplayModifierParamsSO>();
                modifierParam.SetPrivateField("_modifierName", "Unlockable Song");
                modifierParam.SetPrivateField("_hintText", "Unlock this song on completion");
                modifierParam.SetPrivateField("_icon", Assets.UnlockableSongIcon);
                modParams.Add(modifierParam);
            }
            LoadModifiersPanel(modParams);
        }
        public virtual void HandleMissionResultsViewControllerContinueButtonPressed(MissionResultsViewController viewController)
        {
            _campaignFlowCoordinator.InvokePrivateMethod("SetBottomScreenViewController", new object[] { null, false });
            LoadModifiersPanel(modifierParamsList);
        }
        public void LoadModifiersPanel(List<GameplayModifierParamsSO> modifierParamsList)
        {
            modifierParamsPageNumber = 0;
            _pageDownModifiersButton.gameObject.SetActive(modifierParamsList.Count > 0);
            _pageUpModifiersButton.gameObject.SetActive(modifierParamsList.Count > 0);
            this.modifierParamsList = modifierParamsList;
            _modifiersPanelGO.SetActive(modifierParamsList.Count > 0);
            UpdateModifiers();
        }
        public void ModifiersPageDown()
        {
            modifierParamsPageNumber = Math.Min(modifierParamsList.Count/2, modifierParamsPageNumber + 1);
            UpdateModifiers();
        }
        public void ModifiersPageUp()
        {
            modifierParamsPageNumber = Math.Max(0,modifierParamsPageNumber-1);
            UpdateModifiers();
        }
        public void UpdateModifiers()
        {
            _pageDownModifiersButton.interactable = modifierParamsPageNumber*2<modifierParamsList.Count-1;
            if (modifierParamsList.Count <= 2) _pageDownModifiersButton.interactable = false;
            _pageUpModifiersButton.interactable = modifierParamsPageNumber!=0;
            _gameplayModifierInfoListItemsList.SetData(modifierParamsPageNumber * 2 == modifierParamsList.Count - 1? 1 : Math.Min(2, modifierParamsList.Count), delegate (int idx, GameplayModifierInfoListItem gameplayModifierInfoListItem)
            {
                GameplayModifierParamsSO gameplayModifierParamsSO = modifierParamsList[modifierParamsPageNumber * 2 + idx];
                gameplayModifierInfoListItem.modifierIcon = gameplayModifierParamsSO.icon;
                if (gameplayModifierParamsSO.localizedModifierName == null)
                {
                    gameplayModifierInfoListItem.modifierName = gameplayModifierParamsSO.modifierName;
                    gameplayModifierInfoListItem.modifierDescription = gameplayModifierParamsSO.hintText;
                } else
                {
                    gameplayModifierInfoListItem.modifierName = gameplayModifierParamsSO.localizedModifierName;
                    gameplayModifierInfoListItem.modifierDescription = gameplayModifierParamsSO.localizedHintText;
                }
                gameplayModifierInfoListItem.showSeparator = idx==0&& modifierParamsPageNumber * 2 != modifierParamsList.Count - 1;//(idx != modifierParamsList.Count - 1);
            });
        }
    }
}
