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

        if (Keyboard.current.f4Key.wasPressedThisFrame)
        {
            Debug.Log($"[Cheat] F4 pressed: StartGame (Ready -> Playing)");
            GameManager.Instance.StartGame();
        }

        if (Keyboard.current.f5Key.wasPressedThisFrame)
        {
            bool nextPaused = !GameManager.Instance.IsPaused;
            Debug.Log($"[Cheat] F5 pressed: SetPause({nextPaused})");
            GameManager.Instance.SetPause(nextPaused);
        }

        if (Keyboard.current.f6Key.wasPressedThisFrame)
        {
            Teleporter teleporter = FindAnyObjectByType<Teleporter>();
            if (teleporter == null)
            {
                Debug.LogWarning($"[Cheat] F6: 씬에 Teleporter가 없습니다.");
            }
            else
            {
                Debug.Log($"[Cheat] F6 pressed: Teleporter.Interact (활성화 / 클리어 시 다음 스테이지)");
                teleporter.Interact(gameObject);
            }
        }

        if (Keyboard.current.f7Key.wasPressedThisFrame)
        {
            Teleporter teleporter = FindAnyObjectByType<Teleporter>();
            if (teleporter == null)
            {
                Debug.LogWarning($"[Cheat] F7: 씬에 Teleporter가 없습니다.");
            }
            else
            {
                Debug.Log($"[Cheat] F7 pressed: Teleporter 충전 즉시 완료");
                teleporter.DebugForceChargeComplete();
            }
        }

        if (Keyboard.current.f8Key.wasPressedThisFrame)
        {
            Teleporter teleporter = FindAnyObjectByType<Teleporter>();
            if (teleporter == null)
            {
                Debug.LogWarning($"[Cheat] F8: 씬에 Teleporter가 없습니다.");
            }
            else
            {
                Debug.Log($"[Cheat] F8 pressed: 가짜 보스 처치");
                teleporter.DebugKillBoss();
            }
        }

        if (Keyboard.current.f9Key.wasPressedThisFrame)
        {
            Chest chest = FindAnyObjectByType<Chest>();
            if (chest == null)
            {
                Debug.LogWarning($"[Cheat] F9: 씬에 Chest가 없습니다.");
            }
            else
            {
                Debug.Log($"[Cheat] F9 pressed: Chest.Interact (골드 충분 시 열림)");
                chest.Interact(gameObject);
            }
        }
    }
}
#endif