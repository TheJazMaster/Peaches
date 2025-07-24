using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using TheJazMaster.UnseenEffort.Cards;
using TheJazMaster.UnseenEffort.Cards.Peaches;

namespace TheJazMaster.UnseenEffort.Actions;

public class AVariableHintCards : AVariableHint
{
    static ModEntry Instance => ModEntry.Instance;
    public int? value = null;
    
    public AVariableHintCards() : base() {
        hand = true;
    }

    public override Icon? GetIcon(State s) {
        return new Icon(ModEntry.Instance.WorkforceIcon, null, Colors.textMain);
    }

	public override List<Tooltip> GetTooltips(State s)
	{
		List<Tooltip> tooltips = [];

        string delimiter = Instance.Localizations.Localize(["card", "Peaches", "YoureFired", "tooltip", "delimiter"]);
        string delimiterFinal = Instance.Localizations.Localize(["card", "Peaches", "YoureFired", "tooltip", "delimiterFinal"]);

        DefaultInterpolatedStringHandler stringHandler = new(22, 1);
				
        List<string> deckNames = [];
        List<Color> deckColors = [];
        int? cards = null;

		s = MG.inst.g.state;
        if (s.IsOutsideRun()) {
            deckNames = [
                Instance.Localizations.Localize(["card", "YoureFired", "tooltip", "placeholder"], new { Number = 1 }),
                Instance.Localizations.Localize(["card", "YoureFired", "tooltip", "placeholder"], new { Number = 2 })
            ];
            deckColors = [Colors.white, Colors.white];
        } else {
            foreach(Character character in s.characters) {
                if (character.deckType != null && character.deckType != ModEntry.Instance.PeachesCharacter.Configuration.Deck) {
                    deckNames.Add(Character.GetDisplayName(character.deckType.Value, s));
                    deckColors.Add(DB.decks[character.deckType.Value].color);
                }
            }
            cards = YoureFiredCard.CountNonPeachesCards(s.route as Combat ?? null);
        }

        for (int i = 0; i < deckNames.Count; i++) {
            string name = deckNames[i];
			stringHandler.AppendFormatted(ModEntry.Instance.Localizations.Localize(["card", "YoureFired", "tooltip", "segment"],
                new { Color = deckColors[i].ToString(), Character = name }));

            if (i != deckNames.Count - 1)
                stringHandler.AppendLiteral((i == deckNames.Count - 2) ? delimiterFinal : delimiter);
        }
        var str = stringHandler.ToStringAndClear();

        string count = "";
        if (cards != null)
            count = $" (<c=keyword>{cards.Value}</c>)";

		tooltips.Add(new TTText(ModEntry.Instance.Localizations.Localize(["card", "YoureFired", "tooltip", "desc"], new { Text = str, Count = count })));

		return tooltips;
	}
}