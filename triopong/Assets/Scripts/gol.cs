using UnityEngine;

public class Goal : MonoBehaviour
{
    public enum PlayerGoal
    {
        Player1Goal,
        Player2Goal
    }

    [SerializeField] private PlayerGoal goal;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Ball"))
            return;

        if (GameManager.Instance == null)
            return;

        if (goal == PlayerGoal.Player1Goal)
        {
            // A bola entrou no gol do P1.
            // Portanto, P2 marcou.
            GameManager.Instance.GolP2();
        }
        else
        {
            // A bola entrou no gol do P2.
            // Portanto, P1 marcou.
            GameManager.Instance.GolP1();
        }
    }
}