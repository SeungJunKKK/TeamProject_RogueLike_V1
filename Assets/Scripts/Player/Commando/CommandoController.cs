using Player.Commando;
using UnityEngine;

public class CommandoController : PlayerController
{
    public override IState GetPrimaryAttackState()
    {
        return new DoubleTapState(this, 0.4f);
    }

    public override IState GetSecondaryAttackState()
    {
        return new FullMetalJacketState(this, 0.417f);
    }
}