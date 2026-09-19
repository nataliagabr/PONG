using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BallMovement : MonoBehaviour
{
    [Header("Configuração")]
    [SerializeField] private float speed = 5f;

    [Header("Ricochete da Raquete")]
    [SerializeField] private float maxBounceAngle = 60f;

    [Header("Ricochete das Paredes")]
    [SerializeField] private float minimumVerticalAngle = 0.2f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.freezeRotation = true;

        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Start()
    {
        LancarBola();
    }

    private void LancarBola()
    {
        float directionX = Random.value < 0.5f ? -1f : 1f;
        float directionY = Random.Range(-0.5f, 0.5f);

        Vector2 direcao = new Vector2(directionX, directionY).normalized;

        rb.linearVelocity = direcao * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contactCount == 0)
            return;

        // =========================
        // COLISÃO COM RAQUETE
        // =========================

        if (collision.gameObject.CompareTag("Player"))
        {
            RicocheteRaquete(collision.transform);
            return;
        }

        // =========================
        // COLISÃO COM PAREDE
        // =========================

        Vector2 normal = collision.GetContact(0).normal;

        Vector2 velocidadeAtual = rb.linearVelocity.normalized;

        Vector2 novaDirecao = Vector2.Reflect(
            velocidadeAtual,
            normal
        );

        // Evita que a bola fique praticamente
        // paralela à parede.
        if (Mathf.Abs(novaDirecao.y) < minimumVerticalAngle)
        {
            float sinal = novaDirecao.y >= 0f ? 1f : -1f;

            novaDirecao.y = minimumVerticalAngle * sinal;

            novaDirecao.Normalize();
        }

        rb.linearVelocity = novaDirecao * speed;
    }

    private void RicocheteRaquete(Transform raquete)
    {
        Collider2D colliderRaquete = raquete.GetComponent<Collider2D>();

        if (colliderRaquete == null)
            return;

        // Posição da bola em relação ao centro da raquete.
        float diferencaY = transform.position.y - raquete.position.y;

        // Metade da altura da raquete.
        float metadeAltura = colliderRaquete.bounds.extents.y;

        if (metadeAltura <= 0f)
            return;

        // Converte para um valor entre -1 e 1.
        //
        // -1 = parte inferior
        //  0 = centro
        // +1 = parte superior

        float percentual = diferencaY / metadeAltura;

        percentual = Mathf.Clamp(percentual, -1f, 1f);

        // Calcula o ângulo baseado no ponto de impacto.
        float angulo = percentual * maxBounceAngle;

        float radianos = angulo * Mathf.Deg2Rad;

        // Descobre para qual lado a bola deve ir.
        float direcaoX;

        if (transform.position.x < raquete.position.x)
        {
            // Bola está à esquerda da raquete.
            direcaoX = -1f;
        }
        else
        {
            // Bola está à direita da raquete.
            direcaoX = 1f;
        }

        Vector2 novaDirecao = new Vector2(
            direcaoX * Mathf.Cos(radianos),
            Mathf.Sin(radianos)
        );

        novaDirecao.Normalize();

        rb.linearVelocity = novaDirecao * speed;
    }
}