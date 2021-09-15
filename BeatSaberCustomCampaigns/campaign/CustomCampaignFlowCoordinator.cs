using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BS_Utils.Gameplay;
using HMUI;
using SongCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using IPA.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static MenuLightsPresetSO;
using static SongCore.Data.ExtraSongData;
using Image = UnityEngine.UI.Image;

namespace BeatSaberCustomCampaigns.campaign
{
    public class CustomCampaignFlowCoordinator : FlowCoordinator
    {
        public const float EDITOR_TO_GAME_UNITS = 30f / 111;
        public const float HEIGHT_OFFSET = 20;

        public CampaignFlowCoordinator _campaignFlowCoordinator;

        public MissionMapAnimationController _missionMapAnimationController;
        public MissionNodesManager _missionNodesManager;
        public MissionStagesManager _missionStagesManager;
        public MissionConnectionsGenerator _missionConnectionsGenerator;
        public MissionSelectionMapViewController _missionSelectionMapViewController;
        public MissionNodeSelectionManager _missionNodeSelectionManager;
        public MissionSelectionNavigationController _missionSelectionNavigationController;
        public MissionLevelDetailViewController _missionLevelDetailViewController;
        public MissionResultsViewController _missionResultsViewController;
        public Button _playButton;
        public ScrollView _mapScrollView;
        public ScrollViewItemsVisibilityController _mapScrollViewItemsVisibilityController;
        public Image _backgroundImage;
        public GameplayModifierInfoListItemsList _gameplayModifierInfoListItemsList;
        public GameObject _modifiersPanelGO;
        public GameplayModifiersModelSO _gameplayModifiersModel;

        [UIComponent("challenge-name")]
        public TextMeshProUGUI _challengeName;

        public CampaignProgressModel _campaignProgressModel;

        protected NavigationController _campaignListNavigationController;
        protected CampaignListViewController _campaignListViewController;
        protected CampaignDetailViewController _campaignDetailViewController;
        protected CampaignTotalLeaderboardViewController _campaignTotalLeaderboardViewController;

        protected CampaignChallengeLeaderboardViewController _campaignChallengeLeaderbaordViewController;
        protected UnlockedItemsViewController _unlockedItemsViewController;

        MissionNode[] baseNodes;
        MissionNode baseRoot;
        MissionNode baseFinal;
        MissionStage[] baseMissionStages;
        Sprite baseBackground;
        float baseBackAlpha;
        float baseMapHeight;
        MenuLightsPresetSO baseDefaultLights;

        MissionNode[] curCampaignNodes;
        MissionStage[] curMissionStages;

        bool isDownloading = false;
        MissionNode downloadingNode;

        List<GameplayModifierParamsSO> modifierParamsList;

        public static bool unlockAllMissions = false;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                SetTitle("Custom Campaigns");
                showBackButton = true;

                _campaignFlowCoordinator = Resources.FindObjectsOfTypeAll<CampaignFlowCoordinator>().First();
                _missionMapAnimationController = Resources.FindObjectsOfTypeAll<MissionMapAnimationController>().First();
                _missionNodesManager = Resources.FindObjectsOfTypeAll<MissionNodesManager>().First();
                _missionStagesManager = Resources.FindObjectsOfTypeAll<MissionStagesManager>().First();
                _missionConnectionsGenerator = Resources.FindObjectsOfTypeAll<MissionConnectionsGenerator>().First();
                _missionSelectionMapViewController = Resources.FindObjectsOfTypeAll<MissionSelectionMapViewController>().First();
                _missionNodeSelectionManager = Resources.FindObjectsOfTypeAll<MissionNodeSelectionManager>().First();
                _missionLevelDetailViewController = Resources.FindObjectsOfTypeAll<MissionLevelDetailViewController>().First();
                _missionResultsViewController = Resources.FindObjectsOfTypeAll<MissionResultsViewController>().First();

