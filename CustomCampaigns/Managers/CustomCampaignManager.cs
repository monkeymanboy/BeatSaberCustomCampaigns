using BeatSaberMarkupLanguage;
using CustomCampaigns.Campaign.Missions;
using CustomCampaigns.HarmonyPatches;
using CustomCampaigns.UI.MissionObjectiveGameUI;
using CustomCampaigns.UI.ViewControllers;
using CustomCampaigns.Utils;
using HarmonyLib;
using HMUI;
using IPA.Utilities;
using SiraUtil.Affinity;
using SongCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static SongCore.Data.ExtraSongData;

namespace CustomCampaigns.Managers
{
    public class CustomCampaignManager : IAffinity
    {
        public static bool unlockAllMissions = false;

        public Action<CampaignFlowCoordinator> CampaignClosed;

        internal static bool isCampaignLevel = false;
        internal static MissionDataSO currentMissionData;
        private MissionNode _currentNode;
        private bool _currentMissionCleared = false;

        internal static MissionObjectiveResult[] missionObjectiveResults;
        internal static MissionObjectiveGameUIView missionObjectiveGameUIViewPrefab = null;
        private MissionNode _downloadingNode;

        public Campaign.Campaign Campaign { get => _currentCampaign; }

        private Campaign.Campaign _currentCampaign;

        private const string EXTERNAL_MOD_ERROR_TITLE = "Error - External Mod";
        private const string EXTERNAL_MOD_ERROR_DESCRIPTION = "Please install or update the following mod: ";
        private const string CHARACTERISTIC_NOT_FOUND_ERROR_TITLE = "Error - Characteristic Not Found";
        private const string CHARACTERISTIC_NOT_FOUND_ERROR_DESCRIPTION = "Could not find the characteristic ";
        private const string DIFFICULTY_NOT_FOUND_ERROR_TITLE = "Error - Difficulty Not Found";
        private const string DIFFICULTY_NOT_FOUND_ERROR_DESCRIPTION = "Could not find the difficulty ";
        private const string NOT_FOUND_ERROR_SUFFIX = "for this map";
        private const string MISSING_CAPABILITY_ERROR_TITLE = "Error - Missing Level Requirement";
        private const string MISSING_CAPABILITY_ERROR_DESCRIPTION = "Could not find the capability to play levels with ";
        private const string OBJECTIVE_NOT_FOUND_ERROR_TITLE = "Error - Mission Objective Not Found";
        private const string OBJECTIVE_NOT_FOUND_ERROR_DESCRIPTION = "You likely have a typo in the requirement name";
        private const string MISSING_OPTIONAL_TITLE = "Error - Optional Mod";
        private const string MISSING_OPTIONAL_DESCRIPTION = "Missing optional mod ";
        private const string OPTIONAL_FAILURE_TITLE = "Error - Optional Mod";
        private const string OPTIONAL_FAILURE_DESCRIPTION = "Failure from optional mod ";

        private CustomCampaignUIManager _customCampaignUIManager;
        private DownloadManager _downloadManager;

        private bool forceClose = false;

        private CancellationTokenSource _cancellationTokenSource;

        private CampaignProgressModel _campaignProgressModel;

        private CampaignFlowCoordinator _campaignFlowCoordinator;

        private MenuTransitionsHelper _menuTransitionsHelper;

        private MissionSelectionMapViewController _missionSelectionMapViewController;
        private MissionNodeSelectionManager _missionNodeSelectionManager;
        private MissionSelectionNavigationController _missionSelectionNavigationController;
        private MissionLevelDetailViewController _missionLevelDetailViewController;
        private MissionResultsViewController _missionResultsViewController;

        private ModalController _modalController;

        private CustomMissionDataSO _currentMissionDataSO;
        private MissionHelpViewController _missionHelpViewController;

        private UnlockableSongsManager _unlockableSongsManager;

        private PlayerDataModel _playerDataModel;

        private ExternalModifierManager _externalModifierManager;

        private Config _config;

        private bool _waitingForWarningInteraction = false;
        private bool _backingOutMissingOptional = false;
        private bool _backingOutOptionalFailure = false;

        public CustomCampaignManager(CustomCampaignUIManager customCampaignUIManager, DownloadManager downloadManager, CampaignFlowCoordinator campaignFlowCoordinator,
                                     MenuTransitionsHelper menuTransitionsHelper, MissionSelectionMapViewController missionSelectionMapViewController,
                                     MissionSelectionNavigationController missionSelectionNavigationController, MissionLevelDetailViewController missionLevelDetailViewController,
                                     MissionResultsViewController missionResultsViewController, MissionHelpViewController missionHelpViewController, ModalController modalController,
                                     UnlockableSongsManager unlockableSongsManager, PlayerDataModel playerDataModel, ExternalModifierManager externalModifierManager, Config config)
        {
            _customCampaignUIManager = customCampaignUIManager;
            _downloadManager = downloadManager;

            _campaignFlowCoordinator = campaignFlowCoordinator;
            _campaignProgressModel = _customCampaignUIManager.CampaignFlowCoordinator.GetField<CampaignProgressModel, CampaignFlowCoordinator>("_campaignProgressModel");

            _menuTransitionsHelper = menuTransitionsHelper;
            _missionSelectionMapViewController = missionSelectionMapViewController;
            _missionNodeSelectionManager = missionSelectionMapViewController.GetField<MissionNodeSelectionManager, MissionSelectionMapViewController>("_missionNodeSelectionManager");

            _missionSelectionNavigationController = missionSelectionNavigationController;
            _missionLevelDetailViewController = missionLevelDetailViewController;
            _missionResultsViewController = missionResultsViewController;
            _missionHelpViewController = missionHelpViewController;

            _modalController = modalController;

            _unlockableSongsManager = unlockableSongsManager;
            _playerDataModel = playerDataModel;

            _externalModifierManager = externalModifierManager;

            _config = config;
        }

