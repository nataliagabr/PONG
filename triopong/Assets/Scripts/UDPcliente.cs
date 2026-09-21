using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UDPClient : MonoBehaviour
{
    [Header("Conexão")]
    [SerializeField] private string ipServidor = "127.0.0.1";
    [SerializeField] private int porta = 7777;

    [Header("Raquete Local")]
    [SerializeField] private Transform minhaRaquete;

    [Header("Raquete Remota")]
    [SerializeField] private Transform raqueteAdversaria;

    [Header("Bola")]
    [SerializeField] private Transform bola;
    [SerializeField] private Rigidbody2D bolaRb;

    private UdpClient cliente;
    private IPEndPoint enderecoServidor;

    private int playerID = 0;

    private bool conectado = false;
    private bool encerrando = false;
    private bool partidaPronta = false;

    private readonly Queue<string> mensagensRecebidas =
        new Queue<string>();

    private readonly object lockMensagens =
        new object();

    private float tempoEnvioPosicao = 0f;
    private float intervaloEnvioPosicao = 0.033f;

    private float tempoEnvioBola = 0f;
    private float intervaloEnvioBola = 0.033f;

    private Vector2 ultimaPosicaoBola;
    private Vector2 ultimaVelocidadeBola;

    private void Start()
    {
        Conectar();
    }

    private void Update()
    {
        ProcessarMensagensRecebidas();

        if (!conectado || encerrando)
            return;

        if (playerID == 0)
            return;

        // -----------------------------------------------------
        // POSIÇÃO DA PRÓPRIA RAQUETE
        // -----------------------------------------------------

        if (minhaRaquete != null)
        {
            tempoEnvioPosicao += Time.deltaTime;

            if (tempoEnvioPosicao >= intervaloEnvioPosicao)
            {
                tempoEnvioPosicao = 0f;

                EnviarPosicaoRaquete();
            }
        }

        // -----------------------------------------------------
        // BOLA
        // -----------------------------------------------------

        // Player 1 é autoridade da bola.
        if (playerID == 1 &&
            partidaPronta &&
            bola != null &&
            bolaRb != null)
        {
            tempoEnvioBola += Time.deltaTime;

            if (tempoEnvioBola >= intervaloEnvioBola)
            {
                tempoEnvioBola = 0f;

                EnviarEstadoBola();
            }
        }
    }

    private void Conectar()
    {
        try
        {
            enderecoServidor =
                new IPEndPoint(
                    IPAddress.Parse(ipServidor),
                    porta
                );

            cliente =
                new UdpClient();

            cliente.Connect(
                enderecoServidor
            );

            conectado = true;
            encerrando = false;

            Debug.Log(
                "Cliente UDP conectado ao servidor " +
                ipServidor +
                ":" +
                porta
            );

            cliente.BeginReceive(
                ReceberMensagem,
                null
            );

            EnviarMensagem("HELLO");
        }
        catch (Exception erro)
        {
            conectado = false;

            Debug.LogError(
                "Erro ao conectar ao servidor: " +
                erro.Message
            );
        }
    }

    private void EnviarPosicaoRaquete()
    {
        if (minhaRaquete == null)
            return;

        float posicaoY =
            minhaRaquete.position.y;

        string mensagem =
            "POS:" +
            posicaoY.ToString(
                "F3",
                CultureInfo.InvariantCulture
            );

        EnviarMensagem(mensagem);
    }

    private void EnviarEstadoBola()
    {
        if (bola == null || bolaRb == null)
            return;

        Vector2 posicao =
            bola.position;

        Vector2 velocidade =
            bolaRb.linearVelocity;

        string mensagem =
            "BALL:" +
            posicao.x.ToString(
                "F3",
                CultureInfo.InvariantCulture
            ) +
            ":" +
            posicao.y.ToString(
                "F3",
                CultureInfo.InvariantCulture
            ) +
            ":" +
            velocidade.x.ToString(
                "F3",
                CultureInfo.InvariantCulture
            ) +
            ":" +
            velocidade.y.ToString(
                "F3",
                CultureInfo.InvariantCulture
            );

        EnviarMensagem(mensagem);
    }

    public void EnviarPontuacao(
        int scoreP1,
        int scoreP2)
    {
        if (!conectado ||
            encerrando)
        {
            return;
        }

        if (playerID != 1)
            return;

        string mensagem =
            "SCORE:" +
            scoreP1 +
            ":" +
            scoreP2;

        EnviarMensagem(mensagem);

        Debug.Log(
            "Pontuação enviada: P1 " +
            scoreP1 +
            " x P2 " +
            scoreP2
        );
    }

    public void EnviarReinicio()
    {
        if (!conectado ||
            encerrando)
        {
            return;
        }

        if (playerID != 1)
            return;

        EnviarMensagem("RESTART");
    }

    private void EnviarMensagem(
        string mensagem)
    {
        if (cliente == null ||
            encerrando)
        {
            return;
        }

        try
        {
            byte[] dados =
                Encoding.UTF8.GetBytes(
                    mensagem
                );

            cliente.Send(
                dados,
                dados.Length
            );
        }
        catch (SocketException)
        {
            if (!encerrando)
            {
                Debug.LogWarning(
                    "Conexão UDP encerrada."
                );
            }
        }
        catch (ObjectDisposedException)
        {
        }
        catch (Exception erro)
        {
            if (!encerrando)
            {
                Debug.LogError(
                    "Erro ao enviar mensagem: " +
                    erro.Message
                );
            }
        }
    }

    private void ReceberMensagem(
        IAsyncResult resultado)
    {
        if (cliente == null ||
            encerrando)
        {
            return;
        }

        try
        {
            IPEndPoint endereco =
                new IPEndPoint(
                    IPAddress.Any,
                    0
                );

            byte[] dados =
                cliente.EndReceive(
                    resultado,
                    ref endereco
                );

            if (dados != null &&
                dados.Length > 0)
            {
                string mensagem =
                    Encoding.UTF8.GetString(
                        dados
                    );

                lock (lockMensagens)
                {
                    mensagensRecebidas.Enqueue(
                        mensagem
                    );
                }
            }

            if (cliente != null &&
                conectado &&
                !encerrando)
            {
                cliente.BeginReceive(
                    ReceberMensagem,
                    null
                );
            }
        }
        catch (ObjectDisposedException)
        {
        }
        catch (SocketException)
        {
        }
        catch (Exception erro)
        {
            if (!encerrando)
            {
                Debug.LogError(
                    "Erro ao receber mensagem: " +
                    erro.Message
                );
            }
        }
    }

    private void ProcessarMensagensRecebidas()
    {
        while (true)
        {
            string mensagem = null;

            lock (lockMensagens)
            {
                if (mensagensRecebidas.Count == 0)
                    break;

                mensagem =
                    mensagensRecebidas.Dequeue();
            }

            if (!string.IsNullOrEmpty(mensagem))
            {
                ProcessarMensagem(mensagem);
            }
        }
    }

    private void ProcessarMensagem(
        string mensagem)
    {
        Debug.Log(
            "Mensagem recebida: " +
            mensagem
        );

        // -----------------------------------------------------
        // ID DO JOGADOR
        // -----------------------------------------------------

        if (mensagem.StartsWith("ID:"))
        {
            string valor =
                mensagem.Substring(3);

            if (int.TryParse(
                valor,
                out int novoID))
            {
                playerID = novoID;

                Debug.Log(
                    "Meu Player ID é: " +
                    playerID
                );
            }

            return;
        }

        // -----------------------------------------------------
        // PARTIDA PRONTA
        // -----------------------------------------------------

        if (mensagem == "READY")
        {
            partidaPronta = true;

            Debug.Log(
                "Os dois jogadores estão conectados!"
            );

            return;
        }

        // -----------------------------------------------------
        // PARTIDA CHEIA
        // -----------------------------------------------------

        if (mensagem == "FULL")
        {
            conectado = false;

            Debug.LogError(
                "Servidor está cheio."
            );

            return;
        }

        // -----------------------------------------------------
        // MENSAGENS DE OUTRO JOGADOR
        // -----------------------------------------------------

        if (mensagem.StartsWith("P1:"))
        {
            ProcessarMensagemDoJogador(
                1,
                mensagem.Substring(3)
            );

            return;
        }

        if (mensagem.StartsWith("P2:"))
        {
            ProcessarMensagemDoJogador(
                2,
                mensagem.Substring(3)
            );

            return;
        }
    }

    private void ProcessarMensagemDoJogador(
        int jogador,
        string mensagem)
    {
        // -----------------------------------------------------
        // POSIÇÃO DA RAQUETE
        // -----------------------------------------------------

        if (mensagem.StartsWith("POS:"))
        {
            // Se a mensagem é da própria máquina,
            // não precisamos aplicá-la novamente.
            if (jogador == playerID)
                return;

            string valor =
                mensagem.Substring(4);

            if (float.TryParse(
                valor,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out float posicaoY))
            {
                if (raqueteAdversaria != null)
                {
                    Vector3 posicao =
                        raqueteAdversaria.position;

                    posicao.y = posicaoY;

                    raqueteAdversaria.position =
                        posicao;
                }
            }

            return;
        }

        // -----------------------------------------------------
        // BOLA
        // -----------------------------------------------------

        if (mensagem.StartsWith("BALL:"))
        {
            // Apenas Player 2 recebe a bola
            // enviada pelo Player 1.
            if (playerID != 2)
                return;

            string dados =
                mensagem.Substring(5);

            string[] partes =
                dados.Split(':');

            if (partes.Length < 4)
                return;

            bool sucessoX =
                float.TryParse(
                    partes[0],
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out float x
                );

            bool sucessoY =
                float.TryParse(
                    partes[1],
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out float y
                );

            bool sucessoVX =
                float.TryParse(
                    partes[2],
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out float vx
                );

            bool sucessoVY =
                float.TryParse(
                    partes[3],
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out float vy
                );

            if (!sucessoX ||
                !sucessoY ||
                !sucessoVX ||
                !sucessoVY)
            {
                return;
            }

            ultimaPosicaoBola =
                new Vector2(x, y);

            ultimaVelocidadeBola =
                new Vector2(vx, vy);

            if (bola != null)
            {
                Vector3 posicao =
                    bola.position;

                posicao.x = x;
                posicao.y = y;

                bola.position =
                    posicao;
            }

            if (bolaRb != null)
            {
                bolaRb.linearVelocity =
                    ultimaVelocidadeBola;
            }

            return;
        }

        // -----------------------------------------------------
        // SCORE
        // -----------------------------------------------------

        if (mensagem.StartsWith("SCORE:"))
        {
            string dados =
                mensagem.Substring(6);

            string[] partes =
                dados.Split(':');

            if (partes.Length < 2)
                return;

            if (!int.TryParse(
                partes[0],
                out int scoreP1))
            {
                return;
            }

            if (!int.TryParse(
                partes[1],
                out int scoreP2))
            {
                return;
            }

            GameManager gameManager =
                GameManager.Instance;

            if (gameManager != null)
            {
                gameManager.SincronizarPontuacao(
                    scoreP1,
                    scoreP2
                );
            }

            return;
        }

        // -----------------------------------------------------
        // REINICIAR
        // -----------------------------------------------------

        if (mensagem == "RESTART")
        {
            if (playerID == 2)
            {
                GameManager gameManager =
                    GameManager.Instance;

                if (gameManager != null)
                {
                    gameManager.ReiniciarPartidaRemota();
                }
            }

            return;
        }
    }

    public int GetPlayerID()
    {
        return playerID;
    }

    public bool EstaConectado()
    {
        return conectado;
    }

    public bool PartidaPronta()
    {
        return partidaPronta;
    }

    private void OnDestroy()
    {
        FecharCliente();
    }

    private void OnApplicationQuit()
    {
        FecharCliente();
    }

    private void FecharCliente()
    {
        encerrando = true;
        conectado = false;

        if (cliente != null)
        {
            cliente.Close();
            cliente = null;

            Debug.Log(
                "Cliente UDP encerrado."
            );
        }
    }
}
