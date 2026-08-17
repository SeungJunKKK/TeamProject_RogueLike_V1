using UnityEngine;

public class WurmHead : GildedWurmBase
{

    public override void Setup(Transform player)
    {
        base.Setup(player);

        // 추가적인 공격 패턴이 없는 기본 웜 헤드라면 Setup만 호출해 주면 됩니다.
        // 스폰 직후 Base 클래스가 알아서 플레이어를 스치고 지나가는 궤적 비행을 시작합니다.
    }


}