using System.Collections.Generic;
using Nickel;
using TheJazMaster.UnseenEffort.Features;

namespace TheJazMaster.UnseenEffort.Actions;

public class CardSelectAddBuoyantAndRetainForever : CardAction
{
	public override Route? BeginWithRoute(G g, State s, Combat c)
	{
		Card? card = selectedCard;
		if (card != null)
		{
			card.buoyantOverrideIsPermanent = true;
			card.buoyantOverride = true;
			return new CustomShowCards
			{
				message = ModEntry.Instance.Localizations.Localize(["card", "ElbowGrease", "showCardText"]),
				cardIds = new List<int> { card.uuid }
			};
		}
		return null;
	}

	public override string? GetCardSelectText(State s)
	{
		return ModEntry.Instance.Localizations.Localize(["card", "ElbowGrease", "cardSelectText"]);
	}
}