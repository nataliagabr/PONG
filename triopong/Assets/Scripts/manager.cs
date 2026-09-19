using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Pontuação")]
    [SerializeField] private TMP_Text scoreP1Text;
    [SerializeField] private TMP_Text scoreP2Text;

    [Header("Configuração da Partida")]
    [SerializeField] private int pontosParaVencer = 3;

    [Header("Bola")]
    [SerializeField] private Transform ball;
    [SerializeField] private Rigidbody2D ballRb;

    [Header("Tela de Vitória")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TMP_Text winText;

    [Header("Reinício")]
    [SerializeField] private float restartDelay = 1f;

    private int scoreP1 = 0;
    private int scoreP2 = 0;

    private Vector3 ballStartPosition;

    private bool partidaEncerrada = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (ball != null)
        {
            ballStartPosition = ball.position;
        }

        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        AtualizarPlacar();
    }

    public void GolP1()
    {
        if (partidaEncerrada)
            return;

        scoreP1++;

        AtualizarPlacar();

        if (scoreP1 >= pontosParaVencer)
        {
            FinalizarPartida("Jogador 1 venceu!");
            return;
        }

        ReiniciarBola();
    }

    public void GolP2()
    {
        if (partidaEncerrada)
            return;

        scoreP2++;

        AtualizarPlacar();

        if (scoreP2 >= pontosParaVencer)
        {
            FinalizarPartida("Jogador 2 venceu!");
            return;
        }

        ReiniciarBola();
    }

    private void AtualizarPlacar()
    {
        if (scoreP1Text != null)
        {
            scoreP1Text.text = scoreP1.ToString();
        }

        if (scoreP2Text != null)
        {
            scoreP2Text.text = scoreP2.ToString();
        }
    }

    private void ReiniciarBola()
    {
        if (ball == null)
            return;

        ball.position = ballStartPosition;

        if (ballRb != null)
        {
            ballRb.linearVelocity = Vector2.zero;
            ballRb.angularVelocity = 0f;
        }

        CancelInvoke(nameof(LiberarBola));
        Invoke(nameof(LiberarBola), restartDelay);
    }

    private void LiberarBola()
    {
        if (partidaEncerrada)
            return;

        if (ballRb == null)
            return;

        float direcaoX = Random.value < 0.5f ? -1f : 1f;
        float direcaoY = Random.Range(-0.5f, 0.5f);

        Vector2 direcao = new Vector2(
            direcaoX,
            direcaoY
        ).normalized;

        ballRb.linearVelocity = direcao * 5f;
    }

    private void FinalizarPartida(string mensagem)
    {
        partidaEncerrada = true;

        CancelInvoke(nameof(LiberarBola));

        if (ballRb != null)
        {
            ballRb.linearVelocity = Vector2.zero;
            ballRb.angularVelocity = 0f;
        }

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        if (winText != null)
        {
            winText.text = mensagem;
        }
    }

    public void ReiniciarPartida()
    {
        CancelInvoke(nameof(LiberarBola));

        scoreP1 = 0;
        scoreP2 = 0;

        partidaEncerrada = false;

        AtualizarPlacar();

        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        if (ball != null)
        {
            ball.position = ballStartPosition;
        }

        if (ballRb != null)
        {
            ballRb.linearVelocity = Vector2.zero;
            ballRb.angularVelocity = 0f;
        }

        Invoke(nameof(LiberarBola), restartDelay);
    }
}