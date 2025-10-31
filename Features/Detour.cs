using HarmonyLib;
using Microsoft.Extensions.Logging;
using Nickel;
using static Shockah.Kokoro.IKokoroApi.IV2.IStatusRenderingApi;
using static Shockah.Kokoro.IKokoroApi.IV2.IStatusRenderingApi.IHook;

namespace TheJazMaster.UnseenEffort.Features;

[HarmonyPatch]
public class DetourManager : IHook
{
    public DetourManager() {
        ModEntry.Instance.KokoroApi.StatusRendering.RegisterHook(this);
    }

    private static IModData ModData => ModEntry.Instance.Helper.ModData;

    internal static readonly string DetourKey = "Detour";

    enum DetourType {
        None, Line, Anywhere
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MapBase), nameof(MapBase.CanGoHere))]
    public static void MapBase_CanGoHere_Postfix(Vec key, MapBase __instance, ref bool __result) {
        if (__result) return;

        var detour = ModData.GetModDataOrDefault(__instance, DetourKey, DetourType.None);
        switch (detour) {
            case DetourType.Line:
                __result = key.y == __instance.currentLocation.y + 1;
                return;
            case DetourType.Anywhere:
                __result = __instance.markers.TryGetValue(key, out var marker) && !marker.wasVisited;
                return;
            default: return;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MapRoute), nameof(MapRoute.OnClickDestination))]
    private static bool MapRoute_OnClickDestination_Prefix(G g, Vec key, bool force)
	{
		if (force)
			return true;
		if (!g.state.map.markers.TryGetValue(key, out var marker))
			return true;
		if (marker is { wasVisited: false, wasCleared: false })
			return true;

		g.state.map.currentLocation = key;
		return false;
	}

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MapBase), nameof(MapBase.MakeRoute))]
    public static void MapBase_MakeRoute_Prefix(State s, Vec key, MapBase __instance) {
        ModData.RemoveModData(__instance, DetourKey);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Combat), nameof(Combat.PlayerWon))]
    public static void Combat_PlayerWon_Prefix(G g, Combat __instance) {
        Ship ship = g.state.ship;
        if (ship.Get(ModEntry.Instance.DetourPlusStatus) > 0) {
            ModData.SetModData(g.state.map, DetourKey, DetourType.Anywhere);
        } 
        else if (ship.Get(ModEntry.Instance.DetourStatus) > 0) {
            ModData.SetModData(g.state.map, DetourKey, DetourType.Line);
        }
    }


    public IStatusInfoRenderer? OverrideStatusInfoRenderer(IOverrideStatusInfoRendererArgs args) {
        if (args.Status != ModEntry.Instance.DetourStatus && args.Status != ModEntry.Instance.DetourPlusStatus)
            return null;
        return ModEntry.Instance.KokoroApi.StatusRendering.MakeBarStatusInfoRenderer().SetSegmentWidth(0);
    }

    public bool? ShouldShowStatus(IShouldShowStatusArgs args) {
        if (args.Status != ModEntry.Instance.DetourStatus)
            return null;
        if (args.Ship.Get(ModEntry.Instance.DetourPlusStatus) > 0) return false;
        return null;
    }
}