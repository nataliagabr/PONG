
using UnityEngine;
using UnityEngine.InputSystem;

public class PaddleMovement : MonoBehaviour
{
    public float speed = 5f;

    public bool isPlayer1;

    public float minY = -4f;
    public float maxY = 4f;

    void Update()
    {
        float movement = 0f;

        if (Keyboard.current == null)
            return;

        if (isPlayer1)
        {
            if (Keyboard.current.wKey.isPressed)
                movement = 1f;

            if (Keyboard.current.sKey.isPressed)
                movement = -1f;
        }
        else
        {
            if (Keyboard.current.upArrowKey.isPressed)
                movement = 1f;

            if (Keyboard.current.downArrowKey.isPressed)
                movement = -1f;
        }

        Vector3 position = transform.position;

        position.y += movement * speed * Time.deltaTime;

        position.y = Mathf.Clamp(position.y, minY, maxY);

        transform.position = position;
    }
}


