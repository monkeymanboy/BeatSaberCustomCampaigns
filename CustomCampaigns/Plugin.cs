﻿using IPA;
using HarmonyLib;
using SiraUtil;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;
using IPA.Loader;

namespace CustomCampaigns
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        public static Hive.Versioning.Version version;
        public static IPALogger logger;

        private readonly Harmony _harmony;
        private const string _harmonyID = "dev.PulseLane.BeatSaber.CustomCampaigns";

        // TODO Generate CC folder if it doesn't exist
        [Init]
        public Plugin(IPALogger logger, Zenjector zenjector, PluginMetadata metadata)
        {
            version = metadata.HVersion;
            Plugin.logger = logger;
            _harmony = new Harmony(_harmonyID);

            zenjector.On<PCAppInit>().Pseudo(Container => Container.BindLoggerAsSiraLogger(logger));
            zenjector.OnMenu<Installers.MenuInstaller>();
            zenjector.OnGame<Installers.CampaignInstaller>().ShortCircuitForMultiplayer().ShortCircuitForTutorial().ShortCircuitForStandard();
            zenjector.OnGame<Installers.StandardLevelInstaller>().ShortCircuitForMultiplayer().ShortCircuitForTutorial().ShortCircuitForCampaign();
        }

        [OnEnable]
        public void OnEnable()
        {
            _harmony.PatchAll();
        }

        [OnDisable]
        public void OnDisable()
        {
            _harmony.UnpatchAll(_harmonyID);
        }
    }
}