using UnityEngine;

public class HomeGate : MonoBehaviour
{
    private bool m_Triggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (m_Triggered) return;

        if (collision.CompareTag("Player"))
        {
            m_Triggered = true;

            GameResultUI resultUI = Object.FindAnyObjectByType<GameResultUI>(FindObjectsInactive.Include);

            if (resultUI != null)
            {
                resultUI.DisplayResult(true);
            }
            else
            {
                Debug.LogWarning("<color=orange>[HomeGate] 씬에 GameResultUI를 찾을 수 없습니다!</color>");
            }

            Time.timeScale = 0f;

            Debug.Log("<color=green>[HomeGate] 보스 씬 클리어! Won 출력 완료</color>");
        }
    }
}