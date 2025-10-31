using Nickel;
using HarmonyLib;

namespace TheJazMaster.UnseenEffort.Features;

[HarmonyPatch]
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
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(State), nameof(State.PopulateRun))]
    private static void State_PopulateRun_Postfix(State __instance) {
        ModData.RemoveModData(__instance, TiringCostKey);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(State), nameof(State.GoToZone))]
    private static void State_GoToZone_Postfix(MapBase? nextMap, State __instance) {
        if (nextMap != null) {
            ModData.SetModData(__instance, TiringCostKey, ModData.GetModDataOrDefault(__instance, TiringCostKey, 0) + 1);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Card), nameof(Card.GetDataWithOverrides))]
    private static void Card_GetDataWithOverrides_Postfix(State state, Card __instance, ref CardData __result) {
        if (Cards.IsCardTraitActive(state, __instance, TiringTrait)) {
            __result.cost += ModData.GetModDataOrDefault(state, TiringCostKey, 0);
        }
    }
}