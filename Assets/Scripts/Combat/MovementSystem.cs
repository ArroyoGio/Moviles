using UnityEngine;

public class MovementSystem : MonoBehaviour
{
    private float speed;
    private bool defensive = false;

    public void Initialize(int agility)
    {
        // Velocidad derivada de agilidad — entre 1.5 y 4 unidades/seg
        speed = Mathf.Lerp(1.5f, 4f, agility / 1000f);
    }

    public void MoveTowards(Vector3 target)
    {
        transform.position = Vector2.MoveTowards(
            transform.position, target,
            speed * Time.deltaTime);

        // Sorting por Y para profundidad isométrica
        GetComponent<SpriteRenderer>().sortingOrder =
            Mathf.RoundToInt(-transform.position.y * 100);
    }

    public void Retreat()
    {
        // Retrocede en dirección opuesta al centro
        Vector3 dirToCenter = (Vector3.zero - transform.position).normalized;
        transform.position -= dirToCenter * speed * Time.deltaTime;
    }

    public void SetDefensive(bool value) => defensive = value;
}