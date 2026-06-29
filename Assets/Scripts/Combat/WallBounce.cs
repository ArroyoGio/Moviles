using UnityEngine;
using System.Collections;

public class WallBounce : MonoBehaviour
{
    private bool immunityActive = false;

    private const float SHORT_BOUNCE_DUR = 0.3f;  // GDD 4.2
    private const float LONG_BOUNCE_DUR = 0.8f;
    private const float IMMUNITY_DUR = 2.5f;
    private const float ARENA_LIMIT = 4.5f;  // límite visible en cámara

    public void ApplyPush(Vector2 direction, int pushForce)
    {
        var movement = GetComponent<MovementSystem>();
        Vector3 destination = transform.position + (Vector3)(direction * pushForce * 0.001f);
        if (movement != null)
            destination = movement.ConstrainPosition(destination);

        if (OutOfBounds(destination))
        {
            // Clamp dentro del octógono
            transform.position = new Vector3(
                Mathf.Clamp(destination.x, -ARENA_LIMIT, ARENA_LIMIT),
                Mathf.Clamp(destination.y, -ARENA_LIMIT, ARENA_LIMIT), 0);

            float duration = immunityActive ? SHORT_BOUNCE_DUR : LONG_BOUNCE_DUR;
            StartCoroutine(ProcessBounce(duration));
        }
        else
        {
            transform.position = destination;
        }
    }

    private bool OutOfBounds(Vector2 pos) =>
        Mathf.Abs(pos.x) > ARENA_LIMIT || Mathf.Abs(pos.y) > ARENA_LIMIT;

    IEnumerator ProcessBounce(float duration)
    {
        GetComponent<AIBrain>().Pause(duration);
        yield return new WaitForSeconds(duration);
        if (!immunityActive)
            StartCoroutine(StartImmunity());
    }

    IEnumerator StartImmunity()
    {
        immunityActive = true;
        yield return new WaitForSeconds(IMMUNITY_DUR);
        immunityActive = false;
    }
}
