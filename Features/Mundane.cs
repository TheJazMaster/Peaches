using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Nanoray.Shrike;
using Nanoray.Shrike.Harmony;
using Nickel;
using TheJazMaster.UnseenEffort.Artifacts.Carrie;

namespace TheJazMaster.UnseenEffort.Features;

[HarmonyPatch]
public class MundaneManager
{
    static ModEntry Instance => ModEntry.Instance;
    static IModHelper Helper => Instance.Helper;
    static IModData ModData => Helper.ModData;
    static IModCards Cards => Helper.Content.Cards;


    internal static ICardTraitEntry MundaneTrait { get; private set; } = null!;

    public MundaneManager()
    {
        Spr MundaneIcon = Instance.Helper.Content.Sprites.RegisterSprite(Instance.Package.PackageRoot.GetRelativeFile("Sprites/icons/Mundane.png")).Sprite;

        MundaneTrait = ModEntry.Instance.Helper.Content.Cards.RegisterTrait("Mundane", new() {
            Icon = (state, card) => MundaneIcon,
            Name = ModEntry.Instance.AnyLocalizations.Bind(["trait", "mundane"]).Localize,
            Tooltips = (state, card) => [
                new GlossaryTooltip($"trait.{GetType().Namespace!}::Mundane") {
                    Icon = MundaneIcon,
                    TitleColor = Colors.cardtrait,
                    Title = ModEntry.Instance.Localizations.Localize(["trait", "mundane", "name"]),
                    Description = ModEntry.Instance.Localizations.Localize(["trait", "mundane", "description"]),
                }
            ]
        });

        ModEntry.Instance.Helper.Events.RegisterAfterArtifactsHook(nameof(Artifact.OnCombatStart), (State state, Combat combat) => {
            if (!IsFinalBossNode(state)) return;

            state.deck.RemoveAll(card => Cards.IsCardTraitActive(state, card, MundaneTrait));
            if (state.EnumerateAllArtifacts().OfType<PackageArtifact>().FirstOrDefault() is { } artifact) {
                artifact.packagedCards.RemoveAll(card => Cards.IsCardTraitActive(state, card, MundaneTrait));
            }
        });
    }

    private static bool IsFinalBossNode(State state) => state.map.markers[state.map.currentLocation].contents is MapBattle mapBattle && mapBattle.battleType == BattleType.Boss && state.map.IsFinalZone();

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(CardReward), nameof(CardReward.GetOffering))]
    private static IEnumerable<CodeInstruction> ACardSelect_BeginWithRoute_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il, MethodBase originalMethod)
    {
        return new SequenceBlockMatcher<CodeInstruction>(instructions)
            .Find(
                ILMatches.Stfld("rarity"),
                ILMatches.Ldsfld("releasedCards"),
                ILMatches.AnyLdloc,
                ILMatches.Instruction(OpCodes.Ldftn),
                ILMatches.Instruction(OpCodes.Newobj),
                ILMatches.Call("Where"),
                ILMatches.Call("ToList")
            )
			.Insert(SequenceMatcherPastBoundsDirection.After, SequenceMatcherInsertionResultingBounds.IncludingInsertion, [
                new(OpCodes.Ldarg_0),
                new(OpCodes.Call, AccessTools.DeclaredMethod(typeof(MundaneManager), nameof(DontOfferMundaneCards))),
            ])
            .AllElements();
    }

    private static List<Card> DontOfferMundaneCards(List<Card> cards, State s) {
        if (!IsFinalBossNode(s)) return cards;

        return [.. cards.Where(card => !Cards.IsCardTraitActive(s, card, MundaneTrait))];
    }
}