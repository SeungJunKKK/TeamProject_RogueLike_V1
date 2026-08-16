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

        if (Keyboard.current.f10Key.wasPressedThisFrame) // 레벨업 이펙트 테스트용
        {
            PlayerController player = FindAnyObjectByType<PlayerController>();
            if (player == null || player.Stats == null)
            {
                Debug.LogWarning($"[Cheat] F10: 씬에 PlayerController 또는 PlayerStats가 없습니다.");
            }
            else
            {
                // 다음 레벨까지 필요한 경험치를 채워서 레벨업
                float expNeeded = player.Stats.GetRequiredExp(player.Stats.CurrentLevel) - player.Stats.CurrentExp;
                player.Stats.AddExp(expNeeded > 0 ? expNeeded : 100f);

                Debug.Log($"<color=green>[Cheat] F10 pressed: 플레이어 레벨 업! (현재 레벨: {player.Stats.CurrentLevel})</color>");
            }
        }

        if (Keyboard.current.f11Key.wasPressedThisFrame)
        {
            PlayerController player = FindAnyObjectByType<PlayerController>();
            if (player == null || player.Stats == null)
            {
                Debug.LogWarning($"[Cheat] F11: 씬에 PlayerController 또는 PlayerStats가 없습니다.");
            }
            else
            {
                Debug.Log($"<color=red>[Cheat] F11 pressed: 플레이어 체력 강제 0으로 설정 (사망 테스트)</color>");

                player.TakeDamage(player.Stats.CurrentHealth + 9999f);
            }
        }

        if (Keyboard.current.f12Key.wasPressedThisFrame)
        {
            PlayerController player = FindAnyObjectByType<PlayerController>();
            if (player == null || player.Stats == null)
            {
                Debug.LogWarning($"[Cheat] F12: 씬에 PlayerController 또는 PlayerStats가 없습니다.");
            }
            else
            {
                PlayerStats stats = player.Stats;
                float armorValue = stats.Armor != null ? stats.Armor.Value : 0f;

                // 인벤토리에서 아이템 목록과 개수 가져오기
                string itemListStr = "없음";
                PlayerInventory inventory = player.GetComponent<PlayerInventory>();
                if (inventory != null && inventory.passiveItems.Count > 0)
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    foreach (var pair in inventory.passiveItems)
                    {
                        sb.Append($"\n   - {pair.Key.itemName} (x{pair.Value})");
                    }
                    itemListStr = sb.ToString();
                }

                Debug.Log($"<color=cyan>[Cheat] ===== 🧑‍🚀 현재 플레이어 정보 =====</color>\n" +
                          $" 체력: {stats.CurrentHealth:F1} / {stats.MaxHealth.Value:F1}\n" +
                          $" 회복: {stats.HealthRegen.Value:F1} / 초\n" +
                          $" 방어력: {armorValue:F1}\n" +
                          $" 공격력: {stats.Damage.Value:F1} |  공속: {stats.AttackSpeed.Value:F2}\n" +
                          $" 치명타 확률: {stats.CritChance.Value * 100:F1}% |  치명타 배율: {stats.CritDamage.Value * 100:F0}%\n" +
                          $" 이동 속도: {stats.MoveSpeed.Value:F1}\n" +
                          $" 레벨: {stats.CurrentLevel} (EXP: {stats.CurrentExp:F0} / {stats.GetRequiredExp(stats.CurrentLevel):F0})\n" +
                          $" <color=yellow>보유 아이템:</color> {itemListStr}");
            }
        }

        //=========================================
        //숫자 단축기 
        //=========================================

        if (Keyboard.current.digit0Key.wasPressedThisFrame)
        {
            PlayerInventory inventory = FindAnyObjectByType<PlayerInventory>();
            if (inventory == null)
            {
                Debug.LogWarning($"[Cheat] 숫자 0: 씬에 PlayerInventory가 없습니다.");
            }
            else
            {
                Debug.Log($"<color=cyan>[Cheat] 숫자 0 pressed: 모든 쿨타임 즉시 초기화!</color>");

                inventory.ResetActiveCooldown();

                // 나중에 PlayerController 쪽에 스킬(Q, E, R 등) 쿨타임 기능이 생기면 
                // 여기에 player.ResetAllSkillCooldowns(); 같은 함수를 추가로 호출
            }
        }

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            Chest chest = FindAnyObjectByType<Chest>();
            if (chest == null)
            {
                Debug.LogWarning($"[Cheat] 숫자 1: 씬에 Chest가 없습니다.");
            }
            else
            {
                Debug.Log($"<color=green>[Cheat] 숫자 1 pressed: 상자 오픈 (Chest.Interact)</color>");
                chest.Interact(gameObject);
            }
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            PlayerController player = FindAnyObjectByType<PlayerController>();
            if (player == null || player.Stats == null)
            {
                Debug.LogWarning($"[Cheat] 숫자 2: 씬에 PlayerController 또는 PlayerStats가 없습니다.");
            }
            else
            {
                float damageAmount = player.Stats.CurrentHealth * 0.7f;
                Debug.Log($"<color=orange>[Cheat] 숫자 2 pressed: 현재 체력 70% 감소 (-{damageAmount:F1} 피해)</color>");
                player.TakeDamage(damageAmount);
            }
        }



    }
}
#endif