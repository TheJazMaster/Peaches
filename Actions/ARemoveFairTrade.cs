using System.Linq;
using TheJazMaster.UnseenEffort.Artifacts.Carrie;

namespace TheJazMaster.UnseenEffort.Actions;

public class ARemoveFairTrade : CardAction
{
	public override void Begin(G g, State s, Combat c)
	{
		foreach (Artifact item in s.EnumerateAllArtifacts()) {
			if (item is FairTradeArtifact artifact) {
				artifact.uses -= 1;
				if (artifact.uses == 0)
					s.GetCurrentQueue().QueueImmediate(new ALoseArtifact {
						artifactType = artifact.Key()
					});
				return;
			}
		}
	}
}