using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneManager : MonoBehaviour
{
    [SerializeField] private TitleMenu titleMenu;

    private void OnEnable()
    {
        if (titleMenu != null)
        {
            // UI 이벤트
            titleMenu.OnStartSinglePlayerPressed += HandleStartSinglePlayer;
            titleMenu.OnOptionsPressed += HandleOptions;
            titleMenu.OnQuitPressed += HandleQuit;
        }
    }

    private void OnDisable()
    {
        if (titleMenu != null)
        {
            // 메모리 해제
            titleMenu.OnStartSinglePlayerPressed -= HandleStartSinglePlayer;
            titleMenu.OnOptionsPressed -= HandleOptions;
            titleMenu.OnQuitPressed -= HandleQuit;
        }
    }

    private void HandleStartSinglePlayer()
    {
        Debug.Log("GameScene으로 이동");
        SceneManager.LoadScene("GameScene");
    }

    private void HandleOptions()
    {
        Debug.Log("Options 버튼이 눌림");
        // 추후 옵션 팝업 추가 예정
    }

    private void HandleQuit()
    {
        Debug.Log("게임 종료");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}