using BeatSaberMarkupLanguage;
using LeaderboardCore.Models.UI.ViewControllers;
using System.Reflection;
using UnityEngine;
using Zenject;

namespace CustomCampaigns.UI.ViewControllers
{
    public class CampaignPanelViewController : BasicPanelViewController, IInitializable
    {
        protected override string LogoSource => "#CampaignIcon";

        protected string ExtraResourceName { get => "CustomCampaigns.UI.Views.mission-panel.bsml"; }
        protected override string customBSML { get => Utilities.GetResourceContent(Assembly.GetAssembly(typeof(CampaignPanelViewController)), ExtraResourceName); }

        protected override object customHost => this;

        public void Initialize()
        {
            isLoaded = true;
        }

        internal void SetColor(Color color)
        {
            backgroundColor = color;
        }
    }
}
