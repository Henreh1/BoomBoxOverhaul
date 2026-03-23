using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;

namespace BoomBoxOverhaul
{
    [HarmonyPatch(typeof(BoomboxItem))]
    public static class BoomboxItemPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void StartPostfix(BoomboxItem __instance, ref Item ___itemProperties)
        {
            try
            {
                if (Plugin.InfiniteBattery.Value)
                {
                    ___itemProperties.requiresBattery = false;
                }

                if (__instance.GetComponent<UnifiedBoomboxController>() == null)
                {
                    __instance.gameObject.AddComponent<UnifiedBoomboxController>();
                }
            }
            catch (Exception ex)
            {
                Plugin.Error("Boombox Start patch failed: " + ex);
            }
        }

        [HarmonyTranspiler]
        [HarmonyPatch("PocketItem")]
        public static IEnumerable<CodeInstruction> PocketItemTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list = new List<CodeInstruction>(instructions);

            try
            {
                int startIndex = -1;
                int endIndex = -1;
                int i;
                for (i = 0; i < list.Count; i++)
                {
                    if (list[i].opcode != OpCodes.Call)
                    {
                        continue;
                    }

                    string text = list[i].ToString();
                    if (text == null || text.IndexOf("BoomboxItem::StartMusic", StringComparison.Ordinal) < 0)
                    {
                        continue;
                    }

                    endIndex = i;

                    int j;
                    for (j = i; j >= 0; j--)
                    {
                        if (list[j].opcode == OpCodes.Ldarg_0)
                        {
                            startIndex = j;
                            break;
                        }
                    }

                    break;
                }

                if (startIndex > -1 && endIndex > -1 && endIndex >= startIndex)
                {
                    list.RemoveRange(startIndex, endIndex - startIndex + 1);
                    Plugin.Log("Patched BoomboxItem.PocketItem to preserve playback.");
                }
                else
                {
                    Plugin.Warn("PocketItem patch could not find BoomboxItem::StartMusic block.");
                }
            }
            catch (Exception ex)
            {
                Plugin.Error("PocketItem transpiler failed: " + ex);
            }

            return list.AsEnumerable();
        }
    }
}