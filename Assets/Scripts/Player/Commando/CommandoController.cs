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

    public override IState GetUtilitySkillState()
    {
        return new CommandoUtilityState(this, 0.5f); 
    }

    public override IState GetUltimateSkillState()
    {
        return new CommandoUltimateState(this, 1.0f); 
    }

}