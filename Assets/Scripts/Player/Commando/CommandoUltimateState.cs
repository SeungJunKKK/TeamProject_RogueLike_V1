using UnityEngine;

public class CommandoUltimateState : PlayerAttackState
{
    public CommandoUltimateState(PlayerController player, float duration)
        : base(player, "V_Ultimate", duration, SkillType.Ultimate_V) { }

    protected override void ExecuteShoot()
    {

    }
}