using System;
using UnityEngine;
using UnityEngine.UI;

public class TitleMenu : MonoBehaviour
{
    [Header("Title Menu")]
    [SerializeField] private Image backgroundImage;

    [Header("Main Menu Buttons")]
    [SerializeField] private Button startSinglePlayerButton;
    [SerializeField] private Button itemLogButton;
    [SerializeField] private Button monsterLogButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;

    [Header("Item Log Popup UI")]
    [SerializeField] private GameObject itemLogPopup;
    [SerializeField] private Button itemLogBackButton;

    [Header("Monster Log Popup UI")]
    [SerializeField] private GameObject monsterLogPopup;
    [SerializeField] private Button monsterLogBackButton;

    [Header("Options Popup UI")]
    [SerializeField] private GameObject optionsPopup;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Button backButton;

    public event Action OnStartSinglePlayerPressed;
    //public event Action OnOptionsPressed;
    public event Action OnQuitPressed;
    public event Action<bool> OnFullscreenToggled;
    public event Action<float> OnVolumeChanged;

    private void Awake()
    {
        // 메인 메뉴 버튼 연결
        if (startSinglePlayerButton != null)
            startSinglePlayerButton.onClick.AddListener(() => OnStartSinglePlayerPressed?.Invoke());

        if (quitButton != null)
            quitButton.onClick.AddListener(() => OnQuitPressed?.Invoke());

        // Item Log 팝업
        if (itemLogButton != null)
            itemLogButton.onClick.AddListener(OpenItemLogPopup);

        if (itemLogBackButton != null)
            itemLogBackButton.onClick.AddListener(CloseItemLogPopup);

        // Monster Log 팝업
        if (monsterLogButton != null)
            monsterLogButton.onClick.AddListener(OpenMonsterLog);

        if (monsterLogBackButton != null)
            monsterLogBackButton.onClick.AddListener(CloseMonsterLog);

        // Options 팝업
        if (optionsButton != null)
            optionsButton.onClick.AddListener(OpenOptionsPopup);

        if (backButton != null)
            backButton.onClick.AddListener(CloseOptionsPopup);

        // 화면 크기 조정 및 소리 설정
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener((isOn) => OnFullscreenToggled?.Invoke(isOn));

        if (volumeSlider != null)
            volumeSlider.onValueChanged.AddListener((value) => OnVolumeChanged?.Invoke(value));
    }

    private void Start() // 현재 설정값으로 UI 업데이트
    {
        if (fullscreenToggle != null)
            fullscreenToggle.isOn = Screen.fullScreen;

        if (volumeSlider != null)
            volumeSlider.value = AudioListener.volume;
    }

    public void SetBackgroundImage(Sprite sprite)
    {
        if (backgroundImage != null)
        {
            backgroundImage.sprite = sprite;
        }
    }

    #region Item Log Popup
    public void OpenItemLogPopup()
    {
        if (itemLogPopup != null)
            itemLogPopup.SetActive(true);
    }

    public void CloseItemLogPopup()
    {
        if (itemLogPopup != null)
            itemLogPopup.SetActive(false);
    }
    #endregion

    #region Monster Log Popup
    public void OpenMonsterLog()
    {
        if (monsterLogPopup != null) monsterLogPopup.SetActive(true);
        if (itemLogPopup != null) itemLogPopup.SetActive(false);
    }

    public void CloseMonsterLog()
    {
        Debug.Log("Monster Log Back Button Clicked!");
        if (monsterLogPopup != null) monsterLogPopup.SetActive(false);
    }
    #endregion

    #region Options Popup
    public void OpenOptionsPopup()
    {
        optionsPopup.SetActive(true);
    }

    public void CloseOptionsPopup()
    {
        optionsPopup.SetActive(false);
    }
    #endregion
}