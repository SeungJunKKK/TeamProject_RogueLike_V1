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

    public void LoadScene(string scene)
    {
        if (m_IsLoading)
            return;

        StartCoroutine(LoadRoutine(scene));
    }

    private IEnumerator LoadRoutine(string scene)
    {
        m_IsLoading = true;
        m_LoadingScreen.SetActive(true);

        PoolManager.Instance.ClearAll();

        AsyncOperation op = SceneManager.LoadSceneAsync(scene);

        if (op == null)
        {
            Debug.LogError($"[SceneLoader] Failed to load scene: {scene}");
            m_LoadingScreen.SetActive(false);
            m_IsLoading = false;
            yield break;
        }

        op.allowSceneActivation = false;
        // test
        //yield return new WaitForSecondsRealtime(2f);

        while (!op.isDone)
        {
            m_Slider.value = Mathf.Clamp01(op.progress / 0.9f);

            if (op.progress >= 0.9f)
                op.allowSceneActivation = true;

            yield return null; 
        }

        m_LoadingScreen.SetActive(false);
        m_IsLoading = false;
    }

}
