using UnityEngine;
using UnityEngine.InputSystem;

public class GameStateTest : MonoBehaviour
{
    private void OnEnable()
    {
        
    }

    private void OnDestroy()
    {
        
    }

    private void OnStateChanged(GameStateChangedEvent e)
    {
        Debug.Log($"[GameStateTest] State changed from {e.Previous} to {e.Current}");
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            Debug.Log("[GameStateTest] Call StartGame");
            GameManager.Instance.StartGame();
        }

        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            Debug.Log($"[GameStateTest] Call PauseGame: SetPause({!GameManager.Instance.IsPaused})");
            GameManager.Instance.SetPause(!GameManager.Instance.IsPaused);
        }

        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            Debug.Log("[GameStateTest] Call PlayerDied");
            EventBus.Publish(new PlayerDiedEvent());
        }

        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            Debug.Log($"[GameStateTest] Current GameState: {GameManager.Instance.State} / timeScale: {Time.timeScale}");
        }
    }
}
