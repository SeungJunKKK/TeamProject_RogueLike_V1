using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : Singleton<SceneLoader>
{
    [SerializeField]
    private GameObject m_LoadingScreen;
    [SerializeField]
    private Slider m_Slider;

    [SerializeField] private float m_FillSpeed = 1.5f;

    private bool m_IsLoading;

    public void LoadScene(string scene, bool showLoading = true, string preloadLabel = null)
    {
        if (m_IsLoading)
        {
            return;
        }

        StartCoroutine(LoadRoutine(scene, showLoading, preloadLabel));
    }

    private IEnumerator LoadRoutine(string scene, bool showLoading, string preloadLabel)
    {
        m_IsLoading = true;
        ShowLoadingScreen(showLoading);
        EventBus.Publish(new SceneLoadStartedEvent { SceneName = scene });

        PoolManager.Instance.ClearAll();
        AddressableManager.Instance.UnloadAllAssets();

        if (!string.IsNullOrEmpty(preloadLabel))
        {
            var preload = AddressableManager.Instance.PreloadLabelAsync(preloadLabel);
            while (!preload.IsDone)
            {
                if (showLoading && m_Slider != null)
                {
                    m_Slider.value = preload.PercentComplete * 0.5f;
                }
                yield return null;
            }
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(scene);

        if (op == null)
        {
            Debug.LogError($"[SceneLoader] Failed to load scene: {scene}");
            ShowLoadingScreen(false);
            m_IsLoading = false;
            yield break;
        }

        op.allowSceneActivation = false;
        // test
        // yield return new WaitForSecondsRealtime(2f);

        while (!op.isDone)
        {
            float target = (op.progress < 0.9f)
                            ? 0.5f + (op.progress / 0.9f) * 0.5f
                            : 1f;

            if (showLoading && m_Slider != null)
            {
                m_Slider.value = Mathf.MoveTowards(m_Slider.value, target, Time.unscaledDeltaTime * m_FillSpeed);

                if (op.progress >= 0.9f && m_Slider.value >= 0.999f)
                {
                    op.allowSceneActivation = true;
                }
            }
            else if (op.progress >= 0.9f)
            {
                op.allowSceneActivation = true;   // 로딩화면 없으면 즉시
            }

            yield return null;
        }

        EventBus.Publish(new SceneLoadCompletedEvent { SceneName = scene });
        ShowLoadingScreen(false);
        m_IsLoading = false;
    }

    private void ShowLoadingScreen(bool on)
    {
        if (m_LoadingScreen != null)
        {
            m_LoadingScreen.SetActive(on);
        }
    }
}
