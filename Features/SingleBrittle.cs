using System;
using System.Collections.Generic;
using System.Linq;
using Nanoray.Shrike;
using Nanoray.Shrike.Harmony;
using System.Threading.Tasks;
using Nickel;
using HarmonyLib;
using System.Reflection.Emit;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace TheJazMaster.Peaches.Features;
#nullable enable

public class SingleBrittleManager
{
    private static IModData ModData => ModEntry.Instance.Helper.ModData;
    internal const string OldDamageModifierKey = "OldDamageModifier";

    public SingleBrittleManager()
    {
        ModEntry.Instance.Harmony.TryPatch(
		    logger: ModEntry.Instance.Logger,
		    original: AccessTools.DeclaredMethod(typeof(Ship), nameof(Ship.ModifyDamageDueToParts)),
			postfix: new HarmonyMethod(GetType(), nameof(Ship_ModifyDamageDueToParts_Postfix))
		);
    }

    private static void Ship_ModifyDamageDueToParts_Postfix(Ship __instance, State s, Combat c, Part part, bool piercing = false)
    {
        
        bool isSingleUse = ModData.TryGetModData<PDamMod>(part, OldDamageModifierKey, out var oldDamageModifier);

        if (part.damageModifier == PDamMod.brittle && isSingleUse) {
            part.damageModifier = oldDamageModifier;
        }

        ModData.RemoveModData(part, OldDamageModifierKey);
    }

    public static void SetBrittleData(Part part, bool single) {
        if (single) {
            ModData.SetModData(part, OldDamageModifierKey, part.damageModifier);
        } else {
            ModData.RemoveModData(part, OldDamageModifierKey);
        }
    }
}