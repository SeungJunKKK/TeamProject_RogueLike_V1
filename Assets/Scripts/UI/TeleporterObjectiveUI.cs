using UnityEngine;
using TMPro;

public class TeleporterObjectiveUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_ObjectiveText;
    private Vector3 m_OriginalScale;

    private void Awake()
    {
        EventBus.Subscribe<TeleporterUpdateEvent>(OnTeleporterUpdate);
        EventBus.Subscribe<SceneLoadCompletedEvent>(OnSceneLoadCompleted);

        if (m_ObjectiveText != null)
        {
            m_ObjectiveText.text = "";
        }
        m_OriginalScale = transform.localScale;
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<TeleporterUpdateEvent>(OnTeleporterUpdate);
        EventBus.Unsubscribe<SceneLoadCompletedEvent>(OnSceneLoadCompleted);
    }

    private void OnSceneLoadCompleted(SceneLoadCompletedEvent e)
    {
        if (m_ObjectiveText != null)
        {
            m_ObjectiveText.text = "";
        }
    }

    //Flip 방지 
    private void LateUpdate()
    {
        PlayerController player = GetComponentInParent<PlayerController>();

        if (player != null)
        {
            float playerSign = Mathf.Sign(player.transform.localScale.x);

            transform.localScale = new Vector3(m_OriginalScale.x * playerSign, m_OriginalScale.y, m_OriginalScale.z);
        }

        transform.rotation = Quaternion.identity;
    }

    private void OnTeleporterUpdate(TeleporterUpdateEvent e)
    {
        if (m_ObjectiveText == null) return;

        switch (e.State)
        {
            case ETeleporterState.Charging:
                m_ObjectiveText.text = $"<color=#00FFFF>Charging Teleporter... {e.ProgressPercent:F0}%</color>\n<size=80%>Time Left: {e.TimeLeft:F1}s</size>";
                break;

            case ETeleporterState.WaitingForClear:
                string bossStatus = e.IsBossDead ? "<color=#00FF00>Defeated</color>" : "<color=#FF0000>Alive</color>";
                m_ObjectiveText.text = $"<color=#FFA500>Eliminate Remaining</color>\n<size=80%>Enemies Left: {e.RemainingEnemies} | Boss: {bossStatus}</size>";
                break;

            case ETeleporterState.Cleared:
                m_ObjectiveText.text = "<color=#00FF00>Teleporter Activated!</color>\n<size=80%>[Interact to enter next stage]</size>";
                break;

            default:
                m_ObjectiveText.text = "";
                break;
        }
    }
}