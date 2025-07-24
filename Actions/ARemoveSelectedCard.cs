namespace TheJazMaster.UnseenEffort.Actions;

public class ARemoveSelectedCard : CardAction
{
	public override void Begin(G g, State s, Combat c)
	{
		if (selectedCard == null) return;

		s.RemoveCardFromWhereverItIs(selectedCard.uuid);
	}
}