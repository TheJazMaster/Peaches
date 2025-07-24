using Nickel;
using HarmonyLib;
using TheJazMaster.UnseenEffort.Actions;

namespace TheJazMaster.UnseenEffort.Features;

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