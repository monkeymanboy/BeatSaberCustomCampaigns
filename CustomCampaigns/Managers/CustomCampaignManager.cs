using BeatSaberMarkupLanguage;
using CustomCampaignLeaderboardLibrary;
using CustomCampaigns.Campaign.Missions;
using CustomCampaigns.External;
using CustomCampaigns.External.Interfaces;
using CustomCampaigns.HarmonyPatches;
using CustomCampaigns.UI.MissionObjectiveGameUI;
using CustomCampaigns.Utils;
using HMUI;
using IPA.Utilities;
using SongCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using static SongCore.Data.ExtraSongData;

namespace CustomCampaigns.Managers
{
    public class CustomCampaignManager
    {
        public static bool unlockAllMissions = false;

        public Action<CampaignFlowCoordinator> CampaignClosed;

        internal static bool isCampaignLevel = false;
        internal static MissionDataSO currentMissionData;
        private MissionNode _currentNode;
        private bool _currentMissionCleared = false;

        internal static MissionObjectiveResult[] missionObjectiveResults;
        internal static MissionObjectiveGameUIView missionObjectiveGameUIViewPrefab = null;
        internal static MissionNode downloadingNode;

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

        private CustomCampaignUIManager _customCampaignUIManager;
        private Downloader _downloader;

        private CancellationTokenSource _cancellationTokenSource;

        private CampaignProgressModel _campaignProgressModel;

        private CampaignFlowCoordinator _campaignFlowCoordinator;

        private MenuTransitionsHelper _menuTransitionsHelper;

        private MissionSelectionMapViewController _missionSelectionMapViewController;
        private MissionNodeSelectionManager _missionNodeSelectionManager;
        private MissionSelectionNavigationController _missionSelectionNavigationController;
        private MissionLevelDetailViewController _missionLevelDetailViewController;
        private MissionResultsViewController _missionResultsViewController;

        private CustomMissionDataSO _currentMissionDataSO;
        private MissionHelpViewController _missionHelpViewController;

        private UnlockableSongsManager _unlockableSongsManager;

        private PlayerDataModel _playerDataModel;

        private ExternalModifierManager _externalModifierManager;

        public CustomCampaignManager(CustomCampaignUIManager customCampaignUIManager, Downloader downloader, CampaignFlowCoordinator campaignFlowCoordinator,
                                     MenuTransitionsHelper menuTransitionsHelper, MissionSelectionMapViewController missionSelectionMapViewController,
                                     MissionSelectionNavigationController missionSelectionNavigationController, MissionLevelDetailViewController missionLevelDetailViewController,
                                     MissionResultsViewController missionResultsViewController, MissionHelpViewController missionHelpViewController,
                                     UnlockableSongsManager unlockableSongsManager, PlayerDataModel playerDataModel, ExternalModifierManager externalModifierManager)
        {
            _customCampaignUIManager = customCampaignUIManager;
            _downloader = downloader;

            _campaignFlowCoordinator = campaignFlowCoordinator;
            _campaignProgressModel = _customCampaignUIManager.CampaignFlowCoordinator.GetField<CampaignProgressModel, CampaignFlowCoordinator>("_campaignProgressModel");

            _menuTransitionsHelper = menuTransitionsHelper;
            _missionSelectionMapViewController = missionSelectionMapViewController;
            _missionNodeSelectionManager = missionSelectionMapViewController.GetField<MissionNodeSelectionManager, MissionSelectionMapViewController>("_missionNodeSelectionManager");

            _missionSelectionNavigationController = missionSelectionNavigationController;
            _missionLevelDetailViewController = missionLevelDetailViewController;
            _missionResultsViewController = missionResultsViewController;
            _missionHelpViewController = missionHelpViewController;

            _unlockableSongsManager = unlockableSongsManager;
            _playerDataModel = playerDataModel;

            _externalModifierManager = externalModifierManager;
        }

        #region CampaignInit
        public void FirstActivation()
        {
            _customCampaignUIManager.FirstActivation();
        }

