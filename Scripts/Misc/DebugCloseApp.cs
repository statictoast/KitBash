using UnityEngine.InputSystem;
using UnityEngine;

public class DebugCloseApp : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        Keyboard keyboard = InputSystem.GetDevice<Keyboard>();
        if(keyboard.escapeKey.isPressed)
        {
            Application.Quit();
        }
    }
}
