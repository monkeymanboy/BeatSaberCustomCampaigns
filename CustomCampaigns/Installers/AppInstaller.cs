using CustomCampaigns.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace CustomCampaigns.Installers
{
    internal class AppInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ExternalModifierManager>().AsSingle().NonLazy();
        }
    }
}