        public void FlowCoordinatorPresented()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _missionNodeSelectionManager.didSelectMissionNodeEvent -= _missionSelectionMapViewController.HandleMissionNodeSelectionManagerDidSelectMissionNode;
            _missionNodeSelectionManager.didSelectMissionNodeEvent -= OnDidSelectMissionNode;
            _missionNodeSelectionManager.didSelectMissionNodeEvent += OnDidSelectMissionNode;

            _missionLevelDetailViewController.didPressPlayButtonEvent -= _missionSelectionNavigationController.HandleMissionLevelDetailViewControllerDidPressPlayButton;
            _missionLevelDetailViewController.didPressPlayButtonEvent -= OnDidPressPlayButton;
            _missionLevelDetailViewController.didPressPlayButtonEvent += OnDidPressPlayButton;

            _missionResultsViewController.retryButtonPressedEvent -= _campaignFlowCoordinator.HandleMissionResultsViewControllerRetryButtonPressed;
            _missionResultsViewController.retryButtonPressedEvent -= OnRetryButtonPressed;
            _missionResultsViewController.retryButtonPressedEvent += OnRetryButtonPressed;

            _campaignFlowCoordinator.didFinishEvent -= BeatSaberUI.MainFlowCoordinator.HandleCampaignFlowCoordinatorDidFinish;
            _campaignFlowCoordinator.didFinishEvent -= OnDidCloseCampaign;
            _campaignFlowCoordinator.didFinishEvent += OnDidCloseCampaign;

            _missionSelectionMapViewController.didSelectMissionLevelEvent -= OnDidSelectMissionLevel;
            _missionSelectionMapViewController.didSelectMissionLevelEvent += OnDidSelectMissionLevel;

            _missionResultsViewController.continueButtonPressedEvent -= OnContinueButtonPressed;
            _missionResultsViewController.continueButtonPressedEvent += OnContinueButtonPressed;

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
            _campaignFlowCoordinator.didFinishEvent -= BeatSaberUI.MainFlowCoordinator.HandleCampaignFlowCoordinatorDidFinish;
            _campaignFlowCoordinator.didFinishEvent += BeatSaberUI.MainFlowCoordinator.HandleCampaignFlowCoordinatorDidFinish;

            _missionNodeSelectionManager.didSelectMissionNodeEvent -= OnDidSelectMissionNode;
            _missionNodeSelectionManager.didSelectMissionNodeEvent -= _missionSelectionMapViewController.HandleMissionNodeSelectionManagerDidSelectMissionNode;
            _missionNodeSelectionManager.didSelectMissionNodeEvent += _missionSelectionMapViewController.HandleMissionNodeSelectionManagerDidSelectMissionNode;

            _missionLevelDetailViewController.didPressPlayButtonEvent -= OnDidPressPlayButton;
            _missionLevelDetailViewController.didPressPlayButtonEvent -= _missionSelectionNavigationController.HandleMissionLevelDetailViewControllerDidPressPlayButton;
            _missionLevelDetailViewController.didPressPlayButtonEvent += _missionSelectionNavigationController.HandleMissionLevelDetailViewControllerDidPressPlayButton;

            _missionResultsViewController.retryButtonPressedEvent -= OnRetryButtonPressed;
            _missionResultsViewController.retryButtonPressedEvent -= _campaignFlowCoordinator.HandleMissionResultsViewControllerRetryButtonPressed;
            _missionResultsViewController.retryButtonPressedEvent += _campaignFlowCoordinator.HandleMissionResultsViewControllerRetryButtonPressed;

            _campaignFlowCoordinator.didFinishEvent -= OnDidCloseCampaign;
            _campaignFlowCoordinator.didFinishEvent -= BeatSaberUI.MainFlowCoordinator.HandleCampaignFlowCoordinatorDidFinish;
            _campaignFlowCoordinator.didFinishEvent += BeatSaberUI.MainFlowCoordinator.HandleCampaignFlowCoordinatorDidFinish;

            _missionSelectionMapViewController.didSelectMissionLevelEvent -= OnDidSelectMissionLevel;

