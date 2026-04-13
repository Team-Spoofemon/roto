using UnityEngine;

public class StoneProjectile : MonoBehaviour
{
    private Vector3 startPoint;
    private Vector3 targetPoint;
    private float arcHeight;
    private float travelDuration;
    private float damage;
    private float knockback;
    private float timer;
    private Enemy owner;

    public void Initialize(Vector3 start, Vector3 target, float height, float duration, float dmg, float kb, Enemy shooter)
    {
        startPoint = start;
        targetPoint = target;
        arcHeight = height;
        travelDuration = duration;
        damage = dmg;
        knockback = kb;
        owner = shooter;

        transform.position = startPoint;
        timer = 0f;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        float t = timer / travelDuration;
        if (t >= 1f)
        {
            transform.position = targetPoint;
            Destroy(gameObject);
            return;
        }

        Vector3 nextPos = Vector3.Lerp(startPoint, targetPoint, t);
        nextPos.y += arcHeight * 4f * t * (1f - t);

        Vector3 moveDir = nextPos - transform.position;
        if (moveDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(moveDir.normalized);

        transform.position = nextPos;
    }
}