using UnityEngine;
using UnityEngine.InputSystem;

public class EventBusTest : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current == null) return;

        // 1: 구독
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            EventBus.Subscribe<SceneLoadStartedEvent>(OnSceneLoadStarted);
            Debug.Log("[Tester] 구독 완료");
        }

        // 2: 발행 — 구독 상태면 핸들러 로그가 따라 찍혀야 함
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            Debug.Log("[Tester] 발행!");
            EventBus.Publish(new SceneLoadStartedEvent { SceneName = "TestScene" });
        }

        // 3: 해제
        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            EventBus.Unsubscribe<SceneLoadStartedEvent>(OnSceneLoadStarted);
            Debug.Log("[Tester] 해제 완료");
        }

        // 4: 아무도 안 듣는 이벤트 발행 — 에러 없이 지나가야 함
        if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            EventBus.Publish(new SceneLoadCompletedEvent { SceneName = "Nobody" });
            Debug.Log("[Tester] 무청취 발행 — 이 줄이 에러 없이 찍히면 통과");
        }
    }

    // 반드시 "이름 있는 메서드" — 램다로 구독하면 Unsubscribe가 같은 놈을 못 찾음 (신원 비교)
    private void OnSceneLoadStarted(SceneLoadStartedEvent e)
    {
        Debug.Log($"[Handler] 수신: {e.SceneName}");
    }
}
