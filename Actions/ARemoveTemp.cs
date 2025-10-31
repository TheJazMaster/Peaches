using System.Collections.Generic;
using FSPRO;
using Nickel;

namespace TheJazMaster.UnseenEffort.Actions;

public class ARemoveTemp : CardAction
{
	private static IModCards Cards => ModEntry.Instance.Helper.Content.Cards;
	public override void Begin(G g, State s, Combat c)
	{
		if (selectedCard == null) return;

		Cards.SetCardTraitOverride(s, selectedCard, Cards.TemporaryCardTrait, false, true);
		Audio.Play(Event.Status_PowerUp);
	}

	public override List<Tooltip> GetTooltips(State s) => [
		new TTGlossary("cardtrait.temporary")
	];
}