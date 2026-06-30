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
    private int roundsPlayed = 0;
    private bool roundActive = false;

    public static event Action<int> OnRoundStarted;
    public static event Action OnBreakStarted;
    public static event Action<int, int, int> OnRoundEnded;
    public static event Action<CombatResult> OnMatchEnded;

    void Awake() => Instance = this;

    void Start()
    {
        // Evita duplicados de ArenaManager al cargar escenas
        if (Instance != null && Instance != this)
        {
            Debug.Log($"ArenaManager: instancia duplicada detectada en '{gameObject.name}', destruyendo la nueva.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

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
        Debug.Log($"KO - Round {currentRound} terminado - scores: {scores[0]}-{scores[1]}");
        EndRound(true);
    }

    void EndRound(bool byKO)
    {
        roundsPlayed++;
        int roundWinner = -1;

        if (byKO)
        {
            var localAlive = CombatSystem.Instance.GetActiveAllies(1);
            if (localAlive.Count > 0)
            {
                scores[0]++;
                roundWinner = 0;
            }
            else
            {
                scores[1]++;
                roundWinner = 1;
            }
        }

        Debug.Log($"Score tras round {currentRound}: {scores[0]}-{scores[1]}");
        OnRoundEnded?.Invoke(roundWinner, scores[0], scores[1]);

        if (roundsPlayed < TOTAL_ROUNDS)
            StartCoroutine(Break());
        else
            EndMatch();
    }

    IEnumerator Break()
    {
        int nextRound = currentRound + 1;
        OnBreakStarted?.Invoke();
        yield return new WaitForSeconds(10f);

        if (TeamManager.Instance == null)
        {
            Debug.LogError("ArenaManager.Break: TeamManager not found.");
            EndMatch();
            yield break;
        }

        if (CombatSystem.Instance == null)
        {
            Debug.LogError("ArenaManager.Break: CombatSystem not found.");
            EndMatch();
            yield break;
        }

        if (!TeamManager.Instance.EquipoListo())
        {
            Debug.LogError("ArenaManager.Break: team is not ready.");
            EndMatch();
            yield break;
        }

        // Reinstancia fighters sin llamar StartRound desde CombatSystem
        var team = TeamManager.Instance.equipoActual;
        if (team.activos[0] == null || team.activos[1] == null)
        {
            Debug.LogError("ArenaManager.Break: team incomplete.");
            EndMatch();
            yield break;
        }

        EquipmentStateManager.GetOrCreate().RestoreEquipment(team.activos[0]);
        EquipmentStateManager.GetOrCreate().RestoreEquipment(team.activos[1]);

        team.activos[0].RecalculateStatsFromBaseAndEquipment();
        team.activos[1].RecalculateStatsFromBaseAndEquipment();

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
        Debug.Log($"Partida terminada - ganador: {result.winner}");
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
