using CustomCampaigns.Managers;
using Zenject;

namespace CustomCampaigns.Installers
{
    internal class AppInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ExternalModifierManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<CreditsManager>().AsSingle().NonLazy();
        }
    }
}
