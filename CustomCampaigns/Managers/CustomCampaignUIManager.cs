using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using CustomCampaigns.Campaign;
using CustomCampaigns.Campaign.Missions;
using CustomCampaigns.External;
using CustomCampaigns.UI.FlowCoordinators;
using CustomCampaigns.UI.ViewControllers;
using CustomCampaigns.Utils;
using HMUI;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomCampaigns.Managers
{
    public class CustomCampaignUIManager
    {
        public const float EDITOR_TO_GAME_UNITS = 30f / 111;
        public const float HEIGHT_OFFSET = 20;

        private const int YEET_AMOUNT = 10000;

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

        [UIComponent("mission-name")]
        public TextMeshProUGUI MissionName;

        private ImageView _levelBarBackground;
        private Color _originalLevelBarBackgroundColor;
        private Color _lowProgressColor;
        private Color _mediumProgressColor;
        private Color _highProgressColor;

        public CustomCampaignUIManager(CampaignFlowCoordinator campaignFlowCoordinator, MissionSelectionMapViewController missionSelectionMapViewController, MissionSelectionNavigationController missionSelectionNavigationController,
                                        MissionLevelDetailViewController missionLevelDetailViewController, MissionResultsViewController missionResultsViewController,
                                        CampaignMissionLeaderboardViewController campaignMissionLeaderboardViewController, PlatformLeaderboardViewController globalLeaderboardViewController)
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

            _campaignMissionLeaderboardViewController = campaignMissionLeaderboardViewController;
            _globalLeaderboardViewController = globalLeaderboardViewController;

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
                _missionNodesManager.Awake();
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

            YeetBaseGameNodes();
        }

        internal void SetupCampaignUI(Campaign.Campaign campaign)
        {
            SetCampaignBackground(campaign);
            SetCampaignLights(campaign);
            SetCampaignMissionNodes(campaign);

            _mapScrollView.GetField<RectTransform, ScrollView>("_contentRectTransform").SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, campaign.info.mapHeight * EDITOR_TO_GAME_UNITS + HEIGHT_OFFSET);

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

                missionToggle.RefreshUI();
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
            if (_missionNodeSelectionManager.GetField<MissionNode[], MissionNodeSelectionManager>("_missionNodes") == null)
            {
                _missionNodeSelectionManager.Start();
            }

            _missionNodeSelectionManager.OnDestroy();
            CustomCampaignFlowCoordinator.CustomCampaignManager.ResetProgressIds();

            _missionNodesManager.Awake();
            _missionNodeSelectionManager.Start();
            _missionConnectionsGenerator.CreateNodeConnections();
            _missionNodesManager.ResetAllNodes();
            _missionNodesManager.SetupNodeConnections();

            _mapScrollView.OnDestroy();
            _mapScrollView.Awake();

            _missionMapAnimationController.ScrollToTopMostNotClearedMission();
            _mapScrollViewItemsVisibilityController.Start();
        }

        private void SetupGameplaySetupViewController()
        {
            GameplaySetupViewController gameplaySetupViewController = _campaignFlowCoordinator.GetField<GameplaySetupViewController, CampaignFlowCoordinator>("_gameplaySetupViewController");
            gameplaySetupViewController.SetField("_showEnvironmentOverrideSettings", true);
            gameplaySetupViewController.RefreshContent();
        }
        #endregion

        #region Leaderboard UI
        public void MissionLevelSelected(Mission mission)
        {
            _campaignMissionLeaderboardViewController.mission = mission;

            var missionData = mission.GetMissionData(null); // campaign doesn't matter here
            _campaignFlowCoordinator.InvokeMethod<object, CampaignFlowCoordinator>("SetRightScreenViewController", _campaignMissionLeaderboardViewController, ViewController.AnimationType.In);
            _campaignMissionLeaderboardViewController.UpdateLeaderboards();

            UpdateLeaderboards(true);
        }

        public void UpdateLeaderboards(bool fullRefresh)
        {
            if (fullRefresh)
            {
                _campaignMissionLeaderboardViewController.UpdateLeaderboards();
            }

            CustomMissionDataSO missionData = _missionLevelDetailViewController.missionNode.missionData as CustomMissionDataSO as CustomMissionDataSO;
            Mission mission = missionData.mission;
            CustomPreviewBeatmapLevel level = missionData.customLevel;

            if (mission.allowStandardLevel && level != null)
            {
                IDifficultyBeatmap difficultyBeatmap = BeatmapUtils.GetMatchingBeatmapDifficulty(level.levelID, missionData.beatmapCharacteristic, mission.difficulty);
                if (difficultyBeatmap == null)
                {
                    Plugin.logger.Debug("couldn't find matching difficultybeatmap");
                }
                else
                {
                    _globalLeaderboardViewController.SetData(difficultyBeatmap);
                    _campaignFlowCoordinator.InvokeMethod<object, CampaignFlowCoordinator>("SetBottomScreenViewController", _globalLeaderboardViewController, ViewController.AnimationType.In);
                }
            }
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

            InitializeCampaignUI();
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
                Plugin.logger.Debug($"{modName}");
                foreach (var externalModifier in ExternalModifierManager.ExternalModifiers.Values)
                {
                    Plugin.logger.Debug($"{externalModifier.Name}");
                    if (externalModifier.Name == modName)
                    {
                        foreach (ExternalModifier.ExternalModifierInfo modInfo in externalModifier.Infos)
                        {
                            Plugin.logger.Debug($"found mod {modName}");
                            modifierParams.Add(ModifierUtils.CreateModifierParam(SpriteUtils.LoadSpriteFromExternalAssembly(externalModifier.ModifierType.Assembly, modInfo.Icon), modInfo.Name, modInfo.Description));
                        }
                            
                        break;
                    }
                }
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
        #endregion
    }
}
