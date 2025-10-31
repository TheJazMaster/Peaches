using System.Collections.Generic;
using Nanoray.Shrike;
using Nanoray.Shrike.Harmony;
using HarmonyLib;
using System.Reflection.Emit;
using System.Reflection;

namespace TheJazMaster.UnseenEffort.Features;

[HarmonyPatch]
public class HardWorkerManager
{
	[HarmonyTranspiler]
	[HarmonyPatch(typeof(Combat), nameof(Combat.TryPlayCard))]
	private static IEnumerable<CodeInstruction> Combat_TryPlayCard_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il, MethodBase originalMethod)
	{
		var newLocal = il.DeclareLocal(typeof(int));
		List<CodeInstruction> newInstructions = (List<CodeInstruction>)new SequenceBlockMatcher<CodeInstruction>(instructions)
			.Find(
				ILMatches.AnyLdloc,
				ILMatches.Ldfld("card"),
				ILMatches.Ldarg(1),
				ILMatches.Call("GetCurrentCost"),
				ILMatches.Br,
				ILMatches.LdcI4(0),
				ILMatches.Stloc<int>(originalMethod.GetMethodBody()!.LocalVariables).Anchor(out var endAnchor)
			)
			.PointerMatcher(SequenceMatcherRelativeElement.First)
			.Encompass(SequenceMatcherEncompassDirection.After, 1)
			.Insert(SequenceMatcherPastBoundsDirection.After, SequenceMatcherInsertionResultingBounds.IncludingInsertion, new List<CodeInstruction> {
				new(OpCodes.Dup),
				new(OpCodes.Ldarg_1),
				new(OpCodes.Call, AccessTools.DeclaredMethod(typeof(Card), nameof(Card.GetCurrentCost))),
				new(OpCodes.Stloc, newLocal)
			})
			.AllElements();

		var match = new SequenceBlockMatcher<CodeInstruction>(newInstructions)
			.Find(
				ILMatches.Call("OnPlayerPlayCard")
			)
			.EncompassUntil(SequenceMatcherPastBoundsDirection.After, new List<ElementMatch<CodeInstruction>> { ILMatches.Instruction(OpCodes.Leave_S) });

		Label branchTarget = (Label)match.PointerMatcher(SequenceMatcherRelativeElement.Last).Element().operand;

		var ret = match
			.PointerMatcher(branchTarget)
			.ExtractLabels(out var extractedLabels)
			.Insert(SequenceMatcherPastBoundsDirection.Before, SequenceMatcherInsertionResultingBounds.IncludingInsertion, new List<CodeInstruction> {
				new CodeInstruction(OpCodes.Ldarg_1).WithLabels(extractedLabels),
				new(OpCodes.Ldarg_0),
				new(OpCodes.Ldarg_2),
				new(OpCodes.Ldloc, newLocal),
				new(OpCodes.Ldarg_3),
				new(OpCodes.Ldarg, 4),
				new(OpCodes.Call, AccessTools.DeclaredMethod(typeof(HardWorkerManager), nameof(AddFury))),
			})
			.AllElements();

		return ret;
	}
	private static void AddFury(State s, Combat c, Card card, int apparentCost, bool playNoMatterWhatForFree, bool exhaustNoMatterWhat)
	{
		if (apparentCost >= 2)
		{
			var amount = s.ship.Get(ModEntry.Instance.HardWorkerStatus);
			if (amount > 0)
				c.Queue(new AStatus
				{
					targetPlayer = true,
					status = ModEntry.Instance.FuryStatus,
					statusAmount = amount,
					statusPulse = ModEntry.Instance.HardWorkerStatus
				});
		}
	}
}