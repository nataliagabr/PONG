

using UnityEngine;

public class BallMovement : MonoBehaviour
{
    public float speed = 5f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        LaunchBall();
    }

    void LaunchBall()
    {
        float directionX = Random.value < 0.5f ? -1f : 1f;
        float directionY = Random.Range(-0.5f, 0.5f);

        Vector2 direction = new Vector2(directionX, directionY).normalized;

        rb.linearVelocity = direction * speed;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Vector2 direction = rb.linearVelocity.normalized;

            rb.linearVelocity = direction * speed;
        }
    }
}