        #region CampaignInit
        public void FirstActivation()
        {
            _customCampaignUIManager.FirstActivation();

            _modalController.didForceCancelDownloads -= OnForcedDownloadsCancel;
            _modalController.didForceCancelDownloads += OnForcedDownloadsCancel;
        }

        public void FlowCoordinatorPresented()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            _missionNodeSelectionManager.didSelectMissionNodeEvent -=
                (Action<MissionNodeVisualController>)_missionSelectionMapViewController.GetType().GetMethod("HandleMissionNodeSelectionManagerDidSelectMissionNode", AccessTools.all)
                    ?.CreateDelegate(typeof(Action<MissionNodeVisualController>), _missionSelectionMapViewController);
            _missionNodeSelectionManager.didSelectMissionNodeEvent -= OnDidSelectMissionNode;
            _missionNodeSelectionManager.didSelectMissionNodeEvent += OnDidSelectMissionNode;

            _missionLevelDetailViewController.didPressPlayButtonEvent -=
                (Action <MissionLevelDetailViewController>) _missionSelectionNavigationController.GetType().GetMethod("HandleMissionLevelDetailViewControllerDidPressPlayButton", AccessTools.all)
                    ?.CreateDelegate(typeof(Action<MissionLevelDetailViewController>), _missionSelectionNavigationController);
            _missionLevelDetailViewController.didPressPlayButtonEvent -= OnDidPressPlayButton;
            _missionLevelDetailViewController.didPressPlayButtonEvent += OnDidPressPlayButton;

            _missionResultsViewController.retryButtonPressedEvent -=
                (Action <MissionResultsViewController>) _campaignFlowCoordinator.GetType().GetMethod("HandleMissionResultsViewControllerRetryButtonPressed", AccessTools.all)
                    ?.CreateDelegate(typeof(Action<MissionResultsViewController>), _campaignFlowCoordinator);
            _missionResultsViewController.retryButtonPressedEvent -= OnRetryButtonPressed;
            _missionResultsViewController.retryButtonPressedEvent += OnRetryButtonPressed;

            _campaignFlowCoordinator.didFinishEvent -=
                (Action<CampaignFlowCoordinator>) BeatSaberUI.MainFlowCoordinator.GetType().GetMethod("HandleCampaignFlowCoordinatorDidFinish", AccessTools.all)
                    ?.CreateDelegate(typeof(Action<CampaignFlowCoordinator>), BeatSaberUI.MainFlowCoordinator);
            _campaignFlowCoordinator.didFinishEvent -= OnDidCloseCampaign;
            _campaignFlowCoordinator.didFinishEvent += OnDidCloseCampaign;

            _missionSelectionMapViewController.didSelectMissionLevelEvent -= OnDidSelectMissionLevel;
            _missionSelectionMapViewController.didSelectMissionLevelEvent += OnDidSelectMissionLevel;

            _missionResultsViewController.continueButtonPressedEvent -= OnContinueButtonPressed;
            _missionResultsViewController.continueButtonPressedEvent += OnContinueButtonPressed;

            SongCore.Loader.SongsLoadedEvent -= OnSongsLoaded;
            SongCore.Loader.SongsLoadedEvent += OnSongsLoaded;

            _customCampaignUIManager.FlowCoordinatorPresented(_currentCampaign);
        }

        public void SetupCampaign(Campaign.Campaign campaign, Action onFinishSetup)
        {
            _currentCampaign = campaign;
            unlockAllMissions = campaign.info.allUnlocked;
            _customCampaignUIManager.SetupCampaignUI(campaign);
            onFinishSetup?.Invoke();
        }
        #endregion

        public void CustomCampaignEnabled()
        {
            CampaignFlowCoordinatorHandleMissionLevelSceneDidFinishPatch.onMissionSceneFinish -= OnMissionLevelSceneDidFinish;
            CampaignFlowCoordinatorHandleMissionLevelSceneDidFinishPatch.onMissionSceneFinish += OnMissionLevelSceneDidFinish;

            _customCampaignUIManager.CustomCampaignEnabled();
        }

