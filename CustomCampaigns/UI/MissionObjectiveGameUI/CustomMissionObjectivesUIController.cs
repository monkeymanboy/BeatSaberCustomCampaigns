using CustomCampaigns.Managers;
using IPA.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace CustomCampaigns.UI.MissionObjectiveGameUI
{
    class CustomMissionObjectivesUIController : MonoBehaviour
    {
        private List<MissionObjectiveGameUIView> _missionObjectiveGameUIViews = new List<MissionObjectiveGameUIView>();

        private const float X_POS = 0f;
        private const float Y_POS = 4.3f;
        private const float Z_POS = 13f;
        private const float ELEMENT_WIDTH = 1f;
        private const float SEPARATOR = 1f;

        public void Start()
        {
            transform.position = new Vector3(X_POS, Y_POS, Z_POS);
            transform.localPosition = transform.position;
        }

        public void CreateUIElements(List<MissionObjectiveChecker> activeMissionObjectiveCheckers)
        {
            foreach (MissionObjectiveGameUIView missionObjectiveGameUIView in _missionObjectiveGameUIViews)
            {
                missionObjectiveGameUIView.GetField<MissionObjectiveChecker, MissionObjectiveGameUIView>("_missionObjectiveChecker").statusDidChangeEvent -= missionObjectiveGameUIView.HandleMissionObjectiveStatusDidChange;
                missionObjectiveGameUIView.GetField<MissionObjectiveChecker, MissionObjectiveGameUIView>("_missionObjectiveChecker").checkedValueDidChangeEvent -= missionObjectiveGameUIView.HandleMissionObjectiveCheckedValueDidChange;
                Object.Destroy(missionObjectiveGameUIView.gameObject);
            }
            _missionObjectiveGameUIViews.Clear();

            var count = activeMissionObjectiveCheckers.Count;
            var xpos = (ELEMENT_WIDTH - (ELEMENT_WIDTH * count + SEPARATOR * (count - 1))) * 0.5f;
            foreach (var missionObjectiveChecker in activeMissionObjectiveCheckers)
            {
                MissionObjectiveGameUIView missionObjectiveGameUIView = Object.Instantiate(CustomCampaignManager.missionObjectiveGameUIViewPrefab, transform, false);
                missionObjectiveGameUIView.transform.localPosition = new Vector2(xpos, 0f);
                missionObjectiveGameUIView.SetMissionObjectiveChecker(missionObjectiveChecker);
                missionObjectiveGameUIView.gameObject.SetActive(true);

                _missionObjectiveGameUIViews.Add(missionObjectiveGameUIView);
                xpos += SEPARATOR + ELEMENT_WIDTH;
            }
        }

        public void AddUIElement(MissionObjectiveChecker missionObjectiveChecker)
        {
            var count = _missionObjectiveGameUIViews.Count + 1;
            var xpos = (ELEMENT_WIDTH - (ELEMENT_WIDTH * count + SEPARATOR * (count - 1))) * 0.5f;

            foreach (var view in _missionObjectiveGameUIViews)
            {
                view.transform.localPosition = new Vector2(xpos, 0f);
                xpos += SEPARATOR + ELEMENT_WIDTH;
            }

            MissionObjectiveGameUIView missionObjectiveGameUIView = Object.Instantiate(CustomCampaignManager.missionObjectiveGameUIViewPrefab, transform, false);
            missionObjectiveGameUIView.transform.localPosition = new Vector2(xpos, 0f);
            missionObjectiveGameUIView.SetMissionObjectiveChecker(missionObjectiveChecker);
            missionObjectiveGameUIView.gameObject.SetActive(true);

            _missionObjectiveGameUIViews.Add(missionObjectiveGameUIView);
        }
    }
}
