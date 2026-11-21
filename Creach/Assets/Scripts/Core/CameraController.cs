using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public float panSpeed = 20f;   // units per second
    public float edgeSize = 10f;   // pixels from the screen edge that start scrolling

    void Update()
    {
        Mouse mouse = Mouse.current;
        // Don't scroll while you're clicking around in another window
        if(mouse == null || !Application.isFocused) return;

        Vector2 pos = mouse.position.ReadValue();
        Vector2 input = Vector2.zero;

        // <= and >= also catch the mouse being past the edge
        if(pos.x <= edgeSize) input.x = -1;
        else if(pos.x >= Screen.width - edgeSize) input.x = 1;
        if(pos.y <= edgeSize) input.y = -1;
        else if(pos.y >= Screen.height - edgeSize) input.y = 1;

        if(input == Vector2.zero) return;

        // Screen directions flattened onto the ground,
        // so "up" on screen moves across the map, not into it
        Vector3 right = transform.right;
        right.y = 0;
        right.Normalize();
        Vector3 forward = transform.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 move = (right * input.x + forward * input.y).normalized;
        transform.position += move * panSpeed * Time.deltaTime;
    }
}
