using Player.Commando;
using UnityEngine;

public class CommandoController : PlayerController
{
    [Header("Item Test")]
    public bool IsBarrageMode = false;

    public override IState GetPrimaryAttackState()
    {
        float duration = 0.4f / Stats.AttackSpeed.Value;
        return new DoubleTapState(this, duration);
    }
    public override IState GetSecondaryAttackState()
    {
        float duration = 0.417f / Stats.AttackSpeed.Value;
        return new FullMetalJacketState(this, duration);
    }

    public override IState GetUtilitySkillState()
    {
        return new PlayerDashState(this);
    }

    public override IState GetUltimateSkillState()
    {
        if (IsBarrageMode)
        {
            float duration = 1.5f / Stats.AttackSpeed.Value;
            return new SuppressiveBarrageState(this, duration);
        }
        else
        {
            float duration = 1.0f / Stats.AttackSpeed.Value;
            return new SuppressiveFireState(this, duration);
        }
    }

}