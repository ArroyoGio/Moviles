using UnityEngine;
using System.Collections;
using System;

public class ArenaManager : MonoBehaviour
{
    public static ArenaManager Instance;

    private float roundTimer = 0f;
    private int currentRound = 0;
    private const int TOTAL_ROUNDS = 3;
    private int[] scores = new int[2];
    private bool roundActive = false;

    public static event Action<int> OnRoundStarted;
    public static event Action OnBreakStarted;
    public static event Action<CombatResult> OnMatchEnded;

    void Awake() => Instance = this;

    public void StartRound(int number)
    {
        currentRound = number;
        roundTimer = 30f;
        roundActive = true;
        Debug.Log($"Round {currentRound} iniciado");
        OnRoundStarted?.Invoke(currentRound);
    }

    void Update()
    {
        if (!roundActive) return;
        roundTimer -= Time.deltaTime;
        if (roundTimer <= 0)
        {
            roundActive = false;
            EndRound(false);
        }
    }

    public void RoundEndByKO()
    {
        if (!roundActive) return; // evita doble llamada
        roundActive = false;
        Debug.Log($"KO — Round {currentRound} terminado — scores: {scores[0]}-{scores[1]}");
        EndRound(true);
    }

    void EndRound(bool byKO)
    {
        if (byKO)
        {
            var localAlive = CombatSystem.Instance.GetActiveAllies(1);
            if (localAlive.Count > 0) scores[0]++;
            else scores[1]++;
        }

        Debug.Log($"Score tras round {currentRound}: {scores[0]}-{scores[1]}");

        int totalRoundsPlayed = scores[0] + scores[1];
        if (totalRoundsPlayed < TOTAL_ROUNDS)
            StartCoroutine(Break());
        else
            EndMatch();
    }

    IEnumerator Break()
    {
        int nextRound = currentRound + 1;
        OnBreakStarted?.Invoke();
        yield return new WaitForSeconds(10f);

        // Reinstancia fighters sin llamar StartRound desde CombatSystem
        var team = TeamManager.Instance.equipoActual;
        CombatSystem.Instance.StartMatch1v1(team.activos[0], team.activos[1], false);

        StartRound(nextRound);
    }

    void EndMatch()
    {
        bool draw = scores[0] == scores[1];
        var result = new CombatResult
        {
            winner = draw ? -1 : scores[0] > scores[1] ? 0 : 1,
            finalScores = scores,
            requiresOvertime = draw
        };
        Debug.Log($"Partida terminada — ganador: {result.winner}");
        OnMatchEnded?.Invoke(result);
    }
}


[System.Serializable]
public class CombatResult
{
    public int winner;
    public int[] finalScores;
    public bool requiresOvertime;
}