        #region Base Campaign
        public void BaseCampaignEnabled()
        {
            _campaignFlowCoordinator.InvokeMethod<object, CampaignFlowCoordinator>("SetTitle", "Campaign", ViewController.AnimationType.In);
            _campaignFlowCoordinator.didFinishEvent -= OnDidCloseCampaign;
            _campaignFlowCoordinator.didFinishEvent -=
                (Action<CampaignFlowCoordinator>) BeatSaberUI.MainFlowCoordinator.GetType().GetMethod("HandleCampaignFlowCoordinatorDidFinish", AccessTools.all)
                    ?.CreateDelegate(typeof(Action<CampaignFlowCoordinator>), BeatSaberUI.MainFlowCoordinator);
            _campaignFlowCoordinator.didFinishEvent +=
                (Action<CampaignFlowCoordinator>) BeatSaberUI.MainFlowCoordinator.GetType().GetMethod("HandleCampaignFlowCoordinatorDidFinish", AccessTools.all)
                    ?.CreateDelegate(typeof(Action<CampaignFlowCoordinator>), BeatSaberUI.MainFlowCoordinator);

            _missionNodeSelectionManager.didSelectMissionNodeEvent -= OnDidSelectMissionNode;
            _missionNodeSelectionManager.didSelectMissionNodeEvent -=
                (Action<MissionNodeVisualController>) _missionSelectionMapViewController.GetType().GetMethod("HandleMissionNodeSelectionManagerDidSelectMissionNode", AccessTools.all)
                    ?.CreateDelegate(typeof(Action<MissionNodeVisualController>), _missionSelectionMapViewController);
            _missionNodeSelectionManager.didSelectMissionNodeEvent +=
                (Action<MissionNodeVisualController>) _missionSelectionMapViewController.GetType().GetMethod("HandleMissionNodeSelectionManagerDidSelectMissionNode", AccessTools.all)
                    ?.CreateDelegate(typeof(Action<MissionNodeVisualController>), _missionSelectionMapViewController);

            _missionLevelDetailViewController.didPressPlayButtonEvent -= OnDidPressPlayButton;
            _missionLevelDetailViewController.didPressPlayButtonEvent -=
                (Action<MissionLevelDetailViewController>) _missionSelectionNavigationController.GetType().GetMethod("HandleMissionLevelDetailViewControllerDidPressPlayButton", AccessTools.all)
                    ?.CreateDelegate(typeof(Action<MissionLevelDetailViewController>), _missionSelectionNavigationController);
            _missionLevelDetailViewController.didPressPlayButtonEvent +=
                (Action<MissionLevelDetailViewController>) _missionSelectionNavigationController.GetType().GetMethod("HandleMissionLevelDetailViewControllerDidPressPlayButton", AccessTools.all)
                    ?.CreateDelegate(typeof(Action<MissionLevelDetailViewController>), _missionSelectionNavigationController);

            _missionResultsViewController.retryButtonPressedEvent -= OnRetryButtonPressed;
            _missionResultsViewController.retryButtonPressedEvent -=
                (Action<MissionResultsViewController>) _campaignFlowCoordinator.GetType().GetMethod("HandleMissionResultsViewControllerRetryButtonPressed", AccessTools.all)
                    ?.CreateDelegate(typeof(Action<MissionResultsViewController>), _campaignFlowCoordinator);
            _missionResultsViewController.retryButtonPressedEvent +=
                (Action<MissionResultsViewController>) _campaignFlowCoordinator.GetType().GetMethod("HandleMissionResultsViewControllerRetryButtonPressed", AccessTools.all)
                    ?.CreateDelegate(typeof(Action<MissionResultsViewController>), _campaignFlowCoordinator);

            _campaignFlowCoordinator.didFinishEvent -= OnDidCloseCampaign;
            _campaignFlowCoordinator.didFinishEvent -=
                (Action<CampaignFlowCoordinator>) BeatSaberUI.MainFlowCoordinator.GetType().GetMethod("HandleCampaignFlowCoordinatorDidFinish", AccessTools.all)
                    ?.CreateDelegate(typeof(Action<CampaignFlowCoordinator>), BeatSaberUI.MainFlowCoordinator);
            _campaignFlowCoordinator.didFinishEvent +=
                (Action<CampaignFlowCoordinator>) BeatSaberUI.MainFlowCoordinator.GetType().GetMethod("HandleCampaignFlowCoordinatorDidFinish", AccessTools.all)
                    ?.CreateDelegate(typeof(Action<CampaignFlowCoordinator>), BeatSaberUI.MainFlowCoordinator);

            _missionSelectionMapViewController.didSelectMissionLevelEvent -= OnDidSelectMissionLevel;

            _missionResultsViewController.continueButtonPressedEvent -= OnContinueButtonPressed;

            CampaignFlowCoordinatorHandleMissionLevelSceneDidFinishPatch.onMissionSceneFinish -= OnMissionLevelSceneDidFinish;

            _customCampaignUIManager.BaseCampaignEnabled();
        }
        #endregion

        #region Events
        private void OnDidSelectMissionNode(MissionNodeVisualController missionNodeVisualController)
        {
            if (_downloadManager.IsDownloading)
            {
                Plugin.logger.Error("Should never reach here - was downloading");
                return;
            }

            _missionLevelDetailViewController.didPressPlayButtonEvent -=
                (Action<MissionLevelDetailViewController>) _missionSelectionNavigationController.GetType().GetMethod("HandleMissionLevelDetailViewControllerDidPressPlayButton", AccessTools.all)
                    ?.CreateDelegate(typeof(Action<MissionLevelDetailViewController>), _missionSelectionNavigationController);
            CustomPreviewBeatmapLevel level = (missionNodeVisualController.missionNode.missionData as CustomMissionDataSO)?.mission.FindSong();
            if (level == null)
            {
                _missionSelectionMapViewController.GetType().GetMethod("HandleMissionNodeSelectionManagerDidSelectMissionNode", AccessTools.all)
                    ?.Invoke(_missionSelectionMapViewController, new object[] { missionNodeVisualController });
                _customCampaignUIManager.SetPlayButtonText("DOWNLOAD");
            }
            else
            {
                Plugin.logger.Debug("found level");
                _customCampaignUIManager.SetPlayButtonText("PLAY");

                LoadBeatmap(missionNodeVisualController, (missionNodeVisualController.missionNode.missionData as CustomMissionDataSO)?.customLevel.levelID);
            }
            _customCampaignUIManager.SetPlayButtonInteractable(true);
        }

        private void OnDidSelectMissionLevel(MissionSelectionMapViewController missionSelectionMapViewController, MissionNode missionNode)
        {
            Mission mission = (missionNode.missionData as CustomMissionDataSO)?.mission;
            _customCampaignUIManager.SetMissionName(mission?.name);
            _customCampaignUIManager.MissionLevelSelected(mission);

            _currentNode = missionNode;
        }

        private void OnDidPressPlayButton(MissionLevelDetailViewController missionLevelDetailViewController)
        {
            CustomMissionDataSO customMissionData = missionLevelDetailViewController.missionNode.missionData as CustomMissionDataSO;
            var customLevel = customMissionData.mission.FindSong();

            if (customLevel == null)
            {
                DownloadMap(missionLevelDetailViewController.missionNode);
            }
            else
            {
                PlayMap(missionLevelDetailViewController);
            }
        }

