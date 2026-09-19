using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PaddleMovement : MonoBehaviour
{
    [Header("Movimentação")]
    [SerializeField] private float speed = 5f;

    [Header("Jogador")]
    [SerializeField] private bool isPlayer1;

    [Header("Limites")]
    [SerializeField] private float minY = -4f;
    [SerializeField] private float maxY = 4f;

    private Rigidbody2D rb;
    private float movement;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void Update()
    {
        movement = 0f;

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
    }

    private void FixedUpdate()
    {
        Vector2 position = rb.position;

        position.y += movement * speed * Time.fixedDeltaTime;

        position.y = Mathf.Clamp(position.y, minY, maxY);

        rb.MovePosition(position);
    }
}