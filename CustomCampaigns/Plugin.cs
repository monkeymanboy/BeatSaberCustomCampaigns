using CustomCampaigns.HarmonyPatches.ScoreSaber;
using CustomCampaigns.Utils;
using HarmonyLib;
using IPA;
using IPA.Config.Stores;
using IPA.Loader;
using SiraUtil;
using SiraUtil.Zenject;
using System;
using System.Linq;
using System.Reflection;
using IPALogger = IPA.Logging.Logger;

namespace CustomCampaigns
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        public static Hive.Versioning.Version version;
        public static IPALogger logger;
        internal static Config config;

        internal static bool isScoreSaberInstalled;

        private readonly Harmony _harmony;
        private const string _harmonyID = "dev.PulseLane.BeatSaber.CustomCampaigns";

       

        [Init]
        public Plugin(IPALogger logger, IPA.Config.Config conf, Zenjector zenjector, PluginMetadata metadata)
        {
            config = conf.Generated<Config>();

            version = metadata.HVersion;
            Plugin.logger = logger;
            _harmony = new Harmony(_harmonyID);

            zenjector.On<PCAppInit>().Pseudo(Container =>
                                    {
                                        Container.BindLoggerAsSiraLogger(logger);
                                        Container.BindInstance(config).AsSingle();
                                    });
            zenjector.OnApp<Installers.AppInstaller>();
            zenjector.OnMenu<Installers.MenuInstaller>();
            zenjector.OnGame<Installers.CampaignInstaller>().ShortCircuitForMultiplayer().ShortCircuitForTutorial().ShortCircuitForStandard();
            zenjector.OnGame<Installers.StandardLevelInstaller>().ShortCircuitForMultiplayer().ShortCircuitForTutorial().ShortCircuitForCampaign();
        }

        [OnEnable]
        public void OnEnable()
        {
            _harmony.PatchAll();
            BSUtilsUtils.CheckForBSUtilsInstall();
            PatchScoreSaber();
        }

        [OnDisable]
        public void OnDisable()
        {
            _harmony.UnpatchAll(_harmonyID);
        }

        private void PatchScoreSaber()
        {
            if (IsScoreSaberInstalled())
            {
                PluginMetadata metadata = PluginManager.GetPluginFromId("ScoreSaber");

                // PanelViewShow
                var originalPanelViewShow = metadata.Assembly.GetType("ScoreSaber.UI.ViewControllers.PanelView").GetMethod("Show", (BindingFlags) (-1));
                HarmonyMethod harmonyPanelViewShow = new HarmonyMethod(typeof(PanelViewShowPatch).GetMethod("Postfix", (BindingFlags) (-1)));
                _harmony.Patch(originalPanelViewShow, harmonyPanelViewShow);

                // PanelViewSetIsLoaded
                var originalPanelSetIsLoaded = metadata.Assembly.GetType("ScoreSaber.UI.ViewControllers.PanelView").GetMethod("set_isLoaded", (BindingFlags) (-1));
                HarmonyMethod harmonyPanelViewSetIsLoaded = new HarmonyMethod(typeof(PanelViewsIsLoadedSetterPatch).GetMethod("Postfix", (BindingFlags) (-1)));
                _harmony.Patch(originalPanelViewShow, harmonyPanelViewShow);
            }
        }

        private bool IsScoreSaberInstalled()
        {
            Plugin.logger.Debug("checking for ss install");
            try
            {
                var metadatas = PluginManager.EnabledPlugins.Where(x => x.Id == "ScoreSaber");
                isScoreSaberInstalled = metadatas.Count() > 0;
            }
            catch (Exception e)
            {
                logger.Debug($"Error checking for SS install: {e.Message}");
                isScoreSaberInstalled = false;
            }
            return isScoreSaberInstalled;
        }

        // TODO: Conditional harmony patch for SS install
    }
}