using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneManager : MonoBehaviour
{
    [SerializeField] private TitleMenu titleMenu;
    [SerializeField] private string m_BackgroundAddress = "MainMenuBackground"; // Addressable 적용

    private void Start()
    {
        // 씬이 시작되면 어드레서블 매니저를 통해 배경 이미지를 비동기로 로드
        AddressableManager.Instance.LoadAssetAsync<Sprite>(m_BackgroundAddress, (sprite) =>
        {
            if (sprite != null && titleMenu != null)
            {
                // 로드가 완료되면 TitleScene에 적용
                titleMenu.SetBackgroundImage(sprite);
            }
        });
        
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGM(SoundManager.Instance.LobbyBgm);
        }
    }

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

    private void HandleStartSinglePlayer() // 게임 시작 -> 플레이어 선택 씬으로 이동
    {
        // 게임 씬으로 넘어가기 전에 사용 완료한 배경 이미지 메모리 해제
        AddressableManager.Instance.UnloadAsset(m_BackgroundAddress);
       
        SceneManager.LoadScene("PlayerSelectScene");
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
        if (isFullscreen)
        {
            // 체크됨 - 모니터 최대 해상도로 전체화면 전환
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.FullScreenWindow);
        }
        else
        {
            // false- 창모드
            Screen.SetResolution(1280, 720, false);
        }
    }

    private void HandleVolumeChange(float volume) // 소리 줄이기 바
    {
        Debug.Log($"마스터 볼륨 변경: {volume}");
        AudioListener.volume = volume;
    }
}