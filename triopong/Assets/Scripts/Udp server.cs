using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UDPServer : MonoBehaviour
{
    [Header("Servidor")]
    [SerializeField] private bool executarServidor = true;
    [SerializeField] private int porta = 7777;

    private UdpClient servidor;

    private Dictionary<string, int> jogadores =
        new Dictionary<string, int>();

    private Dictionary<int, IPEndPoint> enderecosJogadores =
        new Dictionary<int, IPEndPoint>();

    private int proximoPlayerID = 1;

    private void Start()
    {
        if (!executarServidor)
        {
            Debug.Log(
                "UDP Server desativado nesta instância."
            );

            return;
        }

        IniciarServidor();
    }

    private void IniciarServidor()
    {
        try
        {
            servidor = new UdpClient(porta);

            servidor.BeginReceive(
                ReceberMensagem,
                null
            );

            Debug.Log(
                "SERVIDOR UDP iniciado na porta " +
                porta
            );
        }
        catch (Exception erro)
        {
            Debug.LogError(
                "Erro ao iniciar servidor UDP: " +
                erro.Message
            );
        }
    }

    private void ReceberMensagem(
        IAsyncResult resultado)
    {
        if (servidor == null)
            return;

        try
        {
            IPEndPoint endereco =
                new IPEndPoint(
                    IPAddress.Any,
                    0
                );

            byte[] dados =
                servidor.EndReceive(
                    resultado,
                    ref endereco
                );

            string mensagem =
                Encoding.UTF8.GetString(dados);

            ProcessarMensagem(
                mensagem,
                endereco
            );

            if (servidor != null)
            {
                servidor.BeginReceive(
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
            Debug.LogError(
                "Erro ao receber mensagem: " +
                erro.Message
            );
        }
    }

    private void ProcessarMensagem(
        string mensagem,
        IPEndPoint endereco)
    {
        string identificador =
            endereco.ToString();

        if (!jogadores.ContainsKey(
            identificador))
        {
            RegistrarJogador(
                identificador,
                endereco
            );

            return;
        }

        int playerID =
            jogadores[identificador];

        // =====================================================
        // POSIÇÃO DA RAQUETE
        // =====================================================

        if (mensagem.StartsWith("POS:"))
        {
            EnviarParaOutroJogador(
                playerID,
                mensagem
            );

            return;
        }

        // =====================================================
        // ESTADO DA BOLA
        // =====================================================

        if (mensagem.StartsWith("BALL:"))
        {
            // Player 1 é a autoridade da bola
            if (playerID == 1)
            {
                EnviarParaOutroJogador(
                    playerID,
                    mensagem
                );
            }

            return;
        }

        // =====================================================
        // PONTUAÇÃO
        // =====================================================

        if (mensagem.StartsWith("SCORE:"))
        {
            // Player 1 é a autoridade da pontuação
            if (playerID == 1)
            {
                EnviarParaOutroJogador(
                    playerID,
                    mensagem
                );
            }

            return;
        }
    }

    private void RegistrarJogador(
        string identificador,
        IPEndPoint endereco)
    {
        if (proximoPlayerID > 2)
        {
            Debug.Log(
                "Partida cheia. Jogador recusado."
            );

            EnviarMensagem(
                "FULL",
                endereco
            );

            return;
        }

        int playerID =
            proximoPlayerID;

        jogadores.Add(
            identificador,
            playerID
        );

        enderecosJogadores.Add(
            playerID,
            endereco
        );

        proximoPlayerID++;

        Debug.Log(
            "Jogador " +
            playerID +
            " conectado! (" +
            jogadores.Count +
            "/2)"
        );

        EnviarMensagem(
            "ID:" +
            playerID,
            endereco
        );
    }

    private void EnviarParaOutroJogador(
        int playerQueEnviou,
        string mensagem)
    {
        int outroJogador;

        if (playerQueEnviou == 1)
        {
            outroJogador = 2;
        }
        else
        {
            outroJogador = 1;
        }

        if (!enderecosJogadores.ContainsKey(
            outroJogador))
        {
            return;
        }

        IPEndPoint enderecoDestino =
            enderecosJogadores[
                outroJogador
            ];

        string mensagemComID =
            "P" +
            playerQueEnviou +
            ":" +
            mensagem;

        EnviarMensagem(
            mensagemComID,
            enderecoDestino
        );
    }

    private void EnviarMensagem(
        string mensagem,
        IPEndPoint endereco)
    {
        if (servidor == null)
            return;

        try
        {
            byte[] dados =
                Encoding.UTF8.GetBytes(
                    mensagem
                );

            servidor.Send(
                dados,
                dados.Length,
                endereco
            );
        }
        catch (Exception erro)
        {
            Debug.LogError(
                "Erro ao enviar mensagem: " +
                erro.Message
            );
        }
    }

    private void OnDestroy()
    {
        FecharServidor();
    }

    private void OnApplicationQuit()
    {
        FecharServidor();
    }

    private void FecharServidor()
    {
        if (servidor != null)
        {
            servidor.Close();

            servidor = null;

            Debug.Log(
                "Servidor UDP encerrado."
            );
        }
    }
}