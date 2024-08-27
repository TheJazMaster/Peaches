using System;
using System.Collections.Generic;
using System.Linq;
using daisyowl.text;
using FSPRO;

namespace TheJazMaster.Peaches;

public class CustomShowCards : ShowCards
{
	public string message;

	public override void Render(G g)
	{
		G g2 = g;
		List<Card> list = (from cid in cardIds
			select g2.state.FindCard(cid) into c
			where c != null
			select c).ToList();
		CardUtils.FanOut(list, new Vec(240.0, 90.0));
		foreach (Card item in list)
		{
			item.UpdateAnimation(g2);
		}
		Draw.Sprite(StableSpr.cockpit_deletionChamber, 0.0, 0.0);
		Draw.Fill(Colors.redd.gain(Mutil.Remap(-1.0, 1.0, 0.05, 0.1, Math.Sin(g2.state.time * 4.0))), BlendMode.Add);
		Color? color = Colors.textBold;
		TAlign? align = TAlign.Center;
		Color? outline = Colors.black;
		Draw.Text(message, 240.0, 69.0, null, color, null, null, null, align, dontDraw: false, null, outline);
		SharedArt.ButtonText(g2, new Vec(210.0, 193.0), StableUK.shipUpgrades_continue, Loc.T("uiShared.btnContinue"), null, null, inactive: false, this, null, null, null, null, autoFocus: true);
		foreach (Card item2 in list)
		{
			G g3 = g2;
			State fakeState = DB.fakeState;
			item2.Render(g3, null, fakeState);
		}
	}
}
