using System.Linq;
using TheJazMaster.UnseenEffort.Artifacts.Carrie;

namespace TheJazMaster.UnseenEffort.Actions;

public class AAddFairTrade : CardAction
{
	public required int uses;
	public override void Begin(G g, State s, Combat c)
	{
		foreach (Artifact item in s.EnumerateAllArtifacts()) {
			if (item is FairTradeArtifact artifact) {
				artifact.uses += uses;
				return;
			}
		}
		timer = 0;
		c.Queue(new AAddArtifact {
			artifact = new FairTradeArtifact {
				uses = uses
			}
		});
	}
}