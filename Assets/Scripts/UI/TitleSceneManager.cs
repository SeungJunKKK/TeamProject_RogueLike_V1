using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneManager : MonoBehaviour
{
    [SerializeField] private TitleMenu titleMenu;

    private void OnEnable() // 이벤트
    {
        if (titleMenu != null)
        {
            titleMenu.OnStartSinglePlayerPressed += HandleStartSinglePlayer;
            titleMenu.OnFullscreenToggled += HandleFullscreenToggle;
            titleMenu.OnVolumeChanged += HandleVolumeChange;
            titleMenu.OnQuitPressed += HandleQuit;
        }
    }

    private void OnDisable()
    {
        if (titleMenu != null)
        {
            titleMenu.OnStartSinglePlayerPressed -= HandleStartSinglePlayer;
            titleMenu.OnQuitPressed -= HandleQuit;
            titleMenu.OnFullscreenToggled -= HandleFullscreenToggle;
            titleMenu.OnVolumeChanged -= HandleVolumeChange;
        }
    }

    private void HandleStartSinglePlayer() // 게임 시작 - GameScene 로드
    {
        SceneManager.LoadScene("GameScene");
    }

    private void HandleQuit() // 게임 종료
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void HandleFullscreenToggle(bool isFullscreen) // 전체화면 및 창모드 전환
    {
        Debug.Log($"전체화면 설정 변경: {isFullscreen}");
        Screen.fullScreen = isFullscreen;
    }

    private void HandleVolumeChange(float volume) // 소리 줄이기 바
    {
        Debug.Log($"마스터 볼륨 변경: {volume}");
        AudioListener.volume = volume;
    }
}