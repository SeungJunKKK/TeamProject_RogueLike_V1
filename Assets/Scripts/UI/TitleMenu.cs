using System;
using UnityEngine;
using UnityEngine.UI;

public class TitleMenu : MonoBehaviour
{
    [Header("Title Menu")]
    [SerializeField] private Image backgroundImage;

    [Header("Main Menu Buttons")]
    [SerializeField] private Button startSinglePlayerButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;

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
        startSinglePlayerButton.onClick.AddListener(() => OnStartSinglePlayerPressed?.Invoke());
        quitButton.onClick.AddListener(() => OnQuitPressed?.Invoke());

        // Options 누르면 팝업 열기
        optionsButton.onClick.AddListener(OpenOptionsPopup);

        // Back 누르면 팝업 닫기
        backButton.onClick.AddListener(CloseOptionsPopup);

        // 화면 크기 조정 및 소리 설정
        fullscreenToggle.onValueChanged.AddListener((isOn) => OnFullscreenToggled?.Invoke(isOn));
        volumeSlider.onValueChanged.AddListener((value) => OnVolumeChanged?.Invoke(value));
    }

    private void Start() // 현재 설정값으로 UI 업데이트
    {
        fullscreenToggle.isOn = Screen.fullScreen;
        volumeSlider.value = AudioListener.volume;
    }

    public void SetBackgroundImage(Sprite sprite)
    {
        if (backgroundImage != null)
        {
            backgroundImage.sprite = sprite;
        }
    }

    public void OpenOptionsPopup()
    {
        optionsPopup.SetActive(true);
    }

    public void CloseOptionsPopup()
    {
        optionsPopup.SetActive(false);
    }
}