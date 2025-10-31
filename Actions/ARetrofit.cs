using System.Collections.Generic;
using System.Linq;
using FSPRO;
using Nickel;
using TheJazMaster.UnseenEffort.Artifacts.Carrie;

namespace TheJazMaster.UnseenEffort.Actions;

public class ARetrofit : CardAction
{
    public int drawChange;
	public int energyChange;

    public override void Begin(G g, State s, Combat c)
	{
		foreach (Artifact item in s.EnumerateAllArtifacts()) {
			if (item is RetrofittedPartsArtifact artifact) {
                artifact.plusDraw += drawChange;
				artifact.plusEnergy += energyChange;
                artifact.Pulse();
                s.ship.baseDraw += drawChange;
				s.ship.baseEnergy += energyChange;
                return;
			}
		}
		
		c.Queue(new AAddArtifact {
			artifact = new RetrofittedPartsArtifact {
				plusDraw = drawChange,
				plusEnergy = energyChange
			}
		});
	}
}