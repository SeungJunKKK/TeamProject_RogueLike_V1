using UnityEngine;

// 추가할 것 있으면 추가하면 됨
public struct DamageInfo
{
    public float Amount;           // 최종 데미지량 (크리티컬/아이템 배율 적용 완료값)
    public Vector2 HitPoint;       // 타격 지점 (플로팅 텍스트/이펙트 위치)
    public Vector2 HitDirection;   // 타격 방향 (정규화) — 넉백 계산용
    public float KnockbackForce;   // 넉백 세기
    public GameObject Attacker;    // 공격 주체 (킬 크레딧, 아이템 소유자 판정) — null 허용
    public bool IsCrit;            // 크리티컬 여부 — 플로팅 텍스트 연출 분기용
    public bool CanProc;           // 온힛 아이템 발동 가능 여부 (RoR1의 Active/Inactive 구분)
                                   // 플레이어 직접 공격 = true / 아이템이 발동시킨 공격 = false (무한 연쇄 방지)
}

public interface IDamageable 
{
    void TakeDamage(DamageInfo info);
}
