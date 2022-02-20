using CustomCampaigns.Managers;
using IPA.Loader;
using IPA.Loader.Features;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace CustomCampaigns.External
{
    public class ExternalModifierFeature : Feature
    {
        private Dictionary<PluginMetadata, ExternalModifier> externalModifiers = new Dictionary<PluginMetadata, ExternalModifier>();

        protected override bool Initialize(PluginMetadata meta, JObject featureData)
        {
            try
            {
                ExternalModifier externalModifier = featureData.ToObject<ExternalModifier>();
                externalModifiers.Add(meta, externalModifier);
                return true;
            }
            catch (Exception e)
            {
                InvalidMessage = $"Invalid modifier: {e.Message}";
                return false;
            }
        }

        public override void AfterInit(PluginMetadata meta)
        {
            Plugin.logger.Debug("????");
            if (externalModifiers.TryGetValue(meta, out ExternalModifier externalModifier))
            {
                if (externalModifier.HandlerLocation != null && !TryLoadType(ref externalModifier.HandlerType, meta, externalModifier.HandlerLocation))
                {
                    Plugin.logger.Error($"Failed to load a Type from the provided loader location for {externalModifier.Name}");
                    return;
                }

                if (externalModifier.ModifierLocation != null && !TryLoadType(ref externalModifier.ModifierType, meta, externalModifier.ModifierLocation))
                {
                    Plugin.logger.Error($"Failed to load a Type from the provided modifier location for {externalModifier.Name}");
                    return;
                }

                ExternalModifierManager.ExternalModifiers.Add(meta.Assembly, externalModifier);
                Plugin.logger.Debug($"Loaded external modifier: {externalModifier.Name} of type {externalModifier.ModifierType}");
            }

            else
            {
                Plugin.logger.Critical("monkaW");
            }
        }

        private bool TryLoadType(ref Type typeToLoad, PluginMetadata meta, string location)
        {
            // totally didn't yoink this from Counter+'s CustomCounterFeature which totally didn't yoink this from BSIPA's ConfigProviderFeature
            try
            {
                typeToLoad = meta.Assembly.GetType(location);
            }
            catch (ArgumentException)
            {
                InvalidMessage = $"Invalid type name {location}";
                return false;
            }
            catch (Exception e) when (e is FileNotFoundException || e is FileLoadException || e is BadImageFormatException)
            {
                string filename;

                switch (e)
                {
                    case FileNotFoundException fn:
                        filename = fn.FileName;
                        goto hasFilename;
                    case FileLoadException fl:
                        filename = fl.FileName;
                        goto hasFilename;
                    case BadImageFormatException bi:
                        filename = bi.FileName;
                    hasFilename:
                        InvalidMessage = $"Could not find {filename} while loading type";
                        break;
                    default:
                        InvalidMessage = $"Error while loading type: {e}";
                        break;
                }

                return false;
            }

            return true;
        }
    }
}