        private void DownloadMap(MissionNode missionNode)
        {
            _downloadingNode = missionNode;
            CustomMissionDataSO customMissionData = missionNode.missionData as CustomMissionDataSO;
            Mission mission = customMissionData.mission;
            _customCampaignUIManager.SetPlayButtonText("DOWNLOADING...");
            _customCampaignUIManager.SetPlayButtonInteractable(false);

            DownloadManager.DownloadEntry downloadEntry = new DownloadManager.DownloadEntry(mission.songid, mission.hash, mission.customDownloadURL);
            _downloadManager.AddSongToQueue(downloadEntry);

            _downloadManager.OnDownloadSuccess -= OnDownloadSucceeded;
            _downloadManager.OnDownloadSuccess += OnDownloadSucceeded;

            _downloadManager.OnDownloadFail -= OnDownloadFailed;
            _downloadManager.OnDownloadFail += OnDownloadFailed;

            _downloadManager.DownloadProgress -= OnDownloadProgressUpdated;
            _downloadManager.DownloadProgress += OnDownloadProgressUpdated;

            _downloadManager.DownloadStatus -= OnDownloadStatusUpdate;
            _downloadManager.DownloadStatus += OnDownloadStatusUpdate;

            _downloadManager.InitiateDownloads();
        }

        private void OnDownloadProgressUpdated(float progress)
        {
            _customCampaignUIManager.UpdateProgress(progress);
        }

        private void OnDownloadStatusUpdate(string status)
        {
            Plugin.logger.Debug($"Download status update: {status}");
            if (_missionLevelDetailViewController.missionNode == _downloadingNode)
            {
                _customCampaignUIManager.SetPlayButtonText(status);
            }
        }

        private void OnDownloadFailed()
        {
            Plugin.logger.Debug("Download for map failed :(");
            _customCampaignUIManager.ClearProgressBar();
            if (_missionLevelDetailViewController.missionNode == _downloadingNode)
            {
                _downloadingNode = null;
                _customCampaignUIManager.SetPlayButtonText("DOWNLOAD FAILED");
                _customCampaignUIManager.SetPlayButtonInteractable(true);
            }
            else
            {
                Plugin.logger.Error("Currently selected node is not the downloading one");
            }

            _downloadManager.OnDownloadSuccess -= OnDownloadSucceeded;
            _downloadManager.OnDownloadFail -= OnDownloadFailed;
            _downloadManager.DownloadProgress -= OnDownloadProgressUpdated;
            _downloadManager.DownloadStatus -= OnDownloadStatusUpdate;
        }

        private void OnDownloadSucceeded()
        {
            SongCore.Loader.Instance.RefreshSongs();

            _downloadManager.OnDownloadSuccess -= OnDownloadSucceeded;
            _downloadManager.OnDownloadFail -= OnDownloadFailed;
            _downloadManager.DownloadProgress -= OnDownloadProgressUpdated;
            _downloadManager.DownloadStatus -= OnDownloadStatusUpdate;
        }

        private void OnSongsLoaded(Loader loader, ConcurrentDictionary<string, CustomPreviewBeatmapLevel> levels)
        {
            Plugin.logger.Debug("songs loaded");
            (_missionLevelDetailViewController.missionNode.missionData as CustomMissionDataSO).mission.SetCustomLevel();
            _customCampaignUIManager.RefreshMissionNodeData();
            _customCampaignUIManager.ClearProgressBar();

            _downloadingNode = null;
            _customCampaignUIManager.SetPlayButtonText("Play");
            _customCampaignUIManager.SetPlayButtonInteractable(true);
            _missionNodeSelectionManager.GetField<Action<MissionNodeVisualController>, MissionNodeSelectionManager>("didSelectMissionNodeEvent")(_missionLevelDetailViewController.missionNode.missionNodeVisualController);
        }

        private async void PlayMap(MissionLevelDetailViewController missionLevelDetailViewController)
        {
            Plugin.logger.Debug("play");
            _currentNode = missionLevelDetailViewController.missionNode;
            MissionDataSO missionDataSO = _currentNode.missionData;
            _currentMissionDataSO = missionDataSO as CustomMissionDataSO;
            currentMissionData = _currentMissionDataSO;

            _currentMissionCleared = _playerDataModel.playerData.GetPlayerMissionStatsData(_currentNode.missionId).cleared;

            Mission mission = _currentMissionDataSO.mission;

            _customCampaignUIManager.SetPlayButtonText("Waiting for Modifiers...");
            _customCampaignUIManager.SetPlayButtonInteractable(false);

            var errors = await CheckForErrors(mission);

            _customCampaignUIManager.SetPlayButtonText("PLAY");
            _customCampaignUIManager.SetPlayButtonInteractable(true);
            if (errors.Count > 0)
            {
                Plugin.logger.Debug("Had errors, not starting mission");
                _customCampaignUIManager.LoadErrors(errors);
                return;
            }

            if (mission.allowStandardLevel)
            {
                isCampaignLevel = true;
                Plugin.logger.Debug("allow stadard level");
                if (missionObjectiveGameUIViewPrefab == null)
                {
                    Plugin.logger.Debug("null prefab");
                    InitializeMissionObjectiveGameUIViewPrefab();
                }
                else
                {
                    CustomMissionHelpSO missionHelp = _currentMissionDataSO.missionHelp as CustomMissionHelpSO;
                    if (missionHelp != null && !_playerDataModel.playerData.WasMissionHelpShowed(missionHelp))
                    {
                        _missionHelpViewController.didFinishEvent += HelpControllerDismissed;
                        _missionSelectionNavigationController.GetType().GetMethod("HandleMissionLevelDetailViewControllerDidPressPlayButton", AccessTools.all)
                            ?.Invoke(_missionSelectionNavigationController, new object[] { missionLevelDetailViewController });
                    }
                    else
                    {
                        StartCampaignLevel(null);
                    }
                }
            }
            else
            {
                _missionSelectionNavigationController.GetType().GetMethod("HandleMissionLevelDetailViewControllerDidPressPlayButton", AccessTools.all)
                    ?.Invoke(_missionSelectionNavigationController, new object[] { missionLevelDetailViewController });
            }
        }

        private void HelpControllerDismissed(MissionHelpViewController missionHelpViewController)
        {
            missionHelpViewController.didFinishEvent -= HelpControllerDismissed;
            StartCampaignLevel(HideMissionHelp);
        }

