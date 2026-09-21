using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PaddleMovement : MonoBehaviour
{
    [Header("Movimentação")]
    [SerializeField] private float speed = 5f;

    [Header("Jogador")]
    [SerializeField] private int playerID = 1;

    [Header("Limites")]
    [SerializeField] private float minY = -4f;
    [SerializeField] private float maxY = 4f;

    [Header("Rede")]
    [SerializeField] private UDPClient udpClient;

    private Rigidbody2D rb;

    private float movement;

    private void Awake()
    {
        rb =
            GetComponent<Rigidbody2D>();

        if (udpClient == null)
        {
            udpClient =
                FindFirstObjectByType<UDPClient>();
        }

        rb.bodyType =
            RigidbodyType2D.Kinematic;

        rb.gravityScale = 0f;

        rb.freezeRotation = true;
    }

    private void Update()
    {
        movement = 0f;

        if (udpClient == null)
            return;

        // =====================================================
        // SOMENTE O DONO DESTA RAQUETE PODE CONTROLÁ-LA
        // =====================================================

        if (udpClient.GetPlayerID() != playerID)
            return;

        if (Keyboard.current == null)
            return;

        // =====================================================
        // PLAYER 1
        // =====================================================

        if (playerID == 1)
        {
            if (Keyboard.current.wKey.isPressed)
            {
                movement = 1f;
            }

            if (Keyboard.current.sKey.isPressed)
            {
                movement = -1f;
            }
        }

        // =====================================================
        // PLAYER 2
        // =====================================================

        else if (playerID == 2)
        {
            if (Keyboard.current.upArrowKey.isPressed)
            {
                movement = 1f;
            }

            if (Keyboard.current.downArrowKey.isPressed)
            {
                movement = -1f;
            }
        }
    }

    private void FixedUpdate()
    {
        if (udpClient == null)
            return;

        // Só movimenta se esta raquete
        // pertencer ao jogador local.
        if (udpClient.GetPlayerID() != playerID)
            return;

        Vector2 position =
            rb.position;

        position.y +=
            movement *
            speed *
            Time.fixedDeltaTime;

        position.y =
            Mathf.Clamp(
                position.y,
                minY,
                maxY
            );

        rb.MovePosition(position);
    }
}
