using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LoadSceneTest : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log($"[Test] Current Scene: {SceneManager.GetActiveScene().name}");

            string target = SceneManager.GetActiveScene().name == "BootScene" 
                                                                ? "TestScene" 
                                                                : "BootScene";

            SceneLoader.Instance.LoadScene(target);
        }
    }
}
