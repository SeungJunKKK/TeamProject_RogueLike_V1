using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneManager : MonoBehaviour
{
    [SerializeField] private TitleMenu titleMenu;

    private void OnEnable()
    {
        if (titleMenu != null)
        {
            // UI 이벤트 구독 (Subscribe)
            titleMenu.OnStartSinglePlayerPressed += HandleStartSinglePlayer;
            titleMenu.OnOptionsPressed += HandleOptions;
            titleMenu.OnQuitPressed += HandleQuit;
        }
    }

    private void OnDisable()
    {
        if (titleMenu != null)
        {
            // 메모리 누수 방지를 위한 구독 해제
            titleMenu.OnStartSinglePlayerPressed -= HandleStartSinglePlayer;
            titleMenu.OnOptionsPressed -= HandleOptions;
            titleMenu.OnQuitPressed -= HandleQuit;
        }
    }

    private void HandleStartSinglePlayer()
    {
        Debug.Log("GameScene으로 전환합니다.");
        SceneManager.LoadScene("GameScene");
    }

    private void HandleOptions()
    {
        Debug.Log("Options 버튼이 눌렸습니다. (현재 미구현)");
        // 추후 옵션 팝업을 띄우는 로직 등이 들어올 자리
    }

    private void HandleQuit()
    {
        Debug.Log("게임 프로그램을 종료합니다.");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 에디터 환경에서 작동
#else
        Application.Quit(); // 빌드된 게임에서 작동
#endif
    }
}