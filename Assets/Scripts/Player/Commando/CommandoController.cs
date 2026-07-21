using Player.Commando;
using UnityEngine;

public class CommandoController : PlayerController
{
    [Header("Item Test")]
    public bool IsBarrageMode = false; 

    public override IState GetPrimaryAttackState()
    {
        return new DoubleTapState(this, 0.4f);
    }

    public override IState GetSecondaryAttackState()
    {
        return new FullMetalJacketState(this, 0.417f);
    }

    public override IState GetUltimateSkillState()
    {
        if (IsBarrageMode)
        {
            return new SuppressiveBarrageState(this, 1.5f);
        }
        else
        {
            return new SuppressiveFireState(this, 1.0f);
        }
    }
}