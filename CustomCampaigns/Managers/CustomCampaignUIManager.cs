using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using CustomCampaigns.Campaign;
using CustomCampaigns.Campaign.Missions;
using CustomCampaigns.External;
using CustomCampaigns.HarmonyPatches.ScoreSaber;
using CustomCampaigns.UI.FlowCoordinators;
using CustomCampaigns.UI.GameplaySetupUI;
using CustomCampaigns.UI.ViewControllers;
using CustomCampaigns.Utils;
using HarmonyLib;
using HMUI;
using IPA.Utilities;
using Polyglot;
using SiraUtil.Affinity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomCampaigns.Managers
{
    public class CustomCampaignUIManager : IAffinity
    {
        public const float EDITOR_TO_GAME_UNITS = 30f / 111;
        public const float HEIGHT_OFFSET = 20;

        private const int YEET_AMOUNT = 10000;

        public Action customCampaignEnabledEvent;
        public Action baseCampaignEnabledEvent;
        public Action<Mission, string, Color> missionDataReadyEvent;
        public Action leaderboardUpdateEvent;

        public CampaignFlowCoordinator CampaignFlowCoordinator { get => _campaignFlowCoordinator; }
        private CampaignFlowCoordinator _campaignFlowCoordinator;

        private MissionMapAnimationController _missionMapAnimationController;
        private MissionNodesManager _missionNodesManager;
        private MissionStagesManager _missionStagesManager;
        private MissionConnectionsGenerator _missionConnectionsGenerator;
        private MissionSelectionMapViewController _missionSelectionMapViewController;
        private MissionNodeSelectionManager _missionNodeSelectionManager;
        private MissionSelectionNavigationController _missionSelectionNavigationController;
        private MissionLevelDetailViewController _missionLevelDetailViewController;
        private MissionResultsViewController _missionResultsViewController;

        private Button _playButton;

        private MissionNode[] _baseMissionNodes;
        private MissionNode _baseRootMissionNode;
        private MissionNode _baseFinalMissionNode;
        private MissionStage[] _baseMissionStages;
        private Sprite _baseBackground;
        private float _baseBackAlpha;
        private float _baseMapHeight;
        private MenuLightsPresetSO _baseDefaultLights;

        private ScrollView _mapScrollView;
        private ScrollViewItemsVisibilityController _mapScrollViewItemsVisibilityController;
        private Image _backgroundImage;

        private GameplayModifierInfoListItemsList _gameplayModifierInfoListItemsList;
        private GameObject _modifiersPanelGO;
        private GameplayModifiersModelSO _gameplayModifiersModel;
        private List<GameplayModifierParamsSO> _modifierParams;

        private MissionNode[] _currentCampaignNodes;
        private MissionStage[] _currentMissionStages;

        private CampaignMissionLeaderboardViewController _campaignMissionLeaderboardViewController;
        private PlatformLeaderboardViewController _globalLeaderboardViewController;

        private Vector3 _leaderboardPosition;

        [UIComponent("mission-name")]
        public TextMeshProUGUI MissionName;

        private ImageView _levelBarBackground;
        private Color _originalLevelBarBackgroundColor;
        private Color _lowProgressColor;
        private Color _mediumProgressColor;
        private Color _highProgressColor;

        private CustomMissionDataSO _missionData;

        private bool _inCustomCampaign = false;

        private LevelParamsPanel _levelParamsPanelBase;
        private LevelParamsPanel _levelParamsPanel;

        private HoverHintController _hoverHintController;

        private Vector2 _objectivesSizeDelta;

        private Config _config;

        private GameplaySetupManager _gameplaySetupManager;
        private SettingsHandler _settingsHandler;

        private Transform _lastShownMissionHelp;
        private Dictionary<string, Sprite> _loadedSprites = new Dictionary<string, Sprite>();

        public CustomCampaignUIManager(CampaignFlowCoordinator campaignFlowCoordinator, MissionSelectionMapViewController missionSelectionMapViewController, MissionSelectionNavigationController missionSelectionNavigationController,
                                        MissionLevelDetailViewController missionLevelDetailViewController, MissionResultsViewController missionResultsViewController, StandardLevelDetailViewController standardLevelDetailViewController,
                                        CampaignMissionLeaderboardViewController campaignMissionLeaderboardViewController, PlatformLeaderboardViewController globalLeaderboardViewController,
                                        Config config, GameplaySetupManager gameplaySetupManager, SettingsHandler settingsHandler, HoverHintController hoverHintController)
        {
            _campaignFlowCoordinator = campaignFlowCoordinator;
            _missionSelectionMapViewController = missionSelectionMapViewController;
            _missionSelectionNavigationController = missionSelectionNavigationController;
            _missionLevelDetailViewController = missionLevelDetailViewController;
            _missionResultsViewController = missionResultsViewController;

            _playButton = _missionLevelDetailViewController.GetField<Button, MissionLevelDetailViewController>("_playButton");

            _missionMapAnimationController = _missionSelectionMapViewController.GetField<MissionMapAnimationController, MissionSelectionMapViewController>("_missionMapAnimationController");
            _missionNodeSelectionManager = _missionSelectionMapViewController.GetField<MissionNodeSelectionManager, MissionSelectionMapViewController>("_missionNodeSelectionManager");

            _missionNodesManager = _missionMapAnimationController.GetField<MissionNodesManager, MissionMapAnimationController>("_missionNodesManager");
            _missionStagesManager = _missionNodesManager.GetProperty<MissionStagesManager, MissionNodesManager>("missionStagesManager");

            _mapScrollView = _missionSelectionMapViewController.GetField<ScrollView, MissionSelectionMapViewController>("_mapScrollView");
            _mapScrollViewItemsVisibilityController = _mapScrollView.GetComponent<ScrollViewItemsVisibilityController>();
            _backgroundImage = _mapScrollView.GetComponentsInChildren<Image>().First(x => x.name == "Map");
            _missionConnectionsGenerator = _mapScrollView.GetComponentsInChildren<MissionConnectionsGenerator>().First();

            _gameplayModifierInfoListItemsList = _missionLevelDetailViewController.GetField<GameplayModifierInfoListItemsList, MissionLevelDetailViewController>("_gameplayModifierInfoListItemsList");
            _modifiersPanelGO = _missionLevelDetailViewController.GetField<GameObject, MissionLevelDetailViewController>("_modifiersPanelGO");

            _gameplayModifiersModel = _missionLevelDetailViewController.GetField<GameplayModifiersModelSO, MissionLevelDetailViewController>("_gameplayModifiersModel");

            var standardLevelDetailView = standardLevelDetailViewController.GetField<StandardLevelDetailView, StandardLevelDetailViewController>("_standardLevelDetailView");
            _levelParamsPanelBase = standardLevelDetailView.GetField<LevelParamsPanel, StandardLevelDetailView>("_levelParamsPanel");

            _hoverHintController = hoverHintController;

            _campaignMissionLeaderboardViewController = campaignMissionLeaderboardViewController;
            _globalLeaderboardViewController = globalLeaderboardViewController;

            _config = config;

            _gameplaySetupManager = gameplaySetupManager;
            _settingsHandler = settingsHandler;

            _leaderboardPosition = new Vector3(0, _config.floorLeaderboardPosition, 0);

            var levelBar = _missionLevelDetailViewController.GetField<LevelBar, MissionLevelDetailViewController>("_levelBar");
            _levelBarBackground = levelBar.transform.GetChild(0).GetComponent<ImageView>();
            _originalLevelBarBackgroundColor = _levelBarBackground.color;

            _lowProgressColor = new Color(0.91f, 0.57f, 0.06f, 0.25f);
            _mediumProgressColor = new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, 0.25f);
            _highProgressColor = new Color(Color.green.r, Color.green.g, Color.green.b, 0.25f);

            _levelBarBackground.sprite = Sprite.Create(new Texture2D(1, 1), new Rect(0, 0, 1, 1), Vector2.one / 2f);
        }

        #region UI Setup
        internal void FirstActivation()
        {
            BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "CustomCampaigns.UI.Views.mission-detail.bsml"), _missionLevelDetailViewController.gameObject, this);
            if (!_missionNodesManager.IsInitialized)
            {
                _missionNodesManager.GetType().GetMethod("Awake", AccessTools.all)?.Invoke(_missionNodesManager, null);
            }
            _baseMissionNodes = _missionNodesManager.GetField<MissionNode[], MissionNodesManager>("_allMissionNodes");
            _baseRootMissionNode = _missionNodesManager.GetField<MissionNode, MissionNodesManager>("_rootMissionNode");
            _baseFinalMissionNode = _missionNodesManager.GetField<MissionNode, MissionNodesManager>("_finalMissionNode");
            _baseMissionStages = _missionStagesManager.GetField<MissionStage[], MissionStagesManager>("_missionStages");
            _baseBackground = _backgroundImage.sprite;
            _baseBackAlpha = _backgroundImage.color.a;
            _baseMapHeight = _mapScrollView.GetField<RectTransform, ScrollView>("_contentRectTransform").sizeDelta.y;
            _baseDefaultLights = _campaignFlowCoordinator.GetField<MenuLightsPresetSO, CampaignFlowCoordinator>("_defaultLightsPreset");
        }

        internal void CustomCampaignEnabled()
        {
            Plugin.logger.Debug("custom campaign enabled");
            MissionName.alignment = TextAlignmentOptions.Bottom;
            MissionName.gameObject.SetActive(true);

            if (!_config.floorLeaderboard)
            {
                customCampaignEnabledEvent?.Invoke();
            }

            YeetBaseGameNodes();
            _settingsHandler.PropertyChanged += OnSettingsChanged;

            PanelViewShowPatch.ViewShown -= OnViewActivated;
            PanelViewShowPatch.ViewShown += OnViewActivated;

            InitializeLevelParamsPanel();
            _inCustomCampaign = true;
        }

        internal void SetupCampaignUI(Campaign.Campaign campaign)
        {
            SetCampaignBackground(campaign);
            SetCampaignLights(campaign);
            SetCampaignMissionNodes(campaign);

            _mapScrollView.GetField<RectTransform, ScrollView>("_contentRectTransform").SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, campaign.info.mapHeight * EDITOR_TO_GAME_UNITS + HEIGHT_OFFSET);
            _settingsHandler.SetFloorLeaderboardSettingVisibility(false);

            InitializeCampaignUI();
        }

        internal void FlowCoordinatorPresented(Campaign.Campaign campaign)
        {
            _campaignFlowCoordinator.InvokeMethod<object, CampaignFlowCoordinator>("SetTitle", campaign.info.name, ViewController.AnimationType.In);
            _missionMapAnimationController.ScrollToTopMostNotClearedMission();
            _missionLevelDetailViewController.GetField<Button, MissionLevelDetailViewController>("_playButton").interactable = true;
            SetupGameplaySetupViewController();
        }

        private void YeetBaseGameNodes()
        {
            foreach (var node in _baseMissionNodes)
            {
                node.transform.localPosition += new Vector3(0, -YEET_AMOUNT, 0);
            }
        }

        private void UnYeetBaseGameNodes()
        {
            foreach (var node in _baseMissionNodes)
            {
                node.transform.localPosition += new Vector3(0, YEET_AMOUNT, 0);
            }
        }

        private async void SetCampaignBackground(Campaign.Campaign campaign)
        {
            await campaign.LoadBackground();
            if (campaign.background != null)
            {
                _backgroundImage.color = new Color(1, 1, 1, campaign.info.backgroundAlpha);
                _backgroundImage.sprite = campaign.background;
            }
            else
            {
                _backgroundImage.color = new Color(1, 1, 1, 0);
            }
        }

        private void SetCampaignLights(Campaign.Campaign campaign)
        {
            MenuLightsPresetSO customLights = UnityEngine.Object.Instantiate(_baseDefaultLights);

            SimpleColorSO lightColor = ScriptableObject.CreateInstance<SimpleColorSO>();
            lightColor.SetColor(new Color(campaign.info.lightColor.r, campaign.info.lightColor.g, campaign.info.lightColor.b));
            foreach (var pair in customLights.lightIdColorPairs)
            {
                pair.baseColor = lightColor;
            }
            _campaignFlowCoordinator.SetField("_defaultLightsPreset", customLights);
        }

        private void SetCampaignMissionNodes(Campaign.Campaign campaign)
        {
            SetCampaignMissionStages(campaign);
            CreateMissionNodes(campaign);
            SetMissionNodeChildren(campaign);
        }

        private void SetCampaignMissionStages(Campaign.Campaign campaign)
        {
            Plugin.logger.Debug("Setting mission stages");
            var missionNodesPO = _missionNodesManager.GetField<GameObject, MissionNodesManager>("_missionNodesParentObject");
            _currentMissionStages = new MissionStage[campaign.info.unlockGate.Count + 1];

            for (int i = 0; i < _currentMissionStages.Length - 1; i++)
            {
                if (_baseMissionStages == null)
                {
                    Plugin.logger.Debug("null mission stages");
                }
                if (missionNodesPO == null)
                {
                    Plugin.logger.Debug("null mission nodes po");
                }
                _currentMissionStages[i] = UnityEngine.Object.Instantiate(_baseMissionStages[0], missionNodesPO.transform);
                _currentMissionStages[i].SetField("_minimumMissionsToUnlock", campaign.info.unlockGate[i].clearsToPass);
                _currentMissionStages[i].GetField<RectTransform, MissionStage>("_rectTransform").localPosition = GetCampaignPosition(campaign.info.unlockGate[i].x, campaign.info.unlockGate[i].y, campaign.info.mapHeight);
            }

            // create a fake gate - campaign is coded very strangely and at least one unlock gate must be locked at all times
            _currentMissionStages[campaign.info.unlockGate.Count] = UnityEngine.Object.Instantiate(_baseMissionStages[0], missionNodesPO.transform);
            _currentMissionStages[campaign.info.unlockGate.Count].SetField("_minimumMissionsToUnlock", campaign.info.mapPositions.Count + 1);
            _currentMissionStages[campaign.info.unlockGate.Count].GetField<RectTransform, MissionStage>("_rectTransform").localPosition = GetCampaignPosition(0, -1000, campaign.info.mapHeight);

            _missionStagesManager.SetField("_missionStages", _currentMissionStages.OrderBy(x => x.minimumMissionsToUnlock).ToArray());
        }

        private void CreateMissionNodes(Campaign.Campaign campaign)
        {
            _currentCampaignNodes = new MissionNode[campaign.info.mapPositions.Count];
            var missionNodesPO = _missionNodesManager.GetField<GameObject, MissionNodesManager>("_missionNodesParentObject");
            for (int i = 0; i < _currentCampaignNodes.Length; i++)
            {
                CampaignMapPosition mapPosition = campaign.info.mapPositions[i];

                _currentCampaignNodes[i] = UnityEngine.Object.Instantiate(_baseMissionNodes[0], missionNodesPO.transform);
                _currentCampaignNodes[i].gameObject.SetActive(true);
                _currentCampaignNodes[i].SetField("_missionDataSO", campaign.missions[i].GetMissionData(campaign));

                var missionNodeTransform = _currentCampaignNodes[i].GetField<RectTransform, MissionNode>("_rectTransform");

                missionNodeTransform.localPosition = GetCampaignPosition(mapPosition.x, mapPosition.y, campaign.info.mapHeight);
                missionNodeTransform.sizeDelta = new Vector2(12 * mapPosition.scale, 12 * mapPosition.scale);

                _currentCampaignNodes[i].SetField("_letterPartName", mapPosition.letterPortion);
                _currentCampaignNodes[i].SetField("_numberPartName", mapPosition.numberPortion);
            }

            SetNodeDesigns(campaign);
        }

        private async void SetNodeDesigns(Campaign.Campaign campaign)
        {
            Plugin.logger.Debug("setting node designs");
            await campaign.LoadNodeSprites();
            for (int i = 0; i < _currentCampaignNodes.Length; i++)
            {
                string nodeDefaultColorText = campaign.info.mapPositions[i].nodeDefaultColor;
                string nodeHighlightColorText = campaign.info.mapPositions[i].nodeHighlightColor;
                Color nodeDefaultColor;
                Color nodeHighlightColor;

                var missionNodeVisualController = _currentCampaignNodes[i].GetField<MissionNodeVisualController, MissionNode>("_missionNodeVisualController");
                var missionToggle = missionNodeVisualController.GetField<MissionToggle, MissionNodeVisualController>("_missionToggle");

                if (campaign.info.mapPositions[i].nodeOutline != null)
                {
                    var strokeImage = missionToggle.GetField<Image, MissionToggle>("_strokeImage");
                    strokeImage.sprite = campaign.info.mapPositions[i].nodeOutline;
                }

                if (campaign.info.mapPositions[i].nodeBackground != null)
                {
                    var backgroundImage = missionToggle.GetField<Image, MissionToggle>("_bgImage");
                    backgroundImage.sprite = campaign.info.mapPositions[i].nodeBackground;
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

                missionToggle.GetType().GetMethod("RefreshUI", AccessTools.all)?.Invoke(missionToggle, null);
            }
        }

        private void SetMissionNodeChildren(Campaign.Campaign campaign)
        {
            Plugin.logger.Debug("Setting mission node children");
            for (int i = 0; i < _currentCampaignNodes.Length; i++)
            {
                MissionNode[] children = new MissionNode[campaign.info.mapPositions[i].childNodes.Length];
                for (int j = 0; j < children.Length; j++)
                {
                    children[j] = _currentCampaignNodes[campaign.info.mapPositions[i].childNodes[j]];
                }
                _currentCampaignNodes[i].SetField("_childNodes", children);
            }

            _missionNodesManager.SetField("_rootMissionNode", _currentCampaignNodes[0]);
        }

        private void InitializeCampaignUI()
        {
            var nodeSelectionStartMethodInfo = _missionNodeSelectionManager.GetType().GetMethod("Start", AccessTools.all);
            var nodesManagerAwakeMethodInfo = _missionNodesManager.GetType().GetMethod("Awake", AccessTools.all);
            var nodeSelectionOnDestroyMethodInfo = _missionNodeSelectionManager.GetType().GetMethod("OnDestroy", AccessTools.all);
            var createNodeConnectionsMethodInfo = _missionConnectionsGenerator.GetType().GetMethod("CreateNodeConnections", AccessTools.all);
            var resetAllNodesMethodInfo = _missionNodesManager.GetType().GetMethod("ResetAllNodes",        AccessTools.all);
            var setupNodeConnectionsMethodInfo = _missionNodesManager.GetType().GetMethod("SetupNodeConnections", AccessTools.all);
            var mapScrollViewAwakeMethodInfo = _mapScrollView.GetType().GetMethod("Awake", AccessTools.all);
            var mapScrollViewOnDestroyMethodInfo = _mapScrollView.GetType().GetMethod("OnDestroy", AccessTools.all);
            var mapScrollViewItemsVisibilityControllerStartMethodInfo = _mapScrollViewItemsVisibilityController.GetType().GetMethod("Start", AccessTools.all);

            if (_missionNodeSelectionManager.GetField<MissionNode[], MissionNodeSelectionManager>("_missionNodes") == null)
            {
                nodeSelectionStartMethodInfo?.Invoke(_missionNodeSelectionManager, null);
            }

            nodeSelectionOnDestroyMethodInfo?.Invoke(_missionNodeSelectionManager, null);

            CustomCampaignFlowCoordinator.CustomCampaignManager.ResetProgressIds();

            nodesManagerAwakeMethodInfo?.Invoke(_missionNodesManager, null);
            nodeSelectionStartMethodInfo?.Invoke(_missionNodeSelectionManager, null);
            createNodeConnectionsMethodInfo?.Invoke(_missionConnectionsGenerator, null);
            resetAllNodesMethodInfo?.Invoke(_missionNodesManager, null);
            setupNodeConnectionsMethodInfo?.Invoke(_missionNodesManager, null);
            mapScrollViewOnDestroyMethodInfo?.Invoke(_mapScrollView, null);
            mapScrollViewAwakeMethodInfo?.Invoke(_mapScrollView, null);

            _missionMapAnimationController.ScrollToTopMostNotClearedMission();
            mapScrollViewItemsVisibilityControllerStartMethodInfo?.Invoke(_mapScrollViewItemsVisibilityController, null);
        }

        private void SetupGameplaySetupViewController()
        {
            GameplaySetupViewController gameplaySetupViewController = _campaignFlowCoordinator.GetField<GameplaySetupViewController, CampaignFlowCoordinator>("_gameplaySetupViewController");
            gameplaySetupViewController.SetField("_showEnvironmentOverrideSettings", true);

            AddCustomTab();
            gameplaySetupViewController.GetType().GetMethod("RefreshContent", AccessTools.all)?.Invoke(gameplaySetupViewController, null);
        }

        private void InitializeLevelParamsPanel()
        {
            if (_levelParamsPanel != null)
            {
                Plugin.logger.Debug("level params panel not null");
                return;
            }

            Plugin.logger.Debug("initializing level params panel");
            _levelParamsPanel = GameObject.Instantiate(_levelParamsPanelBase, _missionLevelDetailViewController.transform.GetChild(0));
            _levelParamsPanel.transform.SetSiblingIndex(1);

            foreach (var hoverHint in _levelParamsPanel.GetComponentsInChildren<HoverHint>())
            {
                hoverHint.SetField("_hoverHintController", _hoverHintController);
            }
            foreach (var localizedHoverHint in _levelParamsPanel.GetComponentsInChildren<LocalizedHoverHint>())
            {
                GameObject.Destroy(localizedHoverHint);
            }

            var obstacles = _levelParamsPanel.transform.GetChild(2);
            var bombs = _levelParamsPanel.transform.GetChild(3);

            obstacles.Find("Icon").GetComponent<ImageView>().SetImage("#ClockIcon");
            bombs.Find("Icon").GetComponent<ImageView>().SetImage("#FastNotesIcon");
            obstacles.GetComponent<HoverHint>().text = "Song Length";
            bombs.GetComponent<HoverHint>().text = "Note Jump Speed";

            if (_objectivesSizeDelta.x == 0)
            {
                var objectives = _missionLevelDetailViewController.transform.GetChild(0).GetChild(2);
                _objectivesSizeDelta = (objectives.transform as RectTransform).sizeDelta;
            }

        }
        #endregion

        #region Mission-Specific UI
        public void MissionLevelSelected(Mission mission)
        {
            var missionData = mission.GetMissionData(null); // campaign doesn't matter here

            UpdateLevelParamsPanel();
            UpdateLeaderboards(true);

            var objectives = _missionLevelDetailViewController.transform.GetChild(0).GetChild(2);
            _levelParamsPanel.transform.localPosition = new Vector3(objectives.localPosition.x, _levelParamsPanel.transform.localPosition.y, _levelParamsPanel.transform.localPosition.z);
            _levelParamsPanel.transform.position = new Vector3(0.25f, _levelParamsPanel.transform.position.y, _levelParamsPanel.transform.position.z);
        }

        public void UpdateLeaderboards(bool fullRefresh)
        {
            Plugin.logger.Debug("updating leaderboards");
            if (_missionLevelDetailViewController.missionNode == null)
            {
                return;
            }

            CustomMissionDataSO missionData = _missionLevelDetailViewController.missionNode.missionData as CustomMissionDataSO;
            Mission mission = missionData.mission;
            CustomPreviewBeatmapLevel level = missionData.customLevel;

            bool showGlobalLeaderboard = mission.allowStandardLevel && level != null && Plugin.isScoreSaberInstalled;

            _settingsHandler.SetFloorLeaderboardSettingVisibility(showGlobalLeaderboard);
            if (showGlobalLeaderboard)
            {
                Plugin.logger.Debug("showing global leaderboard");

                IDifficultyBeatmap difficultyBeatmap = BeatmapUtils.GetMatchingBeatmapDifficulty(level.levelID, missionData.beatmapCharacteristic, mission.difficulty);
                if (difficultyBeatmap == null)
                {
                    Plugin.logger.Debug("couldn't find matching difficultybeatmap");
                }
                else
                {
                    _globalLeaderboardViewController.SetData(difficultyBeatmap);
                    if (_config.floorLeaderboard)
                    {
                        Plugin.logger.Debug("floor leaderboard");
                        _campaignMissionLeaderboardViewController.customURL = missionData.campaign.info.customMissionLeaderboard;
                        _campaignMissionLeaderboardViewController.mission = mission;
                        _campaignFlowCoordinator.InvokeMethod<object, CampaignFlowCoordinator>("SetRightScreenViewController", _campaignMissionLeaderboardViewController, ViewController.AnimationType.In);
                        _campaignMissionLeaderboardViewController.UpdateLeaderboards();
                        _campaignFlowCoordinator.InvokeMethod<object, CampaignFlowCoordinator>("SetBottomScreenViewController", _globalLeaderboardViewController, ViewController.AnimationType.None);

                        _globalLeaderboardViewController.transform.localPosition = _leaderboardPosition;
                    }
                    else
                    {
                        Plugin.logger.Debug("boring leaderboard");
                        _campaignFlowCoordinator.InvokeMethod<object, CampaignFlowCoordinator>("SetRightScreenViewController", _globalLeaderboardViewController, ViewController.AnimationType.In);

                        missionDataReadyEvent?.Invoke(mission, missionData.campaign.info.customMissionLeaderboard, GetNodeColor(_missionLevelDetailViewController.missionNode));
                        leaderboardUpdateEvent?.Invoke();
                    }
                }
            }
            else
            {
                Plugin.logger.Debug("not showing global leaderboard");
                _campaignMissionLeaderboardViewController.customURL = missionData.campaign.info.customMissionLeaderboard;
                _campaignMissionLeaderboardViewController.mission = mission;
                _campaignFlowCoordinator.InvokeMethod<object, CampaignFlowCoordinator>("SetRightScreenViewController", _campaignMissionLeaderboardViewController, ViewController.AnimationType.In);
                if (fullRefresh)
                {
                    _campaignMissionLeaderboardViewController.UpdateLeaderboards();
                }
            }
        }

        private async void UpdateLevelParamsPanel()
        {
            CustomMissionDataSO missionData = _missionLevelDetailViewController.missionNode.missionData as CustomMissionDataSO;
            Mission mission = missionData.mission;
            CustomPreviewBeatmapLevel level = missionData.customLevel;

            if (level == null)
            {
                SetLevelParamsTextNotAvailable();
            }
            else
            {
                IDifficultyBeatmap difficultyBeatmap = BeatmapUtils.GetMatchingBeatmapDifficulty(level.levelID, missionData.beatmapCharacteristic, mission.difficulty);
                if (difficultyBeatmap == null)
                {
                    Plugin.logger.Debug("couldn't find matching difficultybeatmap");
                    SetLevelParamsTextNotAvailable();
                    return;
                }

                var audioClip = SongCore.Loader.BeatmapLevelsModelSO.GetBeatmapLevelIfLoaded(level.levelID).beatmapLevelData.audioClip;

                CustomDifficultyBeatmap customDifficultyBeatmap = difficultyBeatmap as CustomDifficultyBeatmap;

                if (customDifficultyBeatmap == null)
                {
                    Plugin.logger.Error("difficulty beatmap was not a custom beatmap??????!111");
                    return;
                }

                IReadonlyBeatmapData beatmapData = null;
                await Task.Run(delegate ()
                {
                    beatmapData = BeatmapDataLoader.GetBeatmapDataFromSaveData(customDifficultyBeatmap.beatmapSaveData, customDifficultyBeatmap.difficulty, customDifficultyBeatmap.level.beatsPerMinute, false, null, null);
                });

                _levelParamsPanel.notesPerSecond = beatmapData.cuttableNotesCount / audioClip.length;
                _levelParamsPanel.notesCount = beatmapData.cuttableNotesCount;

                SetTime(audioClip);
                SetNJS(difficultyBeatmap.noteJumpMovementSpeed);
            }
        }

        private void SetTime(AudioClip audioClip)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(audioClip.length);
            _levelParamsPanel.GetField<TextMeshProUGUI, LevelParamsPanel>("_obstaclesCountText").text = timeSpan.ToString(@"mm\:ss");
        }

        private void SetNJS(float njs)
        {
            _levelParamsPanel.GetField<TextMeshProUGUI, LevelParamsPanel>("_bombsCountText").text = njs.ToString("F1");
        }

        private void SetLevelParamsTextNotAvailable()
        {
            _levelParamsPanel.GetField<TextMeshProUGUI, LevelParamsPanel>("_notesPerSecondText").text = "N/A";
            _levelParamsPanel.GetField<TextMeshProUGUI, LevelParamsPanel>("_notesCountText").text = "N/A";
            _levelParamsPanel.GetField<TextMeshProUGUI, LevelParamsPanel>("_obstaclesCountText").text = "N/A";
            _levelParamsPanel.GetField<TextMeshProUGUI, LevelParamsPanel>("_bombsCountText").text = "N/A";
        }
        #endregion

        public void SetPlayButtonText(string text)
        {
            Plugin.logger.Debug($"Setting play button text: {text}");
            _playButton.SetButtonText(text);
        }

        public void SetPlayButtonInteractable(bool interactable)
        {
            Plugin.logger.Debug($"Setting play button interactability: {interactable}");
            _playButton.interactable = interactable;
        }

        public void CampaignClosed()
        {
            // I honestly have no clue why this is necessary
            _mapScrollView.GetField<RectTransform, ScrollView>("_contentRectTransform").SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _baseMapHeight);

            foreach (MissionNode node in _currentCampaignNodes)
            {
                UnityEngine.Object.Destroy(node.gameObject);
            }
            foreach (MissionStage stage in _currentMissionStages)
            {
                UnityEngine.Object.Destroy(stage.gameObject);
            }

            _campaignFlowCoordinator.GetField<MenuLightsManager, CampaignFlowCoordinator>("_menuLightsManager").SetColorPreset(_baseDefaultLights, animated: true);
            _campaignFlowCoordinator.InvokeMethod<object, CampaignFlowCoordinator>("SetTitle", "Campaign", ViewController.AnimationType.None);
        }

        internal void BaseCampaignEnabled()
        {
            MissionName.gameObject.SetActive(false);

            _missionNodesManager.SetField("_rootMissionNode", _baseRootMissionNode);
            _missionNodesManager.SetField("_finalMissionNode", _baseFinalMissionNode);
            _missionNodesManager.SetField("_allMissionNodes", _baseMissionNodes);
            _missionStagesManager.SetField("_missionStages", _baseMissionStages);

            _backgroundImage.sprite = _baseBackground;
            _backgroundImage.color = new Color(1, 1, 1, _baseBackAlpha);
            _campaignFlowCoordinator.SetField("_defaultLightsPreset", _baseDefaultLights);
            _mapScrollView.GetField<RectTransform, ScrollView>("_contentRectTransform").SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _baseMapHeight);

            UnYeetBaseGameNodes();

            if (_levelParamsPanel != null)
            {
                Plugin.logger.Debug("destroying level params panel");
                GameObject.Destroy(_levelParamsPanel.gameObject);
            }

            InitializeCampaignUI();
            PanelViewShowPatch.ViewShown -= OnViewActivated;
            baseCampaignEnabledEvent?.Invoke();
            _gameplaySetupManager.CampaignExit();
            _inCustomCampaign = false;
        }

        internal void SetMissionName(string missionName)
        {
            MissionName.text = missionName;
        }

        public void RefreshMissionNodeData()
        {
            foreach (MissionNode node in _currentCampaignNodes)
            {
                CustomMissionDataSO customMissionData = node.missionData as CustomMissionDataSO;
                Mission mission = customMissionData.mission;
                node.SetField("_missionDataSO", mission.GetMissionData(customMissionData.campaign));
            }
        }

        internal void UpdateProgress(float progress)
        {
            SetupLevelBarForProgressReporting();
            _levelBarBackground.fillAmount = progress;
            _levelBarBackground.color = progress < 0.33f ? _lowProgressColor : progress < 0.67f ? _mediumProgressColor : _highProgressColor;
        }

        internal void ClearProgressBar()
        {
            ResetLevelBarBackground();
        }

        public void SetDefaultLights()
        {
            MenuLightsManager menuLightsManager = _campaignFlowCoordinator.GetField<MenuLightsManager, CampaignFlowCoordinator>("_menuLightsManager");
            MenuLightsPresetSO defaultLightsPreset = _campaignFlowCoordinator.GetField<MenuLightsPresetSO, CampaignFlowCoordinator>("_defaultLightsPreset");

            menuLightsManager.SetColorPreset(defaultLightsPreset, false);
        }

        public void CreateModifierParamsList(MissionNode missionNode)
        {
            Plugin.logger.Debug("creating modifier param list");
            Mission mission = (missionNode.missionData as CustomMissionDataSO).mission;
            List<GameplayModifierParamsSO> modifierParams = _gameplayModifiersModel.CreateModifierParamsList(missionNode.missionData.gameplayModifiers);

            foreach (string modName in mission.externalModifiers.Keys)
            {
                CreateExternalModifierParamsList(modName, ref modifierParams);
            }

            foreach (string modName in mission.optionalExternalModifiers.Keys)
            {
                CreateExternalModifierParamsList(modName, ref modifierParams);
            }

            foreach (UnlockableItem item in mission.unlockableItems)
            {
                modifierParams.Add(item.GetModifierParam());
            }

            if (mission.unlockMap)
            {
                modifierParams.Add(ModifierUtils.CreateUnlockableSongParam());
            }

            _modifierParams = modifierParams;
            LoadModifiersPanel();
        }

        public void LoadModifiersPanel()
        {
            _modifiersPanelGO.SetActive(_modifierParams.Count > 0);

            _gameplayModifierInfoListItemsList.SetData(_modifierParams.Count, delegate (int index, GameplayModifierInfoListItem gameplayModifierInfoListItem)
            {
                GameplayModifierParamsSO gameplayModifierParamsSO = _modifierParams[index];
                gameplayModifierInfoListItem.SetModifier(gameplayModifierParamsSO, true);
            });

            if (_modifierParams.Count > 0)
            {
                AdjustObjectiveModifierTransforms();
            }
        }

        public void LoadErrors(List<GameplayModifierParamsSO> modifierParams)
        {
            _modifiersPanelGO.SetActive(modifierParams.Count > 0);

            _gameplayModifierInfoListItemsList.SetData(modifierParams.Count, delegate (int index, GameplayModifierInfoListItem gameplayModifierInfoListItem)
            {
                GameplayModifierParamsSO gameplayModifierParamsSO = modifierParams[index];
                gameplayModifierInfoListItem.SetModifier(gameplayModifierParamsSO, true);
            });
        }

        private void AddCustomTab()
        {
            _gameplaySetupManager.Setup(CustomCampaignFlowCoordinator.CustomCampaignManager.Campaign);
        }

        private void OnViewActivated()
        {
            Plugin.logger.Debug("view activated");
            _globalLeaderboardViewController.transform.localPosition = _leaderboardPosition;
        }

        private void OnSettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "floorLeaderboard":
                    //if (_config.floorLeaderboard)
                    //{
                    //    _leaderboardNavigationViewController.FloorLeaderboardEnabled();
                    //}
                    //else
                    //{
                    //    _leaderboardNavigationViewController.FloorLeaderboardDisabled();
                    //}
                    //UpdateLeaderboards(false);
                    break;
                case "floorLeaderboardPosition":
                    _leaderboardPosition = new Vector3(0, _config.floorLeaderboardPosition, 0);
                    if (_config.floorLeaderboard)
                    {
                        _globalLeaderboardViewController.transform.localPosition = _leaderboardPosition;
                    }
                    break;
                default:
                    return;
            }
        }

        #region Helper Functions
        private Vector3 GetCampaignPosition(float x, float y, float mapHeight)
        {
            return new Vector3(x * EDITOR_TO_GAME_UNITS,
                                -_baseMapHeight / 2 + mapHeight * EDITOR_TO_GAME_UNITS + HEIGHT_OFFSET / 2 - y * EDITOR_TO_GAME_UNITS,
                                0);
        }

        private void ResetLevelBarBackground()
        {
            _levelBarBackground.color = _originalLevelBarBackgroundColor;
            _levelBarBackground.type = Image.Type.Sliced;
            _levelBarBackground.fillMethod = Image.FillMethod.Radial360;
            _levelBarBackground.fillAmount = 1;
        }

        private void SetupLevelBarForProgressReporting()
        {
            _levelBarBackground.type = Image.Type.Filled;
            _levelBarBackground.fillMethod = Image.FillMethod.Horizontal;
            _levelBarBackground.fillAmount = 0;
        }

        private void CreateExternalModifierParamsList(string modName, ref List<GameplayModifierParamsSO> modifierParams)
        {
            foreach (var externalModifier in ExternalModifierManager.ExternalModifiers.Values)
            {
                if (externalModifier.Name == modName)
                {
                    foreach (ExternalModifier.ExternalModifierInfo modInfo in externalModifier.Infos)
                    {
                        modifierParams.Add(ModifierUtils.CreateModifierParam(SpriteUtils.LoadSpriteFromExternalAssembly(externalModifier.ModifierType.Assembly, modInfo.Icon), modInfo.Name, modInfo.Description));
                    }

                    break;
                }
            }
        }

        private void AdjustObjectiveModifierTransforms()
        {
            var objectives = _missionLevelDetailViewController.transform.GetChild(0).GetChild(2);

            Vector2 sd = _objectivesSizeDelta;

            sd.x /= 2;
            (objectives.transform as RectTransform).SetProperty("sizeDelta", sd);
            var modifiers = _missionLevelDetailViewController.transform.GetChild(0).GetChild(3);
            (modifiers.transform as RectTransform).SetProperty("sizeDelta", sd);
            modifiers.transform.position = objectives.transform.position;
            modifiers.transform.localPosition = new Vector3(sd.x / 2, -1.5f, objectives.transform.localPosition.z);

            objectives.transform.localPosition = new Vector3(-(sd.x / 2), objectives.transform.localPosition.y, objectives.transform.localPosition.z);
        }

        private Color GetNodeColor(MissionNode missionNode)
        {
            var missionNodeVisualController = missionNode.GetField<MissionNodeVisualController, MissionNode>("_missionNodeVisualController");
            var missionToggle = missionNodeVisualController.GetField<MissionToggle, MissionNodeVisualController>("_missionToggle");

            return missionToggle.GetField<Color, MissionToggle>("_highlightColor");
        }
        #endregion

        #region Affinity Patches
        [AffinityPostfix]
        [AffinityPatch(typeof(MissionHelpViewController), "Setup")]
        private void MissionHelpViewControllerSetupPostfix(MissionHelpSO missionHelp, MissionHelpViewController __instance)
        {
            CustomMissionHelpSO customMissionHelp = missionHelp as CustomMissionHelpSO;
            if (customMissionHelp != null)
            {
                Transform content = __instance.transform.GetChild(0);
                InitializeMissionHelpContent(content, missionHelp as CustomMissionHelpSO);
            }

            else
            {
                __instance.transform.GetChild(0).GetComponentInChildren<CurvedTextMeshPro>().text = "NEW OBJECTIVE";
            }
        }

        private void InitializeMissionHelpContent(Transform content, CustomMissionHelpSO missionHelp)
        {
            MissionInfo missionInfo = missionHelp.missionInfo;

            CurvedTextMeshPro title = content.GetChild(0).GetChild(1).GetComponent<CurvedTextMeshPro>();

            GameObject.Destroy(title.GetComponent<LocalizedTextMeshProUGUI>());
            title.text = missionInfo.title;
            title.richText = true;
            Transform infoContainer = GameObject.Instantiate(content.GetChild(1), content);
            infoContainer.SetSiblingIndex(content.childCount - 2);
            infoContainer.gameObject.SetActive(true);
            if (_lastShownMissionHelp != null)
            {
                GameObject.Destroy(_lastShownMissionHelp.gameObject);
            }
            _lastShownMissionHelp = infoContainer;

            foreach (Transform child in infoContainer)
            {
                GameObject.Destroy(child.gameObject);
            }

            InitializeInfoSegments(content, infoContainer, missionHelp);
        }

        private void InitializeInfoSegments(Transform content, Transform infoContainer, CustomMissionHelpSO missionHelp)
        {
            Transform seperatorPrefab = content.GetChild(6).GetChild(1);
            Transform segmentPrefab = content.GetChild(1).GetChild(1);

            MissionInfo missionInfo = missionHelp.missionInfo;
            string imagePath = missionHelp.imagePath;

            foreach (InfoSegment infoSegment in missionInfo.segments)
            {
                Transform segment = GameObject.Instantiate(segmentPrefab, infoContainer);
                GameObject.Destroy(segment.GetComponentInChildren<LocalizedTextMeshProUGUI>());

                if (infoSegment.text == "")
                {
                    GameObject.Destroy(segment.GetComponentInChildren<CurvedTextMeshPro>().gameObject);
                }
                else
                {
                    segment.GetComponentInChildren<CurvedTextMeshPro>().text = infoSegment.text;
                }

                ImageView imageView = segment.GetComponentInChildren<ImageView>();
                if (infoSegment.imageName == "")
                {
                    GameObject.Destroy(imageView.gameObject);
                }
                else
                {
                    string spritePath = imagePath + infoSegment.imageName;
                    Plugin.logger.Debug(spritePath);
                    SetupImage(imageView, spritePath);
                }

                if (infoSegment.hasSeparator)
                {
                    GameObject.Instantiate(seperatorPrefab, infoContainer);
                }
            }
        }

        private async void SetupImage(ImageView imageView, string spritePath)
        {
            imageView.sprite = null;
            imageView.gradient = false;

            if (!_loadedSprites.ContainsKey(spritePath))
            {
                _loadedSprites[spritePath] = await SpriteUtils.LoadSpriteFromFile(spritePath, false);
            }

            imageView.sprite = _loadedSprites[spritePath];
            imageView.enabled = false;
            imageView.enabled = true;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(MissionSelectionNavigationController), "HandleMissionSelectionMapViewControllerDidSelectMissionLevel")]
        private bool MissionSelectionNavigationControllerHandleMissionSelectionMapViewControllerDidSelectMissionLevelPrefix(MissionSelectionNavigationController __instance, MissionNode _missionNode, MissionLevelDetailViewController ____missionLevelDetailViewController)
        {
            Plugin.logger.Debug("HandleMissionSelectionMapViewControllerDidSelectMissionLevel");
            if (_levelParamsPanel == null)
            {
                return true;
            }

            ____missionLevelDetailViewController.Setup(_missionNode);
            if (!____missionLevelDetailViewController.isInViewControllerHierarchy)
            {
                __instance.PushViewController(this._missionLevelDetailViewController, ChangePosition, false);
            }
            return false;
        }

        private void ChangePosition()
        {
            var objectives = _missionLevelDetailViewController.transform.GetChild(0).GetChild(2);
            _levelParamsPanel.transform.localPosition = new Vector3(objectives.transform.localPosition.x, _levelParamsPanel.transform.localPosition.y, _levelParamsPanel.transform.localPosition.z);
            _levelParamsPanel.transform.position = new Vector3(0.25f, _levelParamsPanel.transform.position.y, _levelParamsPanel.transform.position.z);

            if (_modifierParams != null && _modifierParams.Count > 0)
            {
                AdjustObjectiveModifierTransforms();
            }
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(MissionLevelDetailViewController), "RefreshContent")]
        private bool MissionLevelDetailViewControllerRefreshContentPrefix(MissionLevelDetailViewController __instance, MissionNode ____missionNode, LevelBar ____levelBar, ObjectiveListItemsList ____objectiveListItems,
                            GameplayModifiersModelSO ____gameplayModifiersModel, GameObject ____modifiersPanelGO, GameplayModifierInfoListItemsList ____gameplayModifierInfoListItemsList)
        {
            if (____missionNode.missionData is CustomMissionDataSO && _inCustomCampaign)
            {
                _missionData = ____missionNode.missionData as CustomMissionDataSO;
                CustomPreviewBeatmapLevel level = _missionData.customLevel;
                if (level == null)
                {
                    ____levelBar.GetField<TextMeshProUGUI, LevelBar>("_songNameText").text = "SONG NOT FOUND";
                    ____levelBar.GetField<TextMeshProUGUI, LevelBar>("_difficultyText").text = "SONG NOT FOUND";
                    ____levelBar.GetField<TextMeshProUGUI, LevelBar>("_authorNameText").text = "SONG NOT FOUND";
                    ____levelBar.GetField<ImageView, LevelBar>("_songArtworkImageView").sprite = SongCore.Loader.defaultCoverImage;
                }
                else
                {
                    ____levelBar.Setup(level, _missionData.beatmapCharacteristic, _missionData.beatmapDifficulty);
                }
                ChangePosition();

                MissionObjective[] missionObjectives = _missionData.missionObjectives;
                ____objectiveListItems.SetData((missionObjectives.Length == 0) ? 1 : missionObjectives.Length, OnItemFinish);

                CreateModifierParamsList(____missionNode);
                return false;
            }
            return true;
        }

        private void OnItemFinish(int idx, ObjectiveListItem objectiveListItem)
        {
            if (idx == 0 && _missionData.missionObjectives.Length == 0)
            {
                objectiveListItem.title = Localization.Get("CAMPAIGN_FINISH_LEVEL");
                objectiveListItem.conditionText = "";
                objectiveListItem.hideCondition = true;
            }
            else
            {
                MissionObjective missionObjective = _missionData.missionObjectives[idx];
                if (missionObjective.type.noConditionValue)
                {
                    objectiveListItem.title = missionObjective.type.objectiveNameLocalized.Replace(" ", "\n");
                    objectiveListItem.hideCondition = true;
                }
                else
                {
                    objectiveListItem.title = missionObjective.type.objectiveNameLocalized;
                    objectiveListItem.hideCondition = false;
                    ObjectiveValueFormatterSO objectiveValueFormater = missionObjective.type.objectiveValueFormater;
                    objectiveListItem.conditionText = $"{MissionDataExtensions.Name(missionObjective.referenceValueComparisonType)} {objectiveValueFormater.FormatValue(missionObjective.referenceValue)}";
                }
            }
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(MissionNodeConnection), "SetActive")]
        private bool MissionNodeConnectionSetActivePrefix(ref bool ____isActive, ref Image ____image,  bool animated)
        {
            if (_inCustomCampaign && !animated)
            {
                ____isActive = true;
                ____image.color = Color.white;
                return false;
            }
            return true;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(MissionNodeSelectionManager), "HandleNodeWasDisplayed")]
        private bool MissionNodeSelectionManagerHandleNodeWasDisplayedPrefix()
        {
            return !_inCustomCampaign;
        }
        #endregion
    }
}
