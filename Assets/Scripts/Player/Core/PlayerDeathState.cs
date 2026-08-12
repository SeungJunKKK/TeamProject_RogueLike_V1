using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeathState : IState
{
    private readonly PlayerController m_Player;
    private readonly float m_DeathDuration = 5.0f; 
    private float m_Timer;
    private bool m_IsSceneTransitionTriggered;

    public PlayerDeathState(PlayerController player)
    {
        m_Player = player;
    }

    public void Enter()
    {
        m_Timer = 0f;
        m_IsSceneTransitionTriggered = false;

        if (m_Player.Rb != null)
        {
            m_Player.Rb.linearVelocity = Vector2.zero;
            m_Player.Rb.simulated = false;
        }

        if (m_Player.Anim != null)
        {
            m_Player.Anim.Play("Death");
        }

        Debug.Log("<color=red>[Player] 사망 상태 </color>");
        EventBus.Publish(new PlayerDiedEvent());
    }

    public void Update()
    {
        m_Timer += Time.deltaTime;

        // 일정시간 지난 후 라운드 종료 및 기본 정보 저장 필요.. 
        if (m_Timer >= m_DeathDuration && !m_IsSceneTransitionTriggered)
        {
            m_IsSceneTransitionTriggered = true;
            ReturnToCharacterSelect();
        }
    }

    public void Exit()
    {
        
    }

    private void ReturnToCharacterSelect()
    {
        string targetSceneName = "PlayerSelectScene";
        Debug.Log($"[Player] 씬 전환: {targetSceneName}으로 이동합니다.");
        SceneManager.LoadScene(targetSceneName);
    }
}