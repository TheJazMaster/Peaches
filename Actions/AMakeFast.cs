using Nickel;
using TheJazMaster.Peaches.Features;

#nullable enable
namespace TheJazMaster.Peaches.Actions;

public class AMakeFast : CardAction
{
	static readonly IModCards CardsHelper = ModEntry.Instance.Helper.Content.Cards;
	public int amount = 0;

	public override Route? BeginWithRoute(G g, State s, Combat c)
	{
		timer = 0;
		if (selectedCard != null) {
			CardsHelper.SetCardTraitOverride(s, selectedCard, PriorityManager.FastTrait, true, true);

			return new CustomShowCards {
				message = ModEntry.Instance.Localizations.Localize(["action", "makeFast", "showCardText"]),
				cardIds = [selectedCard.uuid]
			};	
		}
		return null;
	}

	public override Icon? GetIcon(State s) => new Icon(PriorityManager.FastTrait.Configuration.Icon(DB.fakeState, null)!.Value, amount, Colors.textMain);

	public override string? GetCardSelectText(State s)
	{
		return ModEntry.Instance.Localizations.Localize(["action", "makeFast", "cardSelectText"]);
	}
}