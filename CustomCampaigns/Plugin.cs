using CustomCampaigns.HarmonyPatches.ScoreSaber;
using CustomCampaigns.Utils;
using HarmonyLib;
using IPA;
using IPA.Config.Stores;
using IPA.Loader;
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
        internal static bool isPlaylistLibInstalled;
        internal static bool isLeaderboardCoreInstalled;

        private readonly Harmony _harmony;
        private const string _harmonyID = "dev.PulseLane.BeatSaber.CustomCampaigns";

        [Init]
        public Plugin(IPALogger logger, IPA.Config.Config conf, Zenjector zenjector, PluginMetadata metadata)
        {
            config = conf.Generated<Config>();

            version = metadata.HVersion;
            Plugin.logger = logger;
            _harmony = new Harmony(_harmonyID);

            zenjector.Install<Installers.AppInstaller>(Location.App);
            zenjector.Install<Installers.MenuInstaller>(Location.Menu, config);
            zenjector.Install<Installers.CampaignInstaller>(Location.CampaignPlayer, config);
            zenjector.Install<Installers.StandardLevelInstaller>(Location.StandardPlayer, config);
        }

        [OnEnable]
        public void OnEnable()
        {
            _harmony.PatchAll();
            BSUtilsUtils.CheckForBSUtilsInstall();
            PatchScoreSaber();
            CheckForPlaylistsLibInstall();
            CheckForLeaderboardCoreInstall();
        }

        [OnDisable]
        public void OnDisable()
        {
            _harmony.UnpatchSelf();
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

        private bool CheckForPlaylistsLibInstall()
        {
            Plugin.logger.Debug("checking for playlists lib install");
            try
            {
                var metadatas = PluginManager.EnabledPlugins.Where(x => x.Id == "BeatSaberPlaylistsLib");
                isPlaylistLibInstalled = metadatas.Count() > 0;
            }
            catch (Exception e)
            {
                logger.Debug($"Error checking for playlistslib install: {e.Message}");
                isPlaylistLibInstalled = false;
            }
            return isPlaylistLibInstalled;
        }

        private bool CheckForLeaderboardCoreInstall()
        {
            Plugin.logger.Debug("checking for leaderboard core install");
            try
            {
                var metadatas = PluginManager.EnabledPlugins.Where(x => x.Id == "LeaderboardCore");
                isLeaderboardCoreInstalled = metadatas.Count() > 0;
            }
            catch (Exception e)
            {
                logger.Debug($"Error checking for leaderboardcore install: {e.Message}");
                isLeaderboardCoreInstalled = false;
            }
            return isLeaderboardCoreInstalled;
        }
    }
}