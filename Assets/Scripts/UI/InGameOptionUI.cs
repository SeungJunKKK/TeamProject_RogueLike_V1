using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InGameOptionUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pauseMenuPanel; // 수직 버튼 목록 패널
    [SerializeField] private GameObject settingsPanel; // 볼륨/전체화면 설정 패널

    [Header("Top Right UI Button")]
    [SerializeField] private Button pauseIconButton;   // 화면 우측 상단 설정 아이콘 버튼

    [Header("Pause Menu Buttons")]
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitToMenuButton;
    [SerializeField] private Button quitToDesktopButton;

    [Header("Settings Panel Buttons")]
    [SerializeField] private Button settingsBackButton;

    [Header("Title Scene Name")]
    [SerializeField] private string titleSceneName = "TitleScene";

    private PlayerController m_Player;

    private void Awake()
    {
        // 우측 상단 설정 아이콘 이벤트
        if (pauseIconButton != null) pauseIconButton.onClick.AddListener(OpenPauseMenu);

        // 일시정지 메뉴 버튼 이벤트
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
        if (quitToMenuButton != null) quitToMenuButton.onClick.AddListener(QuitToMenu);
        if (quitToDesktopButton != null) quitToDesktopButton.onClick.AddListener(QuitToDesktop);

        // 설정 패널 버튼 이벤트
        if (settingsBackButton != null) settingsBackButton.onClick.AddListener(CloseSettings);
    }

    private void Start()
    {
        m_Player = Object.FindAnyObjectByType<PlayerController>();

        CloseAllPanels();
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Settings 패널이 열려있다면 메인 일시정지 메뉴로 복귀
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                CloseSettings();
            }
            else
            {
                TogglePauseMenu();
            }
        }
    }

    public void TogglePauseMenu()
    {
        if (pauseMenuPanel == null) return;

        bool isCurrentlyActive = pauseMenuPanel.activeSelf;

        if (isCurrentlyActive)
        {
            ResumeGame();
        }
        else
        {
            OpenPauseMenu();
        }
    }

    public void OpenPauseMenu()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        CloseAllPanels();
        Time.timeScale = 1f;
    }

    public void OpenSettings()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(titleSceneName);
    }

    public void QuitToDesktop()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void CloseAllPanels()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}