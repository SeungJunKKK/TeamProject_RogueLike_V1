using UnityEngine;

public class EnforcerController : PlayerController
{
    protected override void UpdateFacingDirection()
    {
        // 방어 모드가 켜져 있다면, 부모의 회전 로직을 무시하고 함수를 종료합니다 (방향 고정!)
        if (IsDefending)
        {
            return;
        }

        // 방어 모드가 아닐 때만 정상적으로 좌우로 회전합니다.
        base.UpdateFacingDirection();
    }


    public void ToggleShieldStance()
    {
        IsDefending = !IsDefending;

        if (IsDefending)
        {
            Debug.Log("<color=green>[엔포서] 방어 자세 ON: 방향 고정, 이속 감소, 공속 증가.</color>");

            Stats.MoveSpeed.AddModifier(new StatModifier(-0.5f, StatModType.PercentAdd, "ShieldStance"));
            Stats.AttackSpeed.AddModifier(new StatModifier(0.3f, StatModType.PercentAdd, "ShieldStance"));
        }
        else
        {
            Debug.Log("<color=blue>[엔포서] 방어 자세 OFF: 스탯 원상 복구, 방향 고정 해제.</color>");

            // 방어 자세를 풀면 "ShieldStance" 이름표가 붙은 변경점들을 모두 제거하여 원상 복구
            Stats.MoveSpeed.RemoveAllModifiersFromSource("ShieldStance");
            Stats.AttackSpeed.RemoveAllModifiersFromSource("ShieldStance");
        }
    }

    public override IState GetPrimaryAttackState()
    {
        float duration = 0.4f / Stats.AttackSpeed.Value;
        return new EnforcerShotgunState(this, duration);
    }

    public override IState GetSecondaryAttackState()
    {
        const float ShieldSlamClipLength = 1.1666667f; 
        float duration = ShieldSlamClipLength / Stats.AttackSpeed.Value;
        return new ShieldSlamState(this, duration);
    }

    public override IState GetUtilitySkillState()
    {
        return new EnforcerToggleShieldState(this);
    }

    public override IState GetUltimateSkillState()
    {
        float duration = 0.4f / Stats.AttackSpeed.Value;
        return new EnforcerGrenadeState(this, duration);
    }

    /// <summary>
    /// 애니메이션 이벤트에서 호출할 공격 타이밍 함수입니다.
    /// </summary>
    public void TriggerAttackEvent()
    {
        if (m_CurrentState is EnforcerShotgunState shotgunState)
        {
            shotgunState.FireShotgun();
        }
    }
    protected override void OnBlockSuccess(DamageInfo info)
    {
        //base.OnBlockSuccess(info);

        PlayAddressableSFX(X_SFXAddress);
    }

}