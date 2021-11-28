using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FrooxEngine;
using HarmonyLib;
using NeosModLoader;

namespace NeosDesktopToolShortcutRemapper
{
    public class NeosDesktopToolShortcutRemapper : NeosMod
    {
        public override string Name => "NeosDesktopToolShortcutRemapper";
        public override string Author => "runtime";
        public override string Version => "1.0.0";
        public override string Link => "https://github.com/zkxs/NeosDesktopToolShortcutRemapper";

        private static Dictionary<Uri, Uri> toolRemapping = new Dictionary<Uri, Uri>();

        static NeosDesktopToolShortcutRemapper() {
            // rebind 5 to component clone
            toolRemapping.Add(
                new Uri("neosrec:///G-Neos/Inventory/SpawnObjects/ShortcutTooltips/ShapeTip"), 
                new Uri("neosrec:///G-Neos/Inventory/Essential Tools/ComponentCloneTip")
            );
        }

        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony("net.michaelripley.NeosDesktopToolShortcutRemapper");
            MethodInfo original = AccessTools.DeclaredMethod(typeof(CommonTool), nameof(CommonTool.SpawnAndEquip));
            MethodInfo prefix = AccessTools.DeclaredMethod(typeof(NeosDesktopToolShortcutRemapper), nameof(NeosDesktopToolShortcutRemapper.SpawnAndEquipPrefix));
            harmony.Patch(original, prefix: new HarmonyMethod(prefix));
            Msg("Patched successfully");
        }

        private static void SpawnAndEquipPrefix(ref Uri uri)
        {
            if (toolRemapping.TryGetValue(uri, out Uri remappedUri))
            {
                Debug($"converted \"{uri}\" into \"{remappedUri}\"");
                uri = remappedUri;
            }
        }
    }
}
