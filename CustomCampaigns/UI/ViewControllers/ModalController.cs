using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using CustomCampaigns.Managers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;

namespace CustomCampaigns.UI.ViewControllers
{
    public class ModalController : INotifyPropertyChanged
    {
        private MissionSelectionMapViewController _missionSelectionMapViewController;

        private bool _parsedCancelDownloads;
        private bool _parsedMissingOptionalModWarning;
        private bool _parsedOptionalModFailureWarning;

        private string _optionalMod;

        public event PropertyChangedEventHandler PropertyChanged;

        public Action didForceCancelDownloads;

        public Action<string> didDisableOptionalWarnings;
        public Action didContinueMissingOptional;
        public Action didBackOutMissingOptional;

        public Action didContinueOptionalFailure;
        public Action didBackOutOptionalFailure;

        [UIParams]
        readonly BSMLParserParams parserParams = null;

        [UIComponent("missing-mod-text")]
        private TextMeshProUGUI missingModText;

        [UIComponent("failure-mod-text")]
        private TextMeshProUGUI failureModText;


        public ModalController(MissionSelectionMapViewController missionSelectionMapViewController)
        {
            _missionSelectionMapViewController = missionSelectionMapViewController;
        }

        private void ParseCancelDownloads()
        {
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "CustomCampaigns.UI.Views.cancel-downloads-modal.bsml"), _missionSelectionMapViewController.transform.Find("ScrollView").gameObject, this);
            _parsedCancelDownloads = true;
        }

        private void ParseMissingOptionalModWarning()
        {
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "CustomCampaigns.UI.Views.missing-optional-mod-warning.bsml"), _missionSelectionMapViewController.transform.Find("ScrollView").gameObject, this);
            _parsedMissingOptionalModWarning = true;
        }

        private void ParseOptionalModFailureWarning()
        {
            Plugin.logger.Debug("parsing optional mod failure");
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "CustomCampaigns.UI.Views.optional-mod-failure-warning.bsml"), _missionSelectionMapViewController.transform.Find("ScrollView").gameObject, this);
            _parsedOptionalModFailureWarning = true;
        }

        internal void ShowCancelDownloadConfirmation()
        {
            if (!_parsedCancelDownloads)
            {
                ParseCancelDownloads();
            }

            parserParams.EmitEvent("confirm-download-cancel");
        }

        internal void ShowMissingOptionalModWarning(string optionalMod)
        {
            _optionalMod = optionalMod;
            if (!_parsedMissingOptionalModWarning)
            {
                ParseMissingOptionalModWarning();
            }

            missingModText.text = $"You are missing the following optional mod: \n{optionalMod}";
            parserParams.EmitEvent("missing-optional-mod");
        }

        internal void ShowOptionalModFailureWarning(string optionalMod, string failureReason = "")
        {
            _optionalMod = optionalMod;
            if (!_parsedOptionalModFailureWarning)
            {
                ParseOptionalModFailureWarning();
            }

            Plugin.logger.Debug("emitting optional mod failure event...");
            failureModText.text = $"The following optional mod has failed to load: \n{optionalMod}";
            if (failureReason != "")
            {
                failureModText.text += $"\n({failureReason})";
            }
            parserParams.EmitEvent("optional-mod-failure");
        }

        [UIAction("forced-downloads-cancel")]
        public void ForcedDownloadsCancel()
        {
            didForceCancelDownloads?.Invoke();
        }

        [UIAction("continue-missing-optional-disable-warning")]
        public void ContinueMissingOptionalDisableWarning()
        {
            didDisableOptionalWarnings?.Invoke(_optionalMod);
            ContinueMissingOptional();
        }

        [UIAction("continue-missing-optional")]
        public void ContinueMissingOptional()
        {
            didContinueMissingOptional?.Invoke();
        }

        [UIAction("back-out-missing-optional")]
        public void BackOutMissingOptional()
        {
            didBackOutMissingOptional?.Invoke();
        }

        [UIAction("continue-optional-failure")]
        public void ContinueOptionalFailure()
        {
            didContinueOptionalFailure?.Invoke();
        }

        [UIAction("back-out-optional-failure")]
        public void BackOutOptionalFailure()
        {
            didBackOutOptionalFailure?.Invoke();
        }
    }
}
