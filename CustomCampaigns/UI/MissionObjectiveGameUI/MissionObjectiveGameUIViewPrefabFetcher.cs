using CustomCampaigns.Managers;
using IPA.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Zenject;

namespace CustomCampaigns.UI.MissionObjectiveGameUI
{
    public class MissionObjectiveGameUIViewPrefabFetcher : MonoBehaviour
    {
        public Action OnPrefabFetched;

        public void FetchPrefab(string SceneName, ZenjectSceneLoader zenjectSceneLoader)
        {
            if (CustomCampaignManager.missionObjectiveGameUIViewPrefab != null)
            {
                OnPrefabFetched?.Invoke();
                return;
            }

            StartCoroutine(FetchPrefabCoroutine(SceneName, zenjectSceneLoader));
        }

        private IEnumerator FetchPrefabCoroutine(string SceneName, ZenjectSceneLoader zenjectSceneLoader)
        {
            AsyncOperationHandle<SceneInstance> asyncOperationHandle = Addressables.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
            yield return asyncOperationHandle;

            Scene missionGameplayScene = SceneManager.GetSceneByName(SceneName);
            List<GameObject> gameObjects = new List<GameObject>(0);
            GameObject wrapper = null;
            missionGameplayScene.GetRootGameObjects(gameObjects);

            foreach (var gameObject in gameObjects)
            {
                if (gameObject.name == "Wrapper")
                {
                    wrapper = gameObject;
                    break;
                }
            }

            if (wrapper == null)
            {
                Plugin.logger.Error("Couldn't find prefab - couldn't find wrapper GO");
            }
            else
            {
                MissionObjectivesGameUIController missionObjectivesGameUIController = wrapper.GetComponentInChildren<MissionObjectivesGameUIController>();
                if (missionObjectivesGameUIController == null)
                {
                    Plugin.logger.Error("Couldn't find prefab - couldn't get missionObjectivesGameUIController");
                }
                else
                {
                    MissionObjectiveGameUIView missionObjectiveGameUIView = missionObjectivesGameUIController.GetField<MissionObjectiveGameUIView, MissionObjectivesGameUIController>("_missionObjectiveGameUIViewPrefab");
                    MissionObjectiveGameUIView missionObjectiveGameUIViewPrefab = GameObject.Instantiate(missionObjectiveGameUIView);
                    missionObjectiveGameUIViewPrefab.gameObject.SetActive(false);
                    CustomCampaignManager.missionObjectiveGameUIViewPrefab = missionObjectiveGameUIViewPrefab;
                }
            }

            SceneManager.UnloadSceneAsync(SceneName);
            OnPrefabFetched?.Invoke();
        }
    }
}
