using HarmonyLib;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeatSaberCustomCampaigns.Harmony_Patches
{
	[HarmonyPatch(typeof(ScenesTransitionSetupDataSO), "Init")]
	class ScenesTransitionSetupDataSOInit
    {
		[HarmonyReversePatch]
		public static void Init(object instance, SceneInfo[] scenes, SceneSetupData[] sceneSetupData)
        {
			var scenesTransitionSetupDataSO = (ScenesTransitionSetupDataSO)instance;
			scenesTransitionSetupDataSO.SetProperty("scenes", scenes);
			scenesTransitionSetupDataSO.SetProperty("sceneSetupDataArray", sceneSetupData);
		}

	}

	[HarmonyPatch(typeof(MissionLevelScenesTransitionSetupDataSO), "Init")]
    class MissionLevelScenesTransitionSetupDataSOInit
    {
        static bool Prefix(ref MissionLevelScenesTransitionSetupDataSO __instance,
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

			GameplaySetupViewController gameplaySetupViewController = Resources.FindObjectsOfTypeAll<GameplaySetupViewController>().FirstOrDefault();
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
			SceneInfo[] scenes = new SceneInfo[]
			{
				environmentInfo.sceneInfo,
				____missionGameplaySceneInfo,
				____gameCoreSceneInfo
			};
			SceneSetupData[] sceneSetupData = new SceneSetupData[]
			{
				new EnvironmentSceneSetupData(usingOverrideEnvironment),
				new MissionGameplaySceneSetupData(missionObjectives, playerSpecificSettings.autoRestart, level, difficultyBeatmap.difficulty, difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic, gameplayModifiers, backButtonText),
				new GameplayCoreSceneSetupData(difficultyBeatmap, previewBeatmapLevel, gameplayModifiers, playerSpecificSettings, null, false, environmentInfo, colorScheme),
				new GameCoreSceneSetupData()
			};
			ScenesTransitionSetupDataSOInit.Init(__instance, scenes, sceneSetupData);

			return false;
		}
    }
}