            _missionResultsViewController.continueButtonPressedEvent -= OnContinueButtonPressed;

            CampaignFlowCoordinatorHandleMissionLevelSceneDidFinishPatch.onMissionSceneFinish -= OnMissionLevelSceneDidFinish;

            _customCampaignUIManager.BaseCampaignEnabled();
        }
        #endregion

        #region Events
        private void OnDidSelectMissionNode(MissionNodeVisualController missionNodeVisualController)
        {
            if (downloadingNode != null)
            {
                Plugin.logger.Error("Should never reach here - was downloading");
                return;
            }

            _missionLevelDetailViewController.didPressPlayButtonEvent -= _missionSelectionNavigationController.HandleMissionLevelDetailViewControllerDidPressPlayButton;
            //CustomPreviewBeatmapLevel level = (missionNodeVisualController.missionNode.missionData as CustomMissionDataSO).customLevel;
            CustomPreviewBeatmapLevel level = (missionNodeVisualController.missionNode.missionData as CustomMissionDataSO).mission.FindSong();
            if (level == null)
            {
                _missionSelectionMapViewController.HandleMissionNodeSelectionManagerDidSelectMissionNode(missionNodeVisualController);
                _customCampaignUIManager.SetPlayButtonText("DOWNLOAD");
            }
            else
            {
                Plugin.logger.Debug("found level");
                _customCampaignUIManager.SetPlayButtonText("PLAY");

                LoadBeatmap(missionNodeVisualController, (missionNodeVisualController.missionNode.missionData as CustomMissionDataSO).customLevel.levelID);
            }
            _customCampaignUIManager.SetPlayButtonInteractable(true);
        }

