using UnityEngine;

/// <summary>
/// 상호작용 가능한 오브젝트의 규약. 상자, 텔레포터, 제단 등이 구현.
/// 감지·발동(키 입력)은 상호작용 주체(플레이어) 쪽 책임. 이 규약은 "발동되면 무엇을 할지"만 정의.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// 상호작용이 발동됐을 때 호출된다.
    /// </summary>
    /// <param name="interactor">상호작용을 시작한 주체 (보통 플레이어)</param>
    void Interact(GameObject interactor);
}