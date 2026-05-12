using UnityEngine;

[RequireComponent(typeof(HealthSystem))]
[RequireComponent(typeof(StaminaSystem))]
[RequireComponent(typeof(AttackSystem))]
[RequireComponent(typeof(MovementSystem))]
[RequireComponent(typeof(WallBounce))]
[RequireComponent(typeof(AIBrain))]
public class Fighter : MonoBehaviour
{
    public VeteranData data;
    public int side; // 1 = jugador, -1 = rival

    public HealthSystem health { get; private set; }
    public StaminaSystem stamina { get; private set; }
    public AttackSystem attack { get; private set; }
    public MovementSystem movement { get; private set; }

    void Awake()
    {
        health = GetComponent<HealthSystem>();
        stamina = GetComponent<StaminaSystem>();
        attack = GetComponent<AttackSystem>();
        movement = GetComponent<MovementSystem>();
    }

    public void Initialize(VeteranData veteranData, int combatSide)
    {
        data = veteranData;
        side = combatSide;

        health.Initialize(data.life);
        stamina.Initialize(data.stamina);
        attack.Initialize(data);
        movement.Initialize(data.agility);
        GetComponent<AIBrain>().Initialize(data.baseData.role);
    }
}