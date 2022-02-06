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

namespace CustomCampaigns.UI.ViewControllers
{
    public class ModalController : INotifyPropertyChanged
    {
        private MissionSelectionMapViewController _missionSelectionMapViewController;
        private bool _parsed;

        public event PropertyChangedEventHandler PropertyChanged;
        public Action didForceCancelDownloads;

        [UIParams]
        readonly BSMLParserParams parserParams = null;

        public ModalController(MissionSelectionMapViewController missionSelectionMapViewController)
        {
            _missionSelectionMapViewController = missionSelectionMapViewController;
        }

        private void Parse()
        {
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "CustomCampaigns.UI.Views.cancel-downloads-modal.bsml"), _missionSelectionMapViewController.transform.Find("ScrollView").gameObject, this);
        }

        internal void ShowCancelDownloadConfirmation()
        {
            if (!_parsed)
            {
                Parse();
            }

            parserParams.EmitEvent("confirm-download-cancel");
        }

        [UIAction("#post-parse")]
        private void Parsed()
        {
            _parsed = true;
        }

        [UIAction("forced-downloads-cancel")]
        public void ForcedDownloadsCancel()
        {
            didForceCancelDownloads?.Invoke();
        }
    }
}
