using CustomCampaigns.Managers;
using IPA.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace CustomCampaigns.UI.MissionObjectiveGameUI
{
    public class MissionObjectiveGameUIViewPrefabFetcher : MonoBehaviour
    {
        public Action OnPrefabFetched;

        private const string SCENE_NAME = "MissionGameplay";

        public void FetchPrefab()
        {
            if (CustomCampaignManager.missionObjectiveGameUIViewPrefab != null)
            {
                OnPrefabFetched?.Invoke();
                return;
            }

            StartCoroutine(FetchPrefabCoroutine());
        }

        private IEnumerator FetchPrefabCoroutine()
        {
            AsyncOperation loadScene = SceneManager.LoadSceneAsync(SCENE_NAME, LoadSceneMode.Additive);
            while (!loadScene.isDone)
            {
                yield return null;
            }

            Scene missionGameplayScene = SceneManager.GetSceneByName(SCENE_NAME);
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
                    Plugin.logger.Debug("Found prefab");
                    MissionObjectiveGameUIView missionObjectiveGameUIViewPrefab = GameObject.Instantiate(missionObjectiveGameUIView);
                    missionObjectiveGameUIViewPrefab.gameObject.SetActive(false);
                    CustomCampaignManager.missionObjectiveGameUIViewPrefab = missionObjectiveGameUIViewPrefab;
                }
            }

            SceneManager.UnloadSceneAsync(SCENE_NAME); 
            OnPrefabFetched?.Invoke();
        }
    }
}
