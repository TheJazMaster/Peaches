using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TheJazMaster.Peaches.Artifacts;

public interface IBideSpenderArtifact
{
    public void OnBideSpend(State s, Combat c, AAttack attack, int amount);
}