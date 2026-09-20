using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UDPClient : MonoBehaviour
{
    [Header("Conexão")]
    [SerializeField] private string ipServidor =
        "127.0.0.1";

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

    private readonly Queue<string>
        mensagensRecebidas =
        new Queue<string>();

    private readonly object lockMensagens =
        new object();

    // =========================================================
    // INICIALIZAÇÃO
    // =========================================================

    private void Start()
    {
        Conectar();
    }

    private void Update()
    {
        ProcessarMensagensRecebidas();

        if (!conectado || encerrando)
            return;

        // Envia posição da própria raquete
        if (minhaRaquete != null)
        {
            EnviarPosicaoRaquete();
        }

        // Player 1 é autoridade da bola
        if (playerID == 1 &&
            bola != null &&
            bolaRb != null)
        {
            EnviarEstadoBola();
        }
    }

    // =========================================================
    // CONECTAR
    // =========================================================

    private void Conectar()
    {
        try
        {
            enderecoServidor =
                new IPEndPoint(
                    IPAddress.Parse(
                        ipServidor
                    ),
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
                "Cliente UDP conectado ao servidor."
            );

            EnviarMensagem("HELLO");

            cliente.BeginReceive(
                ReceberMensagem,
                null
            );
        }
        catch (Exception erro)
        {
            Debug.LogError(
                "Erro ao conectar ao servidor: " +
                erro.Message
            );
        }
    }

    // =========================================================
    // ENVIAR POSIÇÃO DA RAQUETE
    // =========================================================

    private void EnviarPosicaoRaquete()
    {
        float posicaoY =
            minhaRaquete.position.y;

        string mensagem =
            "POS:" +
            posicaoY.ToString("F2");

        EnviarMensagem(
            mensagem
        );
    }

    // =========================================================
    // ENVIAR ESTADO DA BOLA
    // =========================================================

    private void EnviarEstadoBola()
    {
        Vector2 posicao =
            bola.position;

        Vector2 velocidade =
            bolaRb.linearVelocity;

        string mensagem =
            "BALL:" +
            posicao.x.ToString("F3") +
            ":" +
            posicao.y.ToString("F3") +
            ":" +
            velocidade.x.ToString("F3") +
            ":" +
            velocidade.y.ToString("F3");

        EnviarMensagem(
            mensagem
        );
    }

    // =========================================================
    // ENVIAR PONTUAÇÃO
    // =========================================================

    public void EnviarPontuacao(
        int scoreP1,
        int scoreP2)
    {
        if (!conectado ||
            encerrando)
        {
            return;
        }

        // Somente Player 1 envia
        // a pontuação oficial
        if (playerID != 1)
            return;

        string mensagem =
            "SCORE:" +
            scoreP1 +
            ":" +
            scoreP2;

        EnviarMensagem(
            mensagem
        );

        Debug.Log(
            "Pontuação enviada: P1 " +
            scoreP1 +
            " x P2 " +
            scoreP2
        );
    }

    // =========================================================
    // ENVIAR MENSAGEM
    // =========================================================

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

    // =========================================================
    // RECEBER MENSAGEM
    // =========================================================

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

    // =========================================================
    // FILA DE MENSAGENS
    // =========================================================

    private void ProcessarMensagensRecebidas()
    {
        while (true)
        {
            string mensagem = null;

            lock (lockMensagens)
            {
                if (mensagensRecebidas.Count > 0)
                {
                    mensagem =
                        mensagensRecebidas.Dequeue();
                }
            }

            if (mensagem == null)
                break;

            ProcessarMensagem(
                mensagem
            );
        }
    }

    // =========================================================
    // PROCESSAR MENSAGEM
    // =========================================================

    private void ProcessarMensagem(
        string mensagem)
    {
        // -----------------------------------------------------
        // ID DO JOGADOR
        // -----------------------------------------------------

        if (mensagem.StartsWith("ID:"))
        {
            string textoID =
                mensagem.Substring(3);

            if (int.TryParse(
                textoID,
                out int novoID))
            {
                playerID = novoID;

                Debug.Log(
                    "Meu ID: Player " +
                    playerID
                );
            }

            return;
        }

        // -----------------------------------------------------
        // SERVIDOR CHEIO
        // -----------------------------------------------------

        if (mensagem == "FULL")
        {
            Debug.LogWarning(
                "O servidor está cheio."
            );

            return;
        }

        // -----------------------------------------------------
        // POSIÇÃO DA RAQUETE
        // -----------------------------------------------------

        if (mensagem.StartsWith("P1:POS:") ||
            mensagem.StartsWith("P2:POS:"))
        {
            ProcessarPosicaoAdversaria(
                mensagem
            );

            return;
        }

        // -----------------------------------------------------
        // BOLA
        // -----------------------------------------------------

        if (mensagem.StartsWith("P1:BALL:"))
        {
            ProcessarEstadoBola(
                mensagem
            );

            return;
        }

        // -----------------------------------------------------
        // PONTUAÇÃO
        // -----------------------------------------------------

        if (mensagem.StartsWith("P1:SCORE:") ||
            mensagem.StartsWith("P2:SCORE:"))
        {
            ProcessarPontuacao(
                mensagem
            );

            return;
        }
    }

    // =========================================================
    // PROCESSAR POSIÇÃO DA RAQUETE ADVERSÁRIA
    // =========================================================

    private void ProcessarPosicaoAdversaria(
        string mensagem)
    {
        string[] partes =
            mensagem.Split(':');

        if (partes.Length < 3)
            return;

        if (!float.TryParse(
            partes[2],
            out float posicaoY))
        {
            return;
        }

        int jogadorMensagem;

        if (partes[0] == "P1")
        {
            jogadorMensagem = 1;
        }
        else
        {
            jogadorMensagem = 2;
        }

        // Não mexer na própria raquete
        if (jogadorMensagem == playerID)
            return;

        if (raqueteAdversaria == null)
            return;

        Vector3 posicao =
            raqueteAdversaria.position;

        posicao.y = posicaoY;

        raqueteAdversaria.position =
            posicao;
    }

    // =========================================================
    // PROCESSAR ESTADO DA BOLA
    // =========================================================

    private void ProcessarEstadoBola(
        string mensagem)
    {
        string[] partes =
            mensagem.Split(':');

        if (partes.Length < 6)
            return;

        if (!float.TryParse(
            partes[2],
            out float x))
        {
            return;
        }

        if (!float.TryParse(
            partes[3],
            out float y))
        {
            return;
        }

        if (!float.TryParse(
            partes[4],
            out float velocidadeX))
        {
            return;
        }

        if (!float.TryParse(
            partes[5],
            out float velocidadeY))
        {
            return;
        }

        // Player 1 não precisa
        // receber sua própria bola
        if (playerID == 1)
            return;

        if (bola == null ||
            bolaRb == null)
        {
            return;
        }

        bola.position =
            new Vector3(
                x,
                y,
                bola.position.z
            );

        bolaRb.linearVelocity =
            new Vector2(
                velocidadeX,
                velocidadeY
            );
    }

    // =========================================================
    // PROCESSAR PONTUAÇÃO
    // =========================================================

    private void ProcessarPontuacao(
        string mensagem)
    {
        string[] partes =
            mensagem.Split(':');

        if (partes.Length < 4)
            return;

        // Exemplo:
        // P1:SCORE:2:1

        if (!int.TryParse(
            partes[2],
            out int novoScoreP1))
        {
            return;
        }

        if (!int.TryParse(
            partes[3],
            out int novoScoreP2))
        {
            return;
        }

        // Player 1 não precisa
        // receber a própria pontuação
        if (playerID == 1)
            return;

        if (GameManager.Instance == null)
            return;

        GameManager.Instance.SincronizarPontuacao(
            novoScoreP1,
            novoScoreP2
        );

        Debug.Log(
            "Pontuação recebida: P1 " +
            novoScoreP1 +
            " x P2 " +
            novoScoreP2
        );
    }

    // =========================================================
    // GETTERS
    // =========================================================

    public int GetPlayerID()
    {
        return playerID;
    }

    public bool EstaConectado()
    {
        return conectado;
    }

    // =========================================================
    // ENCERRAR CLIENTE
    // =========================================================

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
        if (encerrando)
            return;

        encerrando = true;

        conectado = false;

        if (cliente != null)
        {
            cliente.Close();

            cliente = null;
        }
    }
}