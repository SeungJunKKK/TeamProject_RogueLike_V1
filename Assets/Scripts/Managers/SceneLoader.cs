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

    private bool m_IsLoading;

    public void LoadScene(string scene, bool showLoading = true)
    {
        if (m_IsLoading)
        {
            return;
        }

        StartCoroutine(LoadRoutine(scene, showLoading));
    }

    private IEnumerator LoadRoutine(string scene, bool showLoading)
    {
        m_IsLoading = true;
        ShowLoadingScreen(showLoading);
        EventBus.Publish(new SceneLoadStartedEvent { SceneName = scene });

        PoolManager.Instance.ClearAll();
        AddressableManager.Instance.UnloadAllAssets();

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
            if (showLoading && m_Slider != null)
            {
                m_Slider.value = Mathf.Clamp01(op.progress / 0.9f);
            }

            if (op.progress >= 0.9f)
            {
                op.allowSceneActivation = true;
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
