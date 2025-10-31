using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TheJazMaster.UnseenEffort.Artifacts;

public interface IFurySpenderArtifact
{
    public void OnFurySpend(State s, Combat c, AAttack attack, int amount);
}