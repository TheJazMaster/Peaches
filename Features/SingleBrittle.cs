using Nickel;
using HarmonyLib;

namespace TheJazMaster.Peaches.Features;
#nullable enable

public class SingleDamModManager
{
    private static IModData ModData => ModEntry.Instance.Helper.ModData;
    internal const string OldDamageModifierKey = "OldDamageModifier";
    private readonly PDamMod mod;

    public SingleDamModManager(PDamMod mod)
    {
        this.mod = mod;
        ModEntry.Instance.Harmony.TryPatch(
		    logger: ModEntry.Instance.Logger,
		    original: AccessTools.DeclaredMethod(typeof(Ship), nameof(Ship.ModifyDamageDueToParts)),
			postfix: new HarmonyMethod(GetType(), nameof(Ship_ModifyDamageDueToParts_Postfix))
		);
    }

    private static void Ship_ModifyDamageDueToParts_Postfix(Ship __instance, State s, Combat c, Part part, bool piercing = false)
    {
        
        bool isSingleUse = ModData.TryGetModData<PDamMod>(part, OldDamageModifierKey, out var oldDamageModifier);

        if ((part.damageModifier == PDamMod.weak || part.damageModifier == PDamMod.brittle) && isSingleUse) {
            part.damageModifier = oldDamageModifier;
        }

        ModData.RemoveModData(part, OldDamageModifierKey);
    }

    public static void SetData(Part part, bool single) {
        if (single) {
            ModData.SetModData(part, OldDamageModifierKey, part.damageModifier);
        } else {
            ModData.RemoveModData(part, OldDamageModifierKey);
        }
    }
}