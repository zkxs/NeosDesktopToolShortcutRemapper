using FrooxEngine;
using HarmonyLib;
using NeosModLoader;
using System;
using System.Collections.Generic;

namespace NeosDesktopToolShortcutRemapper
{
    public class DesktopToolShortcutRemapper : NeosMod
    {
        internal const string VERSION = "2.0.0";

        public override string Name => "DesktopToolShortcutRemapper";
        public override string Author => "runtime";
        public override string Version => VERSION;
        public override string Link => "https://github.com/zkxs/NeosDesktopToolShortcutRemapper";

        // map of original tool Uris to remappings
        private static readonly Dictionary<Uri, ToolRemapping> toolRemappings = BuildUriToKeyMap();
        private static ModConfiguration? config = null;

        public override void OnEngineInit()
        {
            config = GetConfiguration();
            Harmony harmony = new Harmony("net.michaelripley.NeosDesktopToolShortcutRemapper");
            harmony.PatchAll();
        }

        public override void DefineConfiguration(ModConfigurationDefinitionBuilder builder)
        {
            builder
                .Version(new Version(1, 0, 0))
                .AutoSave(false);

            foreach (ToolRemapping toolRemapping in toolRemappings.Values)
            {
                builder.Key(toolRemapping.Enabled);
                builder.Key(toolRemapping.Uri);
            }
        }

        private static Dictionary<Uri, ToolRemapping> BuildUriToKeyMap()
        {
            Dictionary<Uri, ToolRemapping> map = new(9);
            map.AddTool(2, "DevToolTip");
            map.AddTool(3, "LogixTip");
            map.AddTool(4, "MaterialTip");
            map.AddTool(5, "ShapeTip");
            map.AddTool(6, "LightTip");
            map.AddTool(7, "GrabbableSetterTip");
            map.AddTool(8, "CharacterColliderSetterTip");
            map.AddTool(9, "Microphone");
            map.AddTool(0, "GlueTip");
            return map;
        }

        [HarmonyPatch]
        private static class HarmonyPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(CommonTool), nameof(CommonTool.SpawnAndEquip))]
            private static void SpawnAndEquipPrefix(ref Uri uri)
            {
                // check if the Uri is one that we're remapping (e.g. the default tool Uris)
                if (toolRemappings.TryGetValue(uri, out ToolRemapping toolRemapping))
                {
                    // make sure we're enabled
                    if (config!.GetValue(toolRemapping.Enabled))
                    {
                        Uri remappedUri = config!.GetValue(toolRemapping.Uri)!;
                        Debug($"converted \"{uri}\" into \"{remappedUri}\"");
                        uri = remappedUri;
                    }
                }
            }
        }
    }

    internal class ToolRemapping
    {
        public ModConfigurationKey<bool> Enabled { get; private set; }
        public ModConfigurationKey<Uri> Uri { get; private set; }

        public ToolRemapping(int key)
        {
            Enabled = new(
                name: $"Key{key}/enable",
                description: $"Enable custom {key} key",
                computeDefault: () => false);
            Uri = new(
                name: $"Key{key}/Uri",
                description: $"Tool record URI for {key} key",
                computeDefault: () => new Uri("neosrec:///G-Neos/Inventory/Essential Tools/ComponentCloneTip"),
                valueValidator: uri => uri != null);
        }
    }

    internal static class MethodExtensions
    {
        internal static void AddTool(this Dictionary<Uri, ToolRemapping> map, int key, string toolName)
        {
            map.Add(BuildUriForTool(toolName), new ToolRemapping(key));
        }

        private static Uri BuildUriForTool(string toolName)
        {
            return new Uri("neosrec:///G-Neos/Inventory/SpawnObjects/ShortcutTooltips/" + toolName);
        }
    }
}
