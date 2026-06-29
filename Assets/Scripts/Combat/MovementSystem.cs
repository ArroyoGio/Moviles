using UnityEngine;

public class MovementSystem : MonoBehaviour
{
    private const float LOCAL_MIN_X = -4.5f;
    private const float LOCAL_MAX_X = -0.25f;
    private const float RIVAL_MIN_X = 0.25f;
    private const float RIVAL_MAX_X = 4.5f;

    private float speed;
    private bool defensive = false;
    private int side = 1;

    public void Initialize(int agility)
    {
        Initialize(agility, side);
    }

    public void Initialize(int agility, int combatSide)
    {
        // Velocidad derivada de agilidad — entre 1.5 y 4 unidades/seg
        speed = Mathf.Lerp(1.5f, 4f, agility / 1000f);
        side = combatSide;
        ClampToSide();
    }

    public void MoveTowards(Vector3 target)
    {
        target = ConstrainPosition(target);

        transform.position = Vector2.MoveTowards(
            transform.position, target,
            speed * Time.deltaTime);

        ClampToSide();

        // Sorting por Y para profundidad isométrica
        GetComponent<SpriteRenderer>().sortingOrder =
            Mathf.RoundToInt(-transform.position.y * 100);
    }

    public void Retreat()
    {
        // Retrocede en dirección opuesta al centro
        Vector3 dirToCenter = (Vector3.zero - transform.position).normalized;
        transform.position -= dirToCenter * speed * Time.deltaTime;
        ClampToSide();
    }

    public void ApplyDisplacement(Vector2 delta)
    {
        transform.position = ConstrainPosition(transform.position + (Vector3)delta);
    }

    public Vector3 ConstrainPosition(Vector3 position)
    {
        if (side == 1)
            position.x = Mathf.Clamp(position.x, LOCAL_MIN_X, LOCAL_MAX_X);
        else if (side == -1)
            position.x = Mathf.Clamp(position.x, RIVAL_MIN_X, RIVAL_MAX_X);

        position.y = Mathf.Clamp(position.y, -4.5f, 4.5f);
        position.z = 0f;
        return position;
    }

    public void ClampToSide()
    {
        transform.position = ConstrainPosition(transform.position);
    }

    public void SetDefensive(bool value) => defensive = value;
}
