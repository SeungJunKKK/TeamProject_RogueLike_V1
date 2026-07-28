using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
public class CheatConsole : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.backquoteKey.wasPressedThisFrame)
        {
            Debug.Log($"[Cheat] ===== 상태 =====\n" +
                        $"State: {GameManager.Instance.State} / TimeScale: {Time.timeScale:F2}\n" +
                        $"Gold: {GameManager.Instance.Gold}\n" +
                        $"Level: {DifficultyManager.Instance.Level} (계수 {DifficultyManager.Instance.Coefficient:F2})\n" +
                        $"경과: {DifficultyManager.Instance.ElapsedSeconds:F0}초\n" +
                        $"배율 - EnemyLevel:{DifficultyManager.Instance.EnemyLevel} Gold:{DifficultyManager.Instance.GetGoldMultiplier():F2}\n" +
                        $"Scene: {SceneManager.GetActiveScene().name}");
        }

        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            Debug.Log($"[Cheat] F1 pressed: Difficulty Time +1 minute");
            DifficultyManager.Instance.AddDebugTime(60f);
        }

        if (Keyboard.current.f2Key.wasPressedThisFrame)
        {
            Debug.Log($"[Cheat] F2 pressed: Difficulty Time +5 minutes");
            DifficultyManager.Instance.AddDebugTime(300f);
        }

        if (Keyboard.current.f3Key.wasPressedThisFrame)
        {
            Debug.Log($"[Cheat] F3 pressed: Gold +1000");
            GameManager.Instance.AddGold(1000);
        }
    }
}
#endif