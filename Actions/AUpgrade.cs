using Nickel;

namespace TheJazMaster.UnseenEffort.Actions;

public class AUpgrade : CardAction
{
	public override Route? BeginWithRoute(G g, State s, Combat c)
	{
		if (selectedCard == null) return null;

		return new CardUpgrade {
			cardCopy = Mutil.DeepCopy(selectedCard)
		};
	}

	public override string? GetCardSelectText(State s)
	{
		return ModEntry.Instance.Localizations.Localize(["action", "upgrade", "cardSelectText"]);
	}
}