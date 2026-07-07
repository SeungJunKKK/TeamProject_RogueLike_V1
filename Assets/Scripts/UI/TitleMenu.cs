using System;
using UnityEngine;
using UnityEngine.UI;

public class TitleMenu : MonoBehaviour
{
    [SerializeField] private Button startSinglePlayerButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;

    // 가이드라인에 맞춘 Action 이벤트 정의 (이벤트 방출)
    public event Action OnStartSinglePlayerPressed;
    public event Action OnOptionsPressed;
    public event Action OnQuitPressed;

    private void Awake()
    {
        // 버튼 리스너 연결
        if (startSinglePlayerButton != null)
            startSinglePlayerButton.onClick.AddListener(() => OnStartSinglePlayerPressed?.Invoke());

        if (optionsButton != null)
            optionsButton.onClick.AddListener(() => OnOptionsPressed?.Invoke());

        if (quitButton != null)
            quitButton.onClick.AddListener(() => OnQuitPressed?.Invoke());
    }
}