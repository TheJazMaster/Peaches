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

namespace TheJazMaster.Peaches.Features;
#nullable enable

public class PriorityManager
{
    static ModEntry Instance => ModEntry.Instance;
    static IModHelper Helper => Instance.Helper;
    static IModData ModData => Helper.ModData;
    static readonly string RuinedPriorityKey = "RuinedPriority";
    static readonly string RenderPriorityFlippedKey = "RenderPriorityFlipped";

    internal static ICardTraitEntry PriorityTrait { get; private set; } = null!;
    internal static ICardTraitEntry FastTrait { get; private set; } = null!;

    public PriorityManager()
    {
        Spr FastIcon = Instance.Helper.Content.Sprites.RegisterSprite(Instance.Package.PackageRoot.GetRelativeFile("Sprites/icons/Fast.png")).Sprite;
		Spr PriorityIcon = Instance.Helper.Content.Sprites.RegisterSprite(Instance.Package.PackageRoot.GetRelativeFile("Sprites/icons/Priority.png")).Sprite;
        Spr PriorityOffIcon = Instance.Helper.Content.Sprites.RegisterSprite(Instance.Package.PackageRoot.GetRelativeFile("Sprites/icons/Priority_off.png")).Sprite;

        PriorityTrait = ModEntry.Instance.Helper.Content.Cards.RegisterTrait("Priority", new() {
            Icon = (state, card) => HasRuinedPriority(state, card) ? PriorityOffIcon : PriorityIcon,
            Name = ModEntry.Instance.AnyLocalizations.Bind(["trait", "priority"]).Localize,
            Tooltips = (state, card) => [ HasRuinedPriority(state, card) ?
                new GlossaryTooltip($"trait.{GetType().Namespace!}::Priority") {
                    Icon = PriorityOffIcon,
                    TitleColor = Colors.action,
                    Title = ModEntry.Instance.Localizations.Localize(["trait", "priority", "name"]),
                    Description = ModEntry.Instance.Localizations.Localize(["trait", "priority", "descriptionOff"]),
                } :
                new GlossaryTooltip($"trait.{GetType().Namespace!}::Priority") {
                    Icon = PriorityIcon,
                    TitleColor = Colors.action,
                    Title = ModEntry.Instance.Localizations.Localize(["trait", "priority", "name"]),
                    Description = ModEntry.Instance.Localizations.Localize(["trait", "priority", "description"]),
                }
            ]
        });

        FastTrait = ModEntry.Instance.Helper.Content.Cards.RegisterTrait("Fast", new() {
            Icon = (_, _) => FastIcon,
            Name = ModEntry.Instance.AnyLocalizations.Bind(["trait", "fast"]).Localize,
            Tooltips = (_, _) => [
                new GlossaryTooltip($"trait.{GetType().Namespace!}::Fast") {
                    Icon = FastIcon,
                    TitleColor = Colors.action,
                    Title = ModEntry.Instance.Localizations.Localize(["trait", "fast", "name"]),
                    Description = ModEntry.Instance.Localizations.Localize(["trait", "fast", "description"]),
                },
                new GlossaryTooltip($"trait.{GetType().Namespace!}::Priority") {
                    Icon = PriorityIcon,
                    TitleColor = Colors.action,
                    Title = ModEntry.Instance.Localizations.Localize(["trait", "priority", "name"]),
                    Description = ModEntry.Instance.Localizations.Localize(["trait", "priority", "description"]),
                }
            ]
        });

        Instance.Harmony.TryPatch(
		    logger: ModEntry.Instance.Logger,
		    original: AccessTools.DeclaredMethod(typeof(AStartPlayerTurn), nameof(AStartPlayerTurn.Begin)),
			finalizer: new HarmonyMethod(GetType(), nameof(AStartPlayerTurn_Begin_Finalizer))
		);
        Instance.Harmony.TryPatch(
		    logger: ModEntry.Instance.Logger,
		    original: AccessTools.DeclaredMethod(typeof(Combat), nameof(Combat.TryPlayCard)),
			postfix: new HarmonyMethod(GetType(), nameof(Combat_TryPlayCard_Postfix))
		);
        Instance.Harmony.TryPatch(
		    logger: ModEntry.Instance.Logger,
		    original: AccessTools.DeclaredMethod(typeof(CardReward), nameof(CardReward.Render)),
			postfix: new HarmonyMethod(GetType(), nameof(CardReward_Render_Postfix))
		);
    }

    internal static bool HasRuinedPriority(State state, Card? card) {
        if (state == DB.fakeState) {
            if (card != null && Helper.ModData.TryGetModData(card, RenderPriorityFlippedKey, out bool data)) {
                return data;
            }
            return false;
        }
        return state.route is Combat c && ModData.TryGetModData(c, RuinedPriorityKey, out bool has) && has;
    }

    private static void AStartPlayerTurn_Begin_Finalizer(G g, State s, Combat c) {
        ModData.RemoveModData(c, RuinedPriorityKey);
    }

    private static void Combat_TryPlayCard_Postfix(State s, Combat __instance, Card card, bool playNoMatterWhatForFree, bool exhaustNoMatterWhat, ref bool __result) {
        if (__result && !(Helper.Content.Cards.IsCardTraitActive(s, card, PriorityTrait) || Helper.Content.Cards.IsCardTraitActive(s, card, FastTrait)))
            ModData.SetModData(__instance, RuinedPriorityKey, true);
    }

    internal static List<CardAction> PrioritySet(State s, Card card, List<CardAction> first, List<CardAction> second) {
        if (HasRuinedPriority(s, card)) {
            foreach (CardAction action in first)
                action.disabled = true;
        }
        else {
            foreach (CardAction action in second)
                action.disabled = true;
        }
        return [.. first.Concat([new ADummyAction()]), .. second];
    }

    private static Route? lastCardReward;
    private static void CardReward_Render_Postfix(G g, CardReward __instance)  {
        lastCardReward = __instance;
		if (__instance.flipFloppableCardsTimer > 3.0) {
			foreach (Card card in __instance.cards)
			{
				if (Helper.Content.Cards.IsCardTraitActive(g.state, card, PriorityTrait)) {
					Helper.ModData.SetModData(card, RenderPriorityFlippedKey, !Helper.ModData.GetModDataOrDefault(card, RenderPriorityFlippedKey, false));
					card.flipAnim = 1.0;
				}
			}
		}
    }
}