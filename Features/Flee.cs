using Nickel;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using Nanoray.Shrike;
using Nanoray.Shrike.Harmony;

namespace TheJazMaster.UnseenEffort.Features;

[HarmonyPatch]
public class FleeManager
{
    static ModEntry Instance => ModEntry.Instance;

    private static IModData ModData => ModEntry.Instance.Helper.ModData;

    internal static readonly string KeepTempKey = "KeepTemp";
    internal static readonly string LoseRewardsKey = "LoseRewards";

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Combat), nameof(Combat.CheckDeath))]
    private static bool Combat_CheckDeath_Prefix(G g, Combat __instance) {
        State s = g.state;
        if (s.ship.escapeTimer > 1)
		{
            s.ship.escapeTimer = 0;

            if (ModData.GetModDataOrDefault(__instance, LoseRewardsKey, false))
                __instance.noReward = true;

            __instance.PlayerWon(g);

            return false;
        }
        return true;
    }

    internal static bool ShouldKeepTempCards(Combat c) =>
        ModData.GetModDataOrDefault(c, KeepTempKey, false);


    [HarmonyPrefix]
    [HarmonyPatch(typeof(CardBrowse), nameof(CardBrowse.GetMergedDeckForDisplay))]
    private static void CardBrowse_GetMergedDeckForDisplay_Prefix(G g, ref bool includeTemporaryCards) {
        if (g.state.route is not Combat) includeTemporaryCards = true;
    }
}