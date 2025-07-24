using System.Collections.Generic;
using FSPRO;
using Nickel;

namespace TheJazMaster.UnseenEffort.Actions;

public class ARemoveTempFromHand : CardAction
{
	private static IModCards Cards => ModEntry.Instance.Helper.Content.Cards;
	public override void Begin(G g, State s, Combat c)
	{
		bool works = false;
		foreach (Card card in c.hand) {
			if (Cards.IsCardTraitActive(s, card, Cards.TemporaryCardTrait)) {
				works = true;
				Cards.SetCardTraitOverride(s, card, Cards.TemporaryCardTrait, false, true);
			}
		}
		if (works) Audio.Play(Event.Status_PowerUp);
	}

	public override List<Tooltip> GetTooltips(State s) => [
		new TTGlossary("cardtrait.temporary")
	];
}