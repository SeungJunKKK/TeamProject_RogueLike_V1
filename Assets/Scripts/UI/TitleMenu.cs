using System;
using UnityEngine;
using UnityEngine.UI;

public class TitleMenu : MonoBehaviour
{
    [SerializeField] private Button startSinglePlayerButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;

    public event Action OnStartSinglePlayerPressed;
    public event Action OnOptionsPressed;
    public event Action OnQuitPressed;

    private void Awake()
    {
        if (startSinglePlayerButton != null)
            startSinglePlayerButton.onClick.AddListener(() => OnStartSinglePlayerPressed?.Invoke());

        if (optionsButton != null)
            optionsButton.onClick.AddListener(() => OnOptionsPressed?.Invoke());

        if (quitButton != null)
            quitButton.onClick.AddListener(() => OnQuitPressed?.Invoke());
    }
}