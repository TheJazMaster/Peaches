using Nickel;
using HarmonyLib;
using TheJazMaster.UnseenEffort.Actions;

namespace TheJazMaster.UnseenEffort.Features;

[HarmonyPatch]
public class SingleDamModManager(PDamMod mod)
{
    private static IModData ModData => ModEntry.Instance.Helper.ModData;
    internal const string OldDamageModifierKey = "OldDamageModifier";
    private readonly PDamMod mod = mod;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Ship), nameof(Ship.ModifyDamageDueToParts))]
    private static void Ship_ModifyDamageDueToParts_Postfix(Ship __instance, State s, Combat c, Part part, bool piercing = false)
    {   
        bool isSingleUse = ModData.TryGetModData<PDamMod>(part, OldDamageModifierKey, out var oldDamageModifier);

        c.QueueImmediate(new AUndoTempDamageModifier {
            oldDamageModifier = isSingleUse ? oldDamageModifier : null,
            part = part
        });
    }

    public static void SetData(Part part, bool single) {
        if (single) {
            ModData.SetModData(part, OldDamageModifierKey, part.damageModifier);
        } else {
            ModData.RemoveModData(part, OldDamageModifierKey);
        }
    }
}