        private void OnDidSelectMissionLevel(MissionSelectionMapViewController missionSelectionMapViewController, MissionNode missionNode)
        {
            Mission mission = (missionNode.missionData as CustomMissionDataSO).mission;
            _customCampaignUIManager.SetMissionName(mission.name);
            _customCampaignUIManager.MissionLevelSelected(mission);

            _currentNode = missionNode;

            _customCampaignUIManager.CreateModifierParamsList(missionNode);
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

        private async void DownloadMap(MissionNode missionNode)
        {
            downloadingNode = missionNode;
            CustomMissionDataSO customMissionData = missionNode.missionData as CustomMissionDataSO;
            Mission mission = customMissionData.mission;
            _customCampaignUIManager.SetPlayButtonText("DOWNLOADING...");
            _customCampaignUIManager.SetPlayButtonInteractable(false);

            if (mission.customDownloadURL != "")
            {
                var path = Path.Combine(CustomLevelPathHelper.customLevelsDirectoryPath, mission.songid);
                path = _downloader.DeterminePathNumber(path, _cancellationTokenSource.Token);
                await _downloader.DownloadMapFromUrlAsync(mission.customDownloadURL, path, _cancellationTokenSource.Token, OnDownloadSucceeded, OnDownloadFailed, OnDownloadProgressUpdated, OnDownloadStatusUpdate);
                return;
            }

            if (mission.hash != "")
            {
                await _downloader.DownloadMapByHashAsync(mission.hash, mission.songid, _cancellationTokenSource.Token, OnDownloadSucceeded, OnDownloadFailed, OnDownloadProgressUpdated, OnDownloadStatusUpdate);
                return;
            }

            await _downloader.DownloadMapByIDAsync(mission.songid, _cancellationTokenSource.Token, OnDownloadSucceeded, OnDownloadFailed, OnDownloadProgressUpdated, OnDownloadStatusUpdate);
        }

        private void OnDownloadProgressUpdated(float progress)
        {
            _customCampaignUIManager.UpdateProgress(progress);
        }

        private void OnDownloadStatusUpdate(string status)
        {
            Plugin.logger.Debug($"Download status update: {status}");
            if (_missionLevelDetailViewController.missionNode == downloadingNode)
            {
                _customCampaignUIManager.SetPlayButtonText(status);
            }
        }

        private void OnDownloadFailed()
        {
            Plugin.logger.Debug("Download for map failed :(");
            _customCampaignUIManager.ClearProgressBar();
            if (_missionLevelDetailViewController.missionNode == downloadingNode)
            {
                downloadingNode = null;
                _customCampaignUIManager.SetPlayButtonText("DOWNLOAD");
                _customCampaignUIManager.SetPlayButtonInteractable(true);
            }
            else
            {
                Plugin.logger.Error("Currently selected node is not the downloading one");
            }
        }

        private void OnDownloadSucceeded()
        {
            SongCore.Loader.SongsLoadedEvent += OnSongsLoaded;
            SongCore.Loader.Instance.RefreshSongs();
        }

        private void OnSongsLoaded(Loader loader, ConcurrentDictionary<string, CustomPreviewBeatmapLevel> levels)
        {
            Plugin.logger.Debug("songs loaded");
            SongCore.Loader.SongsLoadedEvent -= OnSongsLoaded;
            (_missionLevelDetailViewController.missionNode.missionData as CustomMissionDataSO).mission.SetCustomLevel();
            _customCampaignUIManager.RefreshMissionNodeData();
            _customCampaignUIManager.ClearProgressBar();

            bool isDownloadingNode = _missionLevelDetailViewController.missionNode == downloadingNode;
            if (!isDownloadingNode)
            {
                Plugin.logger.Error("Currently selected node is not the downloading one");
            }

            downloadingNode = null;
            _customCampaignUIManager.SetPlayButtonText("Play");
            _customCampaignUIManager.SetPlayButtonInteractable(true);
            _missionNodeSelectionManager.GetField<Action<MissionNodeVisualController>, MissionNodeSelectionManager>("didSelectMissionNodeEvent")(_missionLevelDetailViewController.missionNode.missionNodeVisualController);
        }

        private void PlayMap(MissionLevelDetailViewController missionLevelDetailViewController)
        {
            Plugin.logger.Debug("play");
            _currentNode = missionLevelDetailViewController.missionNode;
            MissionDataSO missionDataSO = _currentNode.missionData;
            _currentMissionDataSO = missionDataSO as CustomMissionDataSO;
            currentMissionData = _currentMissionDataSO;

            _currentMissionCleared = _playerDataModel.playerData.GetPlayerMissionStatsData(_currentNode.missionId).cleared;
            
            Mission mission = _currentMissionDataSO.mission;

            var errors = CheckForErrors(mission);

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
                        _missionSelectionNavigationController.HandleMissionLevelDetailViewControllerDidPressPlayButton(missionLevelDetailViewController);
                    }
                    else
                    {
                        StartCampaignLevel(null);
                    }
                }
            }
            else
            {
                _missionSelectionNavigationController.HandleMissionLevelDetailViewControllerDidPressPlayButton(missionLevelDetailViewController);
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
                _missionSelectionNavigationController.HandleMissionLevelDetailViewControllerDidPressPlayButton(_missionLevelDetailViewController);
            }
            else
            {
                StartCampaignLevel(null);
            }
        }

        private void StartCampaignLevel(Action beforeSceneSwitchCallback)
        {
            isCampaignLevel = true;

            var level = (_missionLevelDetailViewController.missionNode.missionData as CustomMissionDataSO).customLevel.levelID;
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
                                                        gameplayModifiers,
                                                        playerSettings,
                                                        null,
                                                        "Menu",
                                                        false,
                                                        beforeSceneSwitchCallback,
                                                        OnFinishedStandardLevel);
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
            _campaignFlowCoordinator.HandleMissionLevelSceneDidFinish(null, missionCompletionResults);
            isCampaignLevel = false;
        }

        private void OnRetryButtonPressed(MissionResultsViewController missionResultsViewController)
        {
            Plugin.logger.Debug("retry button pressed");

            Mission mission = _currentMissionDataSO.mission;
            HashSet<string> failedMods = LoadExternalModifiers(mission);

            if (failedMods.Count > 0)
            {
                Plugin.logger.Error("Error loading external modifiers");
                return;
            }

            if (mission.allowStandardLevel)
            {
                StartCampaignLevel(HideMissionResults);
            }
            else
            {
                _campaignFlowCoordinator.HandleMissionResultsViewControllerRetryButtonPressed(missionResultsViewController);
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
            _customCampaignUIManager.UpdateLeaderboards(true);
            _customCampaignUIManager.LoadModifiersPanel();
        }

        private void OnDidCloseCampaign(CampaignFlowCoordinator campaignFlowCoordinator)
        {
            Plugin.logger.Debug("closed campaign");
            unlockAllMissions = false;

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = null;
            downloadingNode = null;

            _customCampaignUIManager.CampaignClosed();
            CampaignClosed?.Invoke(campaignFlowCoordinator);
        }

        public void OnMissionLevelSceneDidFinish(MissionLevelScenesTransitionSetupDataSO missionLevelScenesTransitionSetupDataSO, MissionCompletionResults missionCompletionResults)
        {
            Plugin.logger.Debug("updating player stats");
            var level = (_missionLevelDetailViewController.missionNode.missionData as CustomMissionDataSO).customLevel.levelID;
            var beatmapLevel = Loader.BeatmapLevelsModelSO.GetBeatmapLevelIfLoaded(level);
            IDifficultyBeatmap difficultyBeatmap = BeatmapLevelDataExtensions.GetDifficultyBeatmap(beatmapLevel.beatmapLevelData, _currentMissionDataSO.beatmapCharacteristic, _currentMissionDataSO.beatmapDifficulty);

            PlayerLevelStatsData playerLevelStatsData = _playerDataModel.playerData.GetPlayerLevelStatsData(difficultyBeatmap.level.levelID, difficultyBeatmap.difficulty, difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic);
            LevelCompletionResults levelCompletionResults = missionCompletionResults.levelCompletionResults;
            playerLevelStatsData.UpdateScoreData(levelCompletionResults.modifiedScore, levelCompletionResults.maxCombo, levelCompletionResults.fullCombo, levelCompletionResults.rank);

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
                    var score = missionCompletionResults.levelCompletionResults.rawScore;
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

                Plugin.logger.Debug("submitting score...");
                CustomCampaignLeaderboardLibraryUtils.SubmitScoreAsync(mission, missionCompletionResults);
            }
            else
            {
                // incorrectly mark as cleared - fix it
                if (!_currentMissionCleared && _playerDataModel.playerData.GetPlayerMissionStatsData(_currentNode.missionId).cleared)
                {
                    _campaignProgressModel.__SetMissionCleared(_currentNode.missionId, false);
                }
            }
        }
        #endregion

        #region Helper Functions
        public async void LoadBeatmap(MissionNodeVisualController missionNodeVisualController, string songId)
        {
            await Loader.BeatmapLevelsModelSO.GetBeatmapLevelAsync(songId, CancellationToken.None);
            _missionSelectionMapViewController.HandleMissionNodeSelectionManagerDidSelectMissionNode(missionNodeVisualController);
        }

        public void ResetProgressIds()
        {
            _campaignProgressModel.SetField("_missionIds", new HashSet<string>());
            _campaignProgressModel.SetField("_numberOfClearedMissionsDirty", true);
        }

        private List<GameplayModifierParamsSO> CheckForErrors(Mission mission)
        {
            List<GameplayModifierParamsSO> errorList = new List<GameplayModifierParamsSO>();
            HashSet<string> failedMods = LoadExternalModifiers(mission);
            if (failedMods.Count > 0)
            {
                foreach (var mod in failedMods)
                {
                    errorList.Add(ModifierUtils.CreateModifierParam(AssetsManager.ErrorIcon, EXTERNAL_MOD_ERROR_TITLE, $"{EXTERNAL_MOD_ERROR_DESCRIPTION} {mod}"));
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

            return errorList;
        }

        private HashSet<string> LoadExternalModifiers(Mission mission)
        {
            return _externalModifierManager.CheckForModLoadIssues(mission);
        }
        #endregion
    }
}
