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
using Microsoft.Xna.Framework.Input.Touch;
using System.Globalization;

namespace TheJazMaster.UnseenEffort.Features;

public class TiringManager
{
    static ModEntry Instance => ModEntry.Instance;
    static IModHelper Helper => Instance.Helper;
    static IModData ModData => Helper.ModData;
    static IModCards Cards => Helper.Content.Cards;

    internal static readonly string TiringCostKey = "TiringCost";


    internal static ICardTraitEntry TiringTrait { get; private set; } = null!;

    public TiringManager()
    {
        Spr TiringIcon = Instance.Helper.Content.Sprites.RegisterSprite(Instance.Package.PackageRoot.GetRelativeFile("Sprites/icons/Tiring.png")).Sprite;

        TiringTrait = ModEntry.Instance.Helper.Content.Cards.RegisterTrait("Tiring", new() {
            Icon = (state, card) => TiringIcon,
            Name = ModEntry.Instance.AnyLocalizations.Bind(["trait", "tiring"]).Localize,
            Tooltips = (state, card) => [
                new GlossaryTooltip($"trait.{GetType().Namespace!}::Tiring") {
                    Icon = TiringIcon,
                    TitleColor = Colors.cardtrait,
                    Title = ModEntry.Instance.Localizations.Localize(["trait", "tiring", "name"]),
                    Description = ModEntry.Instance.Localizations.Localize(["trait", "tiring", "description"]),
                }
            ]
        });

        Instance.Harmony.TryPatch(
		    logger: ModEntry.Instance.Logger,
		    original: AccessTools.DeclaredMethod(typeof(State), nameof(State.GoToZone)),
			postfix: new HarmonyMethod(GetType(), nameof(State_GoToZone_Postfix))
		);

        Instance.Harmony.TryPatch(
		    logger: ModEntry.Instance.Logger,
		    original: AccessTools.DeclaredMethod(typeof(State), nameof(State.PopulateRun)),
			postfix: new HarmonyMethod(GetType(), nameof(State_PopulateRun_Postfix))
		);

        Instance.Harmony.TryPatch(
		    logger: ModEntry.Instance.Logger,
		    original: AccessTools.DeclaredMethod(typeof(Card), nameof(Card.GetDataWithOverrides)),
			postfix: new HarmonyMethod(GetType(), nameof(Card_GetDataWithOverrides_Postfix))
		);
    }

    private static void State_PopulateRun_Postfix(State __instance) {
        ModData.RemoveModData(__instance, TiringCostKey);
    }

    private static void State_GoToZone_Postfix(MapBase? nextMap, State __instance) {
        if (nextMap != null) {
            ModData.SetModData(__instance, TiringCostKey, ModData.GetModDataOrDefault(__instance, TiringCostKey, 0) + 1);
        }
    }

    private static void Card_GetDataWithOverrides_Postfix(State state, Card __instance, ref CardData __result) {
        if (Cards.IsCardTraitActive(state, __instance, TiringTrait)) {
            __result.cost += ModData.GetModDataOrDefault(state, TiringCostKey, 0);
        }
    }
}