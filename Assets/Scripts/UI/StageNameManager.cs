using UnityEngine;

public class StageManager : MonoBehaviour
{
    [System.Serializable]
    public struct StageData
    {
        public string sceneName; // SceneLoader에서 전달받을 씬 이름
        public string stageDisplayName; // 표시될 텍스트 (예: Dried Lake)
    }

    [Header("참조")]
    [SerializeField] private StageNameDisplay stageNameDisplay;

    [Header("스테이지 정보 등록")]
    [SerializeField] private StageData[] stageDataList;

    private void OnEnable()
    {
        EventBus.Subscribe<SceneLoadCompletedEvent>(OnSceneLoadCompleted);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<SceneLoadCompletedEvent>(OnSceneLoadCompleted);
    }

    private void OnSceneLoadCompleted(SceneLoadCompletedEvent evt)
    {
        foreach (var stage in stageDataList)
        {
            if (stage.sceneName == evt.SceneName)
            {
                if (stageNameDisplay != null)
                {
                    stageNameDisplay.ShowStageName(stage.stageDisplayName);
                }
                break;
            }
        }
    }
}