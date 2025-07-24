namespace TheJazMaster.UnseenEffort.Actions;

public class AMultiCardSelect : ACardSelect
{
	public int maxSelected = 999;

	public override Route? BeginWithRoute(G g, State s, Combat c)
	{
		CardBrowse cardBrowse = ModEntry.Instance.KokoroApi.MultiCardBrowse.MakeRoute(new()
		{
			mode = CardBrowse.Mode.Browse,
			browseSource = browseSource,
			browseAction = browseAction,
			filterUnremovableAtShops = filterUnremovableAtShops,
			filterExhaust = filterExhaust,
			filterRetain = filterRetain,
			filterBuoyant = filterBuoyant,
			filterTemporary = filterTemporary,
			includeTemporaryCards = includeTemporaryCards,
			filterOutTheseRarities = filterOutTheseRarities,
			filterMinCost = filterMinCost,
			filterMaxCost = filterMaxCost,
			filterUpgrade = filterUpgrade,
			filterAvailableUpgrade = filterAvailableUpgrade,
			filterUUID = filterUUID,
			ignoreCardType = ignoreCardType,
			allowCancel = allowCancel,
			allowCloseOverride = allowCloseOverride
		}).SetMaxSelected(maxSelected).AsRoute;

		c.Queue(new ADelay
		{
			time = 0.0,
			timer = 0.0
		});
		if (cardBrowse.GetCardList(g).Count == 0)
		{
			timer = 0.0;
			return null;
		}
		return cardBrowse;
	}
}