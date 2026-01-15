using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [SerializeField] 
    CarHandler carHandler;

    void Update()
    {
        Vector2 input = Vector2.zero;

        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) input.x -= 1;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) input.x += 1;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) input.y += 1;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) input.y -= 1;

            if (kb.rKey.wasPressedThisFrame)
            {
                Time.timeScale = 1.0f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        carHandler.SetInput(input);
    }
}
