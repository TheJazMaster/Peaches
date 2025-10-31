using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using TheJazMaster.UnseenEffort.Artifacts;

namespace TheJazMaster.UnseenEffort.Features;

[HarmonyPatch]
public class ArtifactInterfacesManager
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Card), nameof(Card.GetDataWithOverrides))]
    private static void Card_GetDataWithOverrides_Postfix(Card __instance, ref CardData __result, State state) {
        if (state == DB.fakeState)
            return;
        if (MG.inst.g.metaRoute is not null && MG.inst.g.metaRoute is { subRoute: Codex })
            return;
        
        foreach (Artifact item in state.EnumerateAllArtifacts()) {
            if (item is ICardDataAffectorArtifact artifact)  {
                artifact.AffectCardData(state, __instance, ref __result);
            }       
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Ship), nameof(Ship.Set))]
    private static void Ship_Set_Prefix(Ship __instance, Status status, int n, ref int __state) {
        if (status == Status.survive) {
            __state = __instance.Get(Status.survive);
        }
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Ship), nameof(Ship.Set))]
    private static void Ship_Set_Postfix(Ship __instance, Status status, int n, int __state) {
        if (__state > 0 && __instance.Get(Status.survive) < __state) {
            foreach (Artifact item in MG.inst.g.state.EnumerateAllArtifacts()) {
                if (item is IOnSurviveArtifact artifact) {
                    artifact.OnSurvive(MG.inst.g.state, __instance);
                }
            }
        }
    }
}