                _playButton = _missionLevelDetailViewController.GetField<Button, MissionLevelDetailViewController>("_playButton");
                _mapScrollView = _missionSelectionMapViewController.GetField<ScrollView, MissionSelectionMapViewController>("_mapScrollView");
                _mapScrollViewItemsVisibilityController = _mapScrollView.GetComponent<ScrollViewItemsVisibilityController>();
                _backgroundImage = _mapScrollView.GetComponentsInChildren<Image>().First(x => x.name == "Map");

                _missionSelectionNavigationController = _campaignFlowCoordinator.GetField<MissionSelectionNavigationController, CampaignFlowCoordinator>("_missionSelectionNavigationController");
                _gameplayModifierInfoListItemsList = _missionLevelDetailViewController.GetField<GameplayModifierInfoListItemsList, MissionLevelDetailViewController>("_gameplayModifierInfoListItemsList");
                _modifiersPanelGO = _missionLevelDetailViewController.GetField<GameObject, MissionLevelDetailViewController>("_modifiersPanelGO");

                _gameplayModifiersModel = _missionLevelDetailViewController.GetField<GameplayModifiersModelSO, MissionLevelDetailViewController>("_gameplayModifiersModel");

                BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "BeatSaberCustomCampaigns.Views.challenge-detail.bsml"), _missionLevelDetailViewController.gameObject, this);

                _campaignProgressModel = _campaignFlowCoordinator.GetField<CampaignProgressModel, CampaignFlowCoordinator>("_campaignProgressModel");

                _campaignListViewController = BeatSaberUI.CreateViewController<CampaignListViewController>();
                _campaignDetailViewController = BeatSaberUI.CreateViewController<CampaignDetailViewController>();
                _campaignTotalLeaderboardViewController = BeatSaberUI.CreateViewController<CampaignTotalLeaderboardViewController>();
                _campaignListNavigationController = BeatSaberUI.CreateViewController<NavigationController>();
                _campaignListViewController.clickedCampaign += ShowDetails;
                _campaignDetailViewController.clickedPlay += OpenCampaign;

                _campaignChallengeLeaderbaordViewController = BeatSaberUI.CreateViewController<CampaignChallengeLeaderboardViewController>();
                _unlockedItemsViewController = BeatSaberUI.CreateViewController<UnlockedItemsViewController>();

            }
            if (addedToHierarchy)
            {
                SetBaseCampaignEnabled(false);

                SetViewControllerToNavigationController(_campaignListNavigationController, _campaignListViewController);
                ProvideInitialViewControllers(_campaignListNavigationController);
            }
        }

        public void ShowDetails(Campaign campaign)
        {
            _campaignDetailViewController.campaign = campaign;
            _campaignTotalLeaderboardViewController.lastClicked = campaign.leaderboardID;
            if (!_campaignDetailViewController.isInViewControllerHierarchy)
            {
                PushViewControllerToNavigationController(_campaignListNavigationController, _campaignDetailViewController);
                SetRightScreenViewController(_campaignTotalLeaderboardViewController, ViewController.AnimationType.None);
                (_campaignTotalLeaderboardViewController.table.transform.GetChild(1).GetChild(0).transform as RectTransform).localScale = new Vector3(1, 0.9f, 1);
            }
            _campaignTotalLeaderboardViewController.UpdateLeaderboards();
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            if (removedFromHierarchy)
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
                    baseNodes = _missionNodesManager.GetField<MissionNode[], MissionNodesManager>("_allMissionNodes");
                    baseRoot = _missionNodesManager.GetField<MissionNode, MissionNodesManager>("_rootMissionNode");
                    baseFinal = _missionNodesManager.GetField<MissionNode, MissionNodesManager>("_finalMissionNode");
                    baseMissionStages = _missionStagesManager.GetField<MissionStage[], MissionStagesManager>("_missionStages");
                    baseBackground = _backgroundImage.sprite;
                    baseBackAlpha = _backgroundImage.color.a;
                    baseMapHeight = _mapScrollView.GetField<RectTransform, ScrollView>("_contentRectTransform").sizeDelta.y;
                    baseDefaultLights = _campaignFlowCoordinator.GetField<MenuLightsPresetSO, CampaignFlowCoordinator>("_defaultLightsPreset");
                }
                foreach (MissionNode node in baseNodes)
                {
                    node.transform.localPosition += new Vector3(0, enabled ? 10000 : -10000, 0);
                    node.gameObject.SetActive(true);
                }
                if (enabled)
                {
                    _missionNodesManager.SetField("_rootMissionNode", baseRoot);
                    _missionNodesManager.SetField("_rootMissionNode", baseRoot);
                    _missionNodesManager.SetField("_finalMissionNode", baseFinal);
                    _missionStagesManager.SetField("_missionStages", baseMissionStages);
                     _backgroundImage.sprite = baseBackground;
                    _backgroundImage.color = new Color(1, 1, 1, baseBackAlpha);
                    _campaignFlowCoordinator.SetField("_defaultLightsPreset", baseDefaultLights);
                    _mapScrollView.GetField<RectTransform, ScrollView>("_contentRectTransform").SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, baseMapHeight);
                    CampaignInit();
                }
                _challengeName.gameObject.SetActive(!enabled);
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
        public void CloseCampaign(CampaignFlowCoordinator campaignFlowCoordinator)
        {
            _campaignFlowCoordinator.InvokeMethod<object, CampaignFlowCoordinator>("SetTitle", "Campaign", ViewController.AnimationType.In);
            _campaignFlowCoordinator.didFinishEvent += BeatSaberUI.MainFlowCoordinator.HandleCampaignFlowCoordinatorDidFinish;
            _campaignFlowCoordinator.didFinishEvent -= CloseCampaign;
            _missionNodeSelectionManager.didSelectMissionNodeEvent -= HandleMissionNodeSelectionManagerDidSelectMissionNode;
            _missionLevelDetailViewController.didPressPlayButtonEvent += _missionSelectionNavigationController.HandleMissionLevelDetailViewControllerDidPressPlayButton;
            _missionResultsViewController.retryButtonPressedEvent -= HandleMissionResultsViewControllerRetryButtonPressed;
            _missionSelectionMapViewController.didSelectMissionLevelEvent -= HandleMissionSelectionMapViewControllerDidSelectMissionLevel;
            _missionResultsViewController.continueButtonPressedEvent -= HandleMissionResultsViewControllerContinueButtonPressed;
            _missionLevelDetailViewController.didPressPlayButtonEvent -= HandleMissionLevelDetailViewControllerDidPressPlayButtonPlay;
            _missionLevelDetailViewController.didPressPlayButtonEvent -= HandleMissionLevelDetailViewControllerDidPressPlayButtonDownload;
            _mapScrollView.GetField<RectTransform, ScrollView>("_contentRectTransform").SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, baseMapHeight);
            unlockAllMissions = false;
            foreach (MissionNode node in curCampaignNodes)
            {
                Destroy(node.gameObject);
            }
            foreach (MissionStage stage in curMissionStages)
            {
                Destroy(stage.gameObject);
            }
            _campaignFlowCoordinator.GetField<MenuLightsManager, CampaignFlowCoordinator>("_menuLightsManager").SetColorPreset(baseDefaultLights, animated: true);
            DismissFlowCoordinator(_campaignFlowCoordinator);
        }
        public void CampaignInit()
        {
            if (_missionNodeSelectionManager.GetField<MissionNode[], MissionNodeSelectionManager>("_missionNodes") == null) _missionNodeSelectionManager.Start();
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
            unlockAllMissions = campaign.info.allUnlocked;
            if (campaign.background == null)
            {
                _backgroundImage.color = new Color(1, 1, 1, 0);
            }
            else
            {
                _backgroundImage.color = new Color(1, 1, 1, campaign.info.backgroundAlpha);
                _backgroundImage.sprite = campaign.background;
            }
            MenuLightsPresetSO customLights = Instantiate(baseDefaultLights);

            SimpleColorSO color = ScriptableObject.CreateInstance<SimpleColorSO>();
            color.SetColor(new Color(campaign.info.lightColor.r, campaign.info.lightColor.g, campaign.info.lightColor.b));
            foreach (LightIdColorPair pair in customLights.lightIdColorPairs)
            {
                pair.baseColor = color;
            }
            _campaignFlowCoordinator.SetField("_defaultLightsPreset", customLights);
            MissionNode[] missionNodes = new MissionNode[campaign.info.mapPositions.Count];
            curCampaignNodes = missionNodes;
            MissionStage[] missionStages;
            if (campaign.info.unlockGate.Count == 0 || unlockAllMissions)
            {
                //campaigns require an unlock gate so I make a fake one above the visible map area
                missionStages = new MissionStage[1];
                missionStages[0] = Instantiate(baseMissionStages[0], _missionNodesManager.GetField<GameObject, MissionNodesManager>("_missionNodesParentObject").transform);
                missionStages[0].SetField("_minimumMissionsToUnlock", 0);
                missionStages[0].GetField<RectTransform, MissionStage>("_rectTransform").localPosition = new Vector3(0, -baseMapHeight / 2 + campaign.info.mapHeight * EDITOR_TO_GAME_UNITS + HEIGHT_OFFSET / 2 + 1000, 0);
            }
            else
            {
                missionStages = new MissionStage[campaign.info.unlockGate.Count + 1];
                for (int i = 0; i < missionStages.Length - 1; i++)
                {
                    missionStages[i] = Instantiate(baseMissionStages[0], _missionNodesManager.GetField<GameObject, MissionNodesManager>("_missionNodesParentObject").transform);
                    missionStages[i].SetField("_minimumMissionsToUnlock", campaign.info.unlockGate[i].clearsToPass);
                    missionStages[i].GetField<RectTransform, MissionStage>("_rectTransform").localPosition = new Vector3(campaign.info.unlockGate[i].x * EDITOR_TO_GAME_UNITS, -baseMapHeight / 2 + campaign.info.mapHeight * EDITOR_TO_GAME_UNITS + HEIGHT_OFFSET / 2 - campaign.info.unlockGate[i].y * EDITOR_TO_GAME_UNITS, 0);
                }
                //ghost unlock gate required for some reason as last unlock gate never gets cleared
                missionStages[campaign.info.unlockGate.Count] = Instantiate(baseMissionStages[0], _missionNodesManager.GetField<GameObject, MissionNodesManager>("_missionNodesParentObject").transform);
                missionStages[campaign.info.unlockGate.Count].SetField("_minimumMissionsToUnlock", campaign.info.mapPositions.Count + 1);
                missionStages[campaign.info.unlockGate.Count].GetField<RectTransform, MissionStage>("_rectTransform").localPosition = new Vector3(0, -baseMapHeight / 2 + campaign.info.mapHeight * EDITOR_TO_GAME_UNITS + HEIGHT_OFFSET / 2 + 1000, 0);
            }
            curMissionStages = missionStages;
            _missionStagesManager.SetField("_missionStages", (from stage in missionStages orderby stage.minimumMissionsToUnlock select stage).ToArray());
            for (int i = 0; i < missionNodes.Length; i++)
            {
                CampainMapPosition mapPosition = campaign.info.mapPositions[i];
                missionNodes[i] = Instantiate(baseNodes[0], _missionNodesManager.GetField<GameObject, MissionNodesManager>("_missionNodesParentObject").transform);
                missionNodes[i].gameObject.SetActive(true);
                missionNodes[i].SetField("_missionDataSO", campaign.challenges[i].GetMissionData(campaign));
                var missionNodeTransform = missionNodes[i].GetField<RectTransform, MissionNode>("_rectTransform");
                missionNodeTransform.localPosition = new Vector3(mapPosition.x * EDITOR_TO_GAME_UNITS, -baseMapHeight / 2 + campaign.info.mapHeight * EDITOR_TO_GAME_UNITS + HEIGHT_OFFSET / 2 - mapPosition.y * EDITOR_TO_GAME_UNITS, 0);
                missionNodeTransform.sizeDelta = new Vector2(12 * mapPosition.scale, 12 * mapPosition.scale);
                missionNodes[i].SetField("_letterPartName", mapPosition.letterPortion);
                missionNodes[i].SetField("_numberPartName", mapPosition.numberPortion);

                string nodeDefaultColorText = mapPosition.nodeDefaultColor;
                string nodeHighlightColorText = mapPosition.nodeHighlightColor;

                Color nodeDefaultColor;
                Color nodeHighlightColor;

                var missionNodeVisualController = missionNodes[i].GetField<MissionNodeVisualController, MissionNode>("_missionNodeVisualController");
                var missionToggle = missionNodeVisualController.GetField<MissionToggle, MissionNodeVisualController>("_missionToggle");

                if (mapPosition.nodeOutline != null)
                {
                    var strokeImage = missionToggle.GetField<Image, MissionToggle>("_strokeImage");
                    strokeImage.sprite = mapPosition.nodeOutline;
                }

                if (mapPosition.nodeBackground != null)
                {
                    var backgroundImage = missionToggle.GetField<Image, MissionToggle>("_bgImage");
                    backgroundImage.sprite = mapPosition.nodeBackground;
                }

                if (ColorUtility.TryParseHtmlString(nodeDefaultColorText, out nodeDefaultColor))
                {
                    missionToggle.SetField("_disabledColor", nodeDefaultColor.ColorWithAlpha(0.1f));
                    missionToggle.SetField("_normalColor", nodeDefaultColor);
                }

                if (ColorUtility.TryParseHtmlString(nodeHighlightColorText, out nodeHighlightColor))
                {
                    missionToggle.SetField("_highlightColor", nodeHighlightColor);
                }

            }
            for (int i = 0; i < missionNodes.Length; i++)
            {
                MissionNode[] children = new MissionNode[campaign.info.mapPositions[i].childNodes.Length];
                for (int j = 0; j < children.Length; j++)
                {
                    children[j] = missionNodes[campaign.info.mapPositions[i].childNodes[j]];
                }
                missionNodes[i].SetField("_childNodes", children);
            }
            _missionNodesManager.SetField("_rootMissionNode", missionNodes[0]);
            _mapScrollView.GetField<RectTransform, ScrollView>("_contentRectTransform").SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, campaign.info.mapHeight * EDITOR_TO_GAME_UNITS + HEIGHT_OFFSET);
            CampaignInit();
            PresentFlowCoordinator(_campaignFlowCoordinator, delegate ()
            {
                _campaignFlowCoordinator.InvokeMethod<object, CampaignFlowCoordinator>("SetTitle", campaign.info.name, ViewController.AnimationType.In);
                _missionNodeSelectionManager.didSelectMissionNodeEvent -= _missionSelectionMapViewController.HandleMissionNodeSelectionManagerDidSelectMissionNode;
                _missionLevelDetailViewController.didPressPlayButtonEvent -= _missionSelectionNavigationController.HandleMissionLevelDetailViewControllerDidPressPlayButton;
                _missionResultsViewController.retryButtonPressedEvent += HandleMissionResultsViewControllerRetryButtonPressed;
                _missionResultsViewController.retryButtonPressedEvent -= _campaignFlowCoordinator.HandleMissionResultsViewControllerRetryButtonPressed;
                _campaignFlowCoordinator.didFinishEvent -= BeatSaberUI.MainFlowCoordinator.HandleCampaignFlowCoordinatorDidFinish;
                _campaignFlowCoordinator.didFinishEvent += CloseCampaign;
                _missionSelectionMapViewController.didSelectMissionLevelEvent += HandleMissionSelectionMapViewControllerDidSelectMissionLevel;
                _missionResultsViewController.continueButtonPressedEvent += HandleMissionResultsViewControllerContinueButtonPressed;
                _missionMapAnimationController.ScrollToTopMostNotClearedMission();
                _playButton.interactable = true;
            });
            _missionNodeSelectionManager.didSelectMissionNodeEvent += HandleMissionNodeSelectionManagerDidSelectMissionNode;

            GameplaySetupViewController gameplaySetupViewController = _campaignFlowCoordinator.GetField<GameplaySetupViewController, CampaignFlowCoordinator>("_gameplaySetupViewController");
            gameplaySetupViewController.SetField("_showEnvironmentOverrideSettings", true);
            gameplaySetupViewController.RefreshContent();
        }
        public void HandleMissionNodeSelectionManagerDidSelectMissionNode(MissionNodeVisualController missionNodeVisualController)
        {
            if (isDownloading) return;
            _missionLevelDetailViewController.didPressPlayButtonEvent -= _missionSelectionNavigationController.HandleMissionLevelDetailViewControllerDidPressPlayButton;
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

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this);
        }
        public void HandleMissionLevelDetailViewControllerDidPressPlayButtonDownload(MissionLevelDetailViewController viewController)
        {
            Challenge challenge = ((CustomMissionDataSO)viewController.missionNode.missionData).challenge;

            _playButton.SetButtonText("DOWNLOADING...");
            _playButton.interactable = false;
            SongDownloader.instance.DownloadSong(challenge.songid, challenge.hash, challenge.customDownloadURL, delegate {
                isDownloading = false;
                _playButton.interactable = true;
                foreach (MissionNode node in curCampaignNodes)
                {
                    node.SetField("_missionDataSO", ((CustomMissionDataSO)node.missionData).challenge.GetMissionData(((CustomMissionDataSO)node.missionData).campaign));
                }
                _campaignChallengeLeaderbaordViewController.UpdateAccuracy();
                _missionNodeSelectionManager.GetField<Action<MissionNodeVisualController>, MissionNodeSelectionManager>("didSelectMissionNodeEvent")(downloadingNode.missionNodeVisualController);
            }, delegate {
                _playButton.interactable = true;
                _playButton.SetButtonText("DOWNLOAD");
                isDownloading = false;
            });
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
                    errorList.Add(APITools.CreateModifierParam(Assets.ErrorIcon, "Error - External Mod", "Please install or update the following mod: " + s));
            }
            if (viewController.missionNode.missionData.beatmapCharacteristic.descriptionLocalizationKey == "ERROR NOT FOUND")
                errorList.Add(APITools.CreateModifierParam(Assets.ErrorIcon, "Error - Characteristic Not Found", "Could not find the characteristic \"" + challenge.characteristic + "\" for this map"));
            else if (BeatmapLevelDataExtensions.GetDifficultyBeatmap(Loader.BeatmapLevelsModelSO.GetBeatmapLevelIfLoaded((missionData as CustomMissionDataSO).customLevel.levelID).beatmapLevelData, missionData.beatmapCharacteristic, missionData.beatmapDifficulty) == null)
                errorList.Add(APITools.CreateModifierParam(Assets.ErrorIcon, "Error - Difficulty Not Found", "Could not find the difficulty \"" + challenge.difficulty + "\" for this map"));
            else
            {
                DifficultyData difficultyData = Collections.RetrieveDifficultyData(BeatmapLevelDataExtensions.GetDifficultyBeatmap(Loader.BeatmapLevelsModelSO.GetBeatmapLevelIfLoaded((missionData as CustomMissionDataSO).customLevel.levelID).beatmapLevelData, missionData.beatmapCharacteristic, missionData.beatmapDifficulty));
                foreach (string requirement in difficultyData.additionalDifficultyData._requirements)
                {
                    if (Collections.capabilities.Contains(requirement) || requirement.StartsWith("Complete Campaign Challenge - ")) continue;
                    errorList.Add(APITools.CreateModifierParam(Assets.ErrorIcon, "Error - Missing Level Requirement", "Could not find the capability to play levels with \"" + requirement + "\""));
                }
            }
            foreach(ChallengeRequirement requirement in challenge.requirements)
            {
                if (ChallengeRequirement.GetObjectiveName(requirement.type) == "ERROR")
                    errorList.Add(APITools.CreateModifierParam(Assets.ErrorIcon, "Error - Mission Objective Not Found", "You likely have a typo in the requirement name"));
            }
            if (errorList.Count==0)
            {
                Gamemode.NextLevelIsIsolated("Custom Campaigns");
                _missionSelectionNavigationController.HandleMissionLevelDetailViewControllerDidPressPlayButton(viewController);
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
                Gamemode.NextLevelIsIsolated("Custom Campaigns");
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
        public void ResetProgressIds()
        {
            _campaignProgressModel.SetField("_missionIds", new HashSet<string>());
            _campaignProgressModel.SetField("_numberOfClearedMissionsDirty", true);
        }
        public void HandleMissionSelectionMapViewControllerDidSelectMissionLevel(MissionSelectionMapViewController viewController, MissionNode missionNode)
        {
            Challenge challenge = (missionNode.missionData as CustomMissionDataSO).challenge;
            _campaignChallengeLeaderbaordViewController.lastClicked = challenge;
            _campaignFlowCoordinator.InvokeMethod<object, CampaignFlowCoordinator>("SetRightScreenViewController", _campaignChallengeLeaderbaordViewController, ViewController.AnimationType.In);
            StartCoroutine(_campaignChallengeLeaderbaordViewController.UpdateLeaderboardsCoroutine());
            _challengeName.text = challenge.name;
            _challengeName.alignment = TextAlignmentOptions.Bottom;
            List<GameplayModifierParamsSO> modParams = _gameplayModifiersModel.CreateModifierParamsList(missionNode.missionData.gameplayModifiers);
            foreach (string modName in challenge.externalModifiers.Keys)
            {
                if (!ChallengeExternalModifiers.getInfo.ContainsKey(modName)) continue;
                foreach(ExternalModifierInfo modInfo in ChallengeExternalModifiers.getInfo[modName](challenge.externalModifiers[modName]))
                    modParams.Add(APITools.CreateModifierParam(modInfo.icon, modInfo.name, modInfo.desc));
            }
            foreach (UnlockableItem item in challenge.unlockableItems)
                modParams.Add(item.GetModifierParam());
            if (challenge.unlockMap)
                modParams.Add(APITools.CreateModifierParam(Assets.UnlockableSongIcon, "Unlockable Song", "Unlock this song on completion"));
            LoadModifiersPanel(modParams);
        }
        public virtual void HandleMissionResultsViewControllerContinueButtonPressed(MissionResultsViewController viewController)
        {
            _campaignFlowCoordinator.InvokeMethod<object, CampaignFlowCoordinator>("SetBottomScreenViewController", null, ViewController.AnimationType.In);
            LoadModifiersPanel(modifierParamsList);
        }
        public void LoadModifiersPanel(List<GameplayModifierParamsSO> modifierParamsList)
        {
            this.modifierParamsList = modifierParamsList;
            _modifiersPanelGO.SetActive(modifierParamsList.Count > 0);
            UpdateModifiers();
        }

        public void UpdateModifiers()
        {
            _gameplayModifierInfoListItemsList.SetData(modifierParamsList.Count, delegate (int idx, GameplayModifierInfoListItem gameplayModifierInfoListItem)
            {
                GameplayModifierParamsSO gameplayModifierParamsSO = modifierParamsList[idx];
                gameplayModifierInfoListItem.SetModifier(gameplayModifierParamsSO, true);
            });
        }
    }
}
