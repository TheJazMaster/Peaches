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
using TheJazMaster.UnseenEffort.Artifacts.Carrie;

namespace TheJazMaster.UnseenEffort.Features;

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
            if (!(state.map.markers[state.map.currentLocation].contents is MapBattle mapBattle && mapBattle.battleType == BattleType.Boss && state.map.IsFinalZone())) return;
            foreach (Card card in state.deck) {
                if (Cards.IsCardTraitActive(state, card, MundaneTrait)) state.RemoveCardFromWhereverItIs(card.uuid);
            }
            if (state.EnumerateAllArtifacts().OfType<PackageArtifact>().FirstOrDefault() is { } artifact) {
                artifact.packagedCards.RemoveAll(card => Cards.IsCardTraitActive(state, card, MundaneTrait));
            }
        });
    }
}