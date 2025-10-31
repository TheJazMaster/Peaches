using System.Linq;
using TheJazMaster.UnseenEffort.Artifacts.Carrie;
using TheJazMaster.UnseenEffort.Features;

namespace TheJazMaster.UnseenEffort.Actions;

public class ASequencedCardOffering : ACardOffering
{
    public required int index;
	public required int total;
    public override Route? BeginWithRoute(G g, State s, Combat c)
	{
        return base.BeginWithRoute(g, s, c).ApplyModData(FindManager.RewardSequenceKey, new FindManager.SequenceData {
			Total = total,
			Index = index
		});
    }
}