        private void InitializeMissionObjectiveGameUIViewPrefab()
        {
            var missionObjectiveGameUIViewPrefabFetcher = new GameObject().AddComponent<MissionObjectiveGameUIViewPrefabFetcher>();
            missionObjectiveGameUIViewPrefabFetcher.OnPrefabFetched += OnPrefabFetched;
            missionObjectiveGameUIViewPrefabFetcher.FetchPrefab();
        }

        private void OnPrefabFetched()
        {
            CustomMissionHelpSO missionHelp = _currentMissionDataSO.missionHelp as CustomMissionHelpSO;
            if (missionHelp != null && !_playerDataModel.playerData.WasMissionHelpShowed(missionHelp))
            {
                _missionHelpViewController.didFinishEvent += HelpControllerDismissed;
                _missionSelectionNavigationController.GetType().GetMethod("HandleMissionLevelDetailViewControllerDidPressPlayButton", AccessTools.all)
                    ?.Invoke(_missionSelectionNavigationController, new object[] { _missionLevelDetailViewController });
            }
            else
            {
                StartCampaignLevel(null);
            }
        }

        private void StartCampaignLevel(Action beforeSceneSwitchCallback)
        {
            isCampaignLevel = true;

            var level = (_missionLevelDetailViewController.missionNode.missionData as CustomMissionDataSO)?.customLevel.levelID;
            currentMissionData = _currentMissionDataSO;
            var beatmapLevel = Loader.BeatmapLevelsModelSO.GetBeatmapLevelIfLoaded(level);
            IDifficultyBeatmap difficultyBeatmap = BeatmapLevelDataExtensions.GetDifficultyBeatmap(beatmapLevel.beatmapLevelData, _currentMissionDataSO.beatmapCharacteristic, _currentMissionDataSO.beatmapDifficulty);
            GameplayModifiers gameplayModifiers = _currentMissionDataSO.gameplayModifiers;
            MissionObjective[] missionObjectives = _currentMissionDataSO.missionObjectives;
            GameplaySetupViewController gameplaySetupViewController = _campaignFlowCoordinator.GetField<GameplaySetupViewController, CampaignFlowCoordinator>("_gameplaySetupViewController");
            PlayerSpecificSettings playerSettings = gameplaySetupViewController.playerSettings;
            ColorScheme overrideColorScheme = gameplaySetupViewController.colorSchemesSettings.GetOverrideColorScheme();

            _menuTransitionsHelper.StartStandardLevel("Solo",
                                                        difficultyBeatmap,
                                                        beatmapLevel,
                                                        gameplaySetupViewController.environmentOverrideSettings,
                                                        overrideColorScheme,
                                                        //TODO: Maybe add the real beatmapOverrideColorScheme instead of passing the playerOverrideColorScheme
                                                        overrideColorScheme,
                                                        gameplayModifiers,
                                                        playerSettings,
                                                        null,
                                                        "Menu",
                                                        false,
                                                        false,
                                                        beforeSceneSwitchCallback,
                                                        OnFinishedStandardLevel,
                                                        OnRestartedStandardLevel);
        }

        private void OnFinishedStandardLevel(StandardLevelScenesTransitionSetupDataSO standardLevelScenesTransitionSetupDataSO, LevelCompletionResults levelCompletionResults)
        {
            Plugin.logger.Debug("on finish standard level");
            if (levelCompletionResults.levelEndAction == LevelCompletionResults.LevelEndAction.Restart)
            {
                StartCampaignLevel(null);
                return;
            }
            standardLevelScenesTransitionSetupDataSO.didFinishEvent -= OnFinishedStandardLevel;

            if (BSUtilsUtils.WasSubmissionDisabled())
            {
                levelCompletionResults.SetField("levelEndStateType", LevelCompletionResults.LevelEndStateType.Failed);
            }

            MissionCompletionResults missionCompletionResults = new MissionCompletionResults(levelCompletionResults, missionObjectiveResults);
            _campaignFlowCoordinator.GetType().GetMethod("HandleMissionLevelSceneDidFinish", AccessTools.all)
                ?.Invoke(_campaignFlowCoordinator, new object[] { null, missionCompletionResults });
            isCampaignLevel = false;
        }

        private void OnRestartedStandardLevel(LevelScenesTransitionSetupDataSO levelScenesTransitionSetupDataSO, LevelCompletionResults levelCompletionResults)
        {

        }

        private async void OnRetryButtonPressed(MissionResultsViewController missionResultsViewController)
        {
            Plugin.logger.Debug("retry button pressed");

            Mission mission = _currentMissionDataSO.mission;
            (Dictionary<string, string>, HashSet<string>, Dictionary<string, string>) failedMods = await LoadExternalModifiers(mission);

            if (failedMods.Item1.Count > 0)
            {
                Plugin.logger.Error("Error loading external modifiers");
                return;
            }

            // TODO: optional failures

            if (mission.allowStandardLevel)
            {
                StartCampaignLevel(HideMissionResults);
            }
            else
            {
                _campaignFlowCoordinator.GetType().GetMethod("HandleMissionResultsViewControllerRetryButtonPressed", AccessTools.all)
                    ?.Invoke(_campaignFlowCoordinator, new object[] { missionResultsViewController });
            }

        }

        private void HideMissionHelp()
        {
            _customCampaignUIManager.SetDefaultLights();
            _campaignFlowCoordinator.InvokeMethod<object, CampaignFlowCoordinator>("DismissViewController", _missionHelpViewController, ViewController.AnimationDirection.Horizontal, null, true);
        }

        private void HideMissionResults()
        {
            _customCampaignUIManager.SetDefaultLights();
            _campaignFlowCoordinator.InvokeMethod<object, CampaignFlowCoordinator>("DismissViewController", _missionResultsViewController, ViewController.AnimationDirection.Horizontal, null, true);
        }

        private void OnContinueButtonPressed(MissionResultsViewController missionResultsViewController)
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private void OnDidCloseCampaign(CampaignFlowCoordinator campaignFlowCoordinator)
        {
            Plugin.logger.Debug("closed campaign");
            unlockAllMissions = false;

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = null;
            _downloadingNode = null;
            forceClose = false;

            SongCore.Loader.SongsLoadedEvent -= OnSongsLoaded;

            foreach (Mission mission in _currentCampaign.missions)
            {
                mission.CampaignClosed();
            }

            _customCampaignUIManager.CampaignClosed();
            CampaignClosed?.Invoke(campaignFlowCoordinator);
        }

        public void OnMissionLevelSceneDidFinish(MissionLevelScenesTransitionSetupDataSO missionLevelScenesTransitionSetupDataSO, MissionCompletionResults missionCompletionResults)
        {
            Plugin.logger.Debug("updating player stats");
            var level = (_missionLevelDetailViewController.missionNode.missionData as CustomMissionDataSO).customLevel.levelID;
            var beatmapLevel = Loader.BeatmapLevelsModelSO.GetBeatmapLevelIfLoaded(level);
            IDifficultyBeatmap difficultyBeatmap = BeatmapLevelDataExtensions.GetDifficultyBeatmap(beatmapLevel.beatmapLevelData, _currentMissionDataSO.beatmapCharacteristic, _currentMissionDataSO.beatmapDifficulty);

            if (missionCompletionResults.levelCompletionResults.levelEndStateType == LevelCompletionResults.LevelEndStateType.Cleared)
            {
                PlayerLevelStatsData playerLevelStatsData = _playerDataModel.playerData.GetPlayerLevelStatsData(difficultyBeatmap.level.levelID, difficultyBeatmap.difficulty, difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic);
                LevelCompletionResults levelCompletionResults = missionCompletionResults.levelCompletionResults;
                playerLevelStatsData.UpdateScoreData(levelCompletionResults.modifiedScore, levelCompletionResults.maxCombo, levelCompletionResults.fullCombo, levelCompletionResults.rank);
            }

            if (missionCompletionResults.IsMissionComplete)
            {
                Plugin.logger.Debug("cleared mission");

                Mission mission = _currentMissionDataSO.mission;
                foreach (UnlockableItem item in mission.unlockableItems)
                {
                    Plugin.logger.Debug($"Attempting to unlock {item.name}");
                    try
                    {
                        item.UnlockItem(_currentCampaign.campaignPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to unlock item: " + item.fileName + " - Exception: " + ex.Message);
                    }
                }
                //UnlockedItemsViewController unlockedItemsViewController = Resources.FindObjectsOfTypeAll<UnlockedItemsViewController>().First();
                //unlockedItemsViewController.items = challenge.unlockableItems;
                //unlockedItemsViewController.index = 0;
                //if (unlockedItemsViewController.items.Count > 0) __instance.InvokeMethod("SetBottomScreenViewController", new object[] { unlockedItemsViewController, HMUI.ViewController.AnimationType.None });

                if (mission.unlockMap)
                {
                    _unlockableSongsManager.CompleteMission(mission.name);
                }

                if (!string.IsNullOrWhiteSpace(_currentCampaign.completionPost))
                {
                    var hash = CustomCampaignLeaderboardLibraryUtils.GetHash(mission.rawJSON);
                    var score = missionCompletionResults.levelCompletionResults.multipliedScore;
                    var requirements = new List<CompletionSubmission.Requirement>();
                    foreach (MissionObjectiveResult objective in missionCompletionResults.missionObjectiveResults)
                    {
                        var name = objective.missionObjective.type.objectiveName;
                        var value = objective.value; ;
                        CompletionSubmission.Requirement requirement = new CompletionSubmission.Requirement(name, value);
                        requirements.Add(requirement);
                    }

                    CompletionSubmission submission = new CompletionSubmission(CustomCampaignLeaderboardLibraryUtils.GetHash(mission.rawJSON), score, requirements);

                    submission.Submit(_currentCampaign.completionPost);
                }

                if (_currentCampaign.info.customMissionLeaderboard == "")
                {
                    Plugin.logger.Debug("submitting score...");
                    CustomCampaignLeaderboardLibraryUtils.SubmitScoreAsync(mission, missionCompletionResults);
                }
            }
            else
            {
                // incorrectly mark as cleared - fix it
                if (!_currentMissionCleared && _playerDataModel.playerData.GetPlayerMissionStatsData(_currentNode.missionId).cleared)
                {
                    _campaignProgressModel.__SetMissionCleared(_currentNode.missionId, false);
                }
            }

            _customCampaignUIManager.UpdateLeaderboards(true);
        }

        private void OnForcedDownloadsCancel()
        {
            _downloadManager.CancelDownloads();
            forceClose = true;
            _campaignFlowCoordinator.InvokeMethod<object, CampaignFlowCoordinator>("BackButtonWasPressed", _missionSelectionNavigationController);
        }

        private void OnDidDisableOptionalWarnings(string optionalMod)
        {
            Dictionary<string, HashSet<string>> disabledOptionalModWarnings = _config.disabledOptionalModWarnings;
            if (_config.disabledOptionalModWarnings.ContainsKey(_currentCampaign.info.name))
            {
                _config.disabledOptionalModWarnings[_currentCampaign.info.name].Add(optionalMod);
            }
            else
            {
                _config.disabledOptionalModWarnings[_currentCampaign.info.name] = new HashSet<string>() { optionalMod };
            }

            _config.Changed();
        }

        private void OnDidContinueMissingOptional()
        {
            _backingOutMissingOptional = false;
            _waitingForWarningInteraction = false;
        }

        private void OnDidBackOutMissingOptional()
        {
            _backingOutMissingOptional = true;
            _waitingForWarningInteraction = false;
        }

        private void OnDidContinueOptionalFailure()
        {
            _backingOutOptionalFailure = false;
            _waitingForWarningInteraction = false;
        }

        private void OnDidBackOutOptionalFailure()
        {
            _backingOutOptionalFailure = true;
            _waitingForWarningInteraction = false;
        }
        #endregion

        #region Helper Functions
        public async void LoadBeatmap(MissionNodeVisualController missionNodeVisualController, string songId)
        {
            bool loaded = false;
            while (!loaded)
            {
                try
                {
                    await Loader.BeatmapLevelsModelSO.GetBeatmapLevelAsync(songId, CancellationToken.None);
                    loaded = true;
                }
                catch (Exception e)
                {
                    Plugin.logger.Debug($"Could not load beatmap: {e.Message}");
                }
            }

            _missionSelectionMapViewController.GetType().GetMethod("HandleMissionNodeSelectionManagerDidSelectMissionNode", AccessTools.all)
                ?.Invoke(_missionSelectionMapViewController, new object[] { missionNodeVisualController });
        }

        public void ResetProgressIds()
        {
            _campaignProgressModel.SetField("_missionIds", new HashSet<string>());
            _campaignProgressModel.SetField("_numberOfClearedMissionsDirty", true);
        }

        private async Task<List<GameplayModifierParamsSO>> CheckForErrors(Mission mission)
        {
            List<GameplayModifierParamsSO> errorList = new List<GameplayModifierParamsSO>();

            // requiredModFailures, missingOptionalMods, optionalModFailures
            (Dictionary<string, string>, HashSet<string>, Dictionary<string, string>) failedMods = await LoadExternalModifiers(mission);

            if (failedMods.Item1.Count > 0)
            {
                foreach (var kvp in failedMods.Item1)
                {
                    string mod = kvp.Key;
                    string failureReason = kvp.Value == "" ? EXTERNAL_MOD_ERROR_DESCRIPTION : kvp.Value;
                    string errorMessage = $"{failureReason} {mod}";
                    errorList.Add(ModifierUtils.CreateModifierParam(AssetsManager.ErrorIcon, EXTERNAL_MOD_ERROR_TITLE, errorMessage));
                }
            }

            var missionData = mission.GetMissionData(_currentCampaign);
            if (missionData.beatmapCharacteristic.descriptionLocalizationKey == "ERROR NOT FOUND")
            {
                errorList.Add(ModifierUtils.CreateModifierParam(AssetsManager.ErrorIcon, CHARACTERISTIC_NOT_FOUND_ERROR_TITLE, $"{CHARACTERISTIC_NOT_FOUND_ERROR_DESCRIPTION}{mission.characteristic} {NOT_FOUND_ERROR_SUFFIX}"));
            }
            else
            {
                var difficultyBeatmap = BeatmapLevelDataExtensions.GetDifficultyBeatmap(Loader.BeatmapLevelsModelSO.GetBeatmapLevelIfLoaded((missionData as CustomMissionDataSO).customLevel.levelID).beatmapLevelData, missionData.beatmapCharacteristic, missionData.beatmapDifficulty);
                if (difficultyBeatmap == null)
                {
                    errorList.Add(ModifierUtils.CreateModifierParam(AssetsManager.ErrorIcon, DIFFICULTY_NOT_FOUND_ERROR_TITLE, $"{DIFFICULTY_NOT_FOUND_ERROR_DESCRIPTION}{mission.difficulty} {NOT_FOUND_ERROR_SUFFIX}"));
                }
                else
                {
                    DifficultyData difficultyData = Collections.RetrieveDifficultyData(difficultyBeatmap);
                    foreach (string requirement in difficultyData.additionalDifficultyData._requirements)
                    {
                        if (Collections.capabilities.Contains(requirement) || requirement.StartsWith("Complete Campaign Challenge - ")) continue;
                        errorList.Add(ModifierUtils.CreateModifierParam(AssetsManager.ErrorIcon, MISSING_CAPABILITY_ERROR_TITLE, $"{MISSING_CAPABILITY_ERROR_DESCRIPTION}{requirement}"));
                    }
                }
            }

            foreach (var requirement in mission.requirements)
            {
                if (MissionRequirement.GetObjectiveName(requirement.type) == "ERROR")
                {
                    errorList.Add(ModifierUtils.CreateModifierParam(AssetsManager.ErrorIcon, OBJECTIVE_NOT_FOUND_ERROR_TITLE, OBJECTIVE_NOT_FOUND_ERROR_DESCRIPTION));
                }
            }

            _backingOutMissingOptional = false;
            _backingOutOptionalFailure = false;
            string missingOptional = "";

            // TODO: support multiple optional mods
            foreach (var optionalMod in failedMods.Item2)
            {
                if (!OptionalWarningsDisabled(optionalMod))
                {
                    _waitingForWarningInteraction = true;

                    _modalController.didDisableOptionalWarnings -= OnDidDisableOptionalWarnings;
                    _modalController.didDisableOptionalWarnings += OnDidDisableOptionalWarnings;

                    _modalController.didContinueMissingOptional -= OnDidContinueMissingOptional;
                    _modalController.didContinueMissingOptional += OnDidContinueMissingOptional;

                    _modalController.didBackOutMissingOptional -= OnDidBackOutMissingOptional;
                    _modalController.didBackOutMissingOptional += OnDidBackOutMissingOptional;

                    missingOptional = optionalMod;
                    _modalController.ShowMissingOptionalModWarning(optionalMod);
                    break;
                }
            }

            if (!_waitingForWarningInteraction)
            {
                foreach (var kvp in failedMods.Item3)
                {
                    string optionalMod = kvp.Key;
                    Plugin.logger.Debug($"Failed optional mod: {optionalMod}");
                    _waitingForWarningInteraction = true;

                    _modalController.didContinueOptionalFailure -= OnDidContinueOptionalFailure;
                    _modalController.didContinueOptionalFailure += OnDidContinueOptionalFailure;

                    _modalController.didBackOutOptionalFailure -= OnDidBackOutOptionalFailure;
                    _modalController.didBackOutOptionalFailure += OnDidBackOutOptionalFailure;

                    missingOptional = optionalMod;
                    _modalController.ShowOptionalModFailureWarning(optionalMod, kvp.Value);
                    break;
                }
            }

            while (_waitingForWarningInteraction)
            {
                await Task.Yield();
            }

            if (_backingOutMissingOptional)
            {
                errorList.Add(ModifierUtils.CreateModifierParam(AssetsManager.ErrorIcon, MISSING_OPTIONAL_TITLE, $"{MISSING_OPTIONAL_DESCRIPTION}{missingOptional}"));
            }

            if (_backingOutOptionalFailure)
            {
                errorList.Add(ModifierUtils.CreateModifierParam(AssetsManager.ErrorIcon, OPTIONAL_FAILURE_TITLE, $"{OPTIONAL_FAILURE_DESCRIPTION}{missingOptional}"));
            }

            return errorList;
        }

        private bool OptionalWarningsDisabled(string optionalMod)
        {
            return _config.disabledOptionalModWarnings.ContainsKey(_currentCampaign.info.name) &&
                   _config.disabledOptionalModWarnings[_currentCampaign.info.name].Contains(optionalMod);
        }

        private async Task<(Dictionary<string, string>, HashSet<string>, Dictionary<string, string>)> LoadExternalModifiers(Mission mission)
        {
            return await _externalModifierManager.CheckForModLoadIssues(mission);
        }
        #endregion

        #region Affinity Patches
        [AffinityPrefix]
        [AffinityPatch(typeof(MissionLevelScenesTransitionSetupDataSO), "Init")]
        public bool MissionLevelScenesTransitionSetupDataSOInitPrefix(ref MissionLevelScenesTransitionSetupDataSO __instance,
                                                                      ref SceneInfo ____missionGameplaySceneInfo,
                                                                      ref SceneInfo ____gameCoreSceneInfo,
                                                                      string missionId,
                                                                      IDifficultyBeatmap difficultyBeatmap,
                                                                      IPreviewBeatmapLevel previewBeatmapLevel,
                                                                      MissionObjective[] missionObjectives,
                                                                      ColorScheme overrideColorScheme,
                                                                      GameplayModifiers gameplayModifiers,
                                                                      PlayerSpecificSettings playerSpecificSettings,
                                                                      string backButtonText)
        {
            __instance.SetProperty("missionId", missionId);
            __instance.SetProperty("difficultyBeatmap", difficultyBeatmap);
            EnvironmentInfoSO environmentInfo = difficultyBeatmap.GetEnvironmentInfo();

            GameplaySetupViewController gameplaySetupViewController = _campaignFlowCoordinator.GetField<GameplaySetupViewController, CampaignFlowCoordinator>("_gameplaySetupViewController");
            OverrideEnvironmentSettings overrideEnvironmentSettings = gameplaySetupViewController.environmentOverrideSettings;
            bool usingOverrideEnvironment = overrideEnvironmentSettings != null && overrideEnvironmentSettings.overrideEnvironments;

            if (usingOverrideEnvironment)
            {
                EnvironmentInfoSO overrideEnvironmentInfoForType = overrideEnvironmentSettings.GetOverrideEnvironmentInfoForType(environmentInfo.environmentType);
                if (overrideEnvironmentInfoForType != null)
                {
                    if (environmentInfo.environmentName == overrideEnvironmentInfoForType.environmentName)
                    {
                        usingOverrideEnvironment = false;
                    }
                    else
                    {
                        environmentInfo = overrideEnvironmentInfoForType;
                    }
                }
            }

            ColorScheme colorScheme = overrideColorScheme ?? new ColorScheme(environmentInfo.colorScheme);
            IBeatmapLevel level = difficultyBeatmap.level;

            __instance.SetProperty<LevelScenesTransitionSetupDataSO, GameplayCoreSceneSetupData>("gameplayCoreSceneSetupData",
                    new GameplayCoreSceneSetupData(difficultyBeatmap, previewBeatmapLevel, gameplayModifiers, playerSpecificSettings, null, false, environmentInfo, colorScheme, __instance.GetField<MainSettingsModelSO, MissionLevelScenesTransitionSetupDataSO>("_mainSettingsModel")));
            SceneInfo[] scenes = new SceneInfo[]
            {
                environmentInfo.sceneInfo,
                ____missionGameplaySceneInfo,
                ____gameCoreSceneInfo
            };
            SceneSetupData[] sceneSetupData = new SceneSetupData[]
            {
                new EnvironmentSceneSetupData(difficultyBeatmap.GetEnvironmentInfo(), previewBeatmapLevel, hideBranding: false),
                new MissionGameplaySceneSetupData(missionObjectives, playerSpecificSettings.autoRestart, level, difficultyBeatmap.difficulty, difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic, gameplayModifiers, backButtonText),
                __instance.GetProperty<GameplayCoreSceneSetupData, LevelScenesTransitionSetupDataSO>("gameplayCoreSceneSetupData"),
                new GameCoreSceneSetupData()
            };

            var scenesTransitionSetupDataSO = (ScenesTransitionSetupDataSO) __instance;
            scenesTransitionSetupDataSO.SetProperty("scenes", scenes);
            scenesTransitionSetupDataSO.SetField("_sceneSetupDataArray", sceneSetupData);

            return false;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(CampaignFlowCoordinator), "BackButtonWasPressed")]
        public bool CampaignFlowCoordinatorBackButtonWasPressedPrefix()
        {
            if (forceClose)
            {
                forceClose = false;
                return true;
            }

            if (_downloadManager.IsDownloading)
            {
                _modalController.ShowCancelDownloadConfirmation();
                return false;
            }

            return true;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(MissionToggle), "ChangeSelection")]
        public bool MissionToggleChangeSelectionPrefix()
        {
            return !_downloadManager.IsDownloading;
        }
        #endregion
    }
}
