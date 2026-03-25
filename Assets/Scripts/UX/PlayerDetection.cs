using UnityEngine;
using UnityEngine.Events;

public class PlayerDetection : MonoBehaviour
{
    public Transform playerRoot;

    public string spotObjectName = "Spot";
    public string shadowName = "Player Shadow";
    public string swordName = "Sword Hitbox";

    public LayerMask detectionMask = ~0;
    public float rayStartOffset = 0.05f;

    public bool triggerOnce = true;
    public UnityEvent onPlayerCaught;

    Transform lightOrigin;
    Light spotLight;
    Transform playerShadow;
    Transform playerSword;

    bool hasCaughtPlayer;

    void Awake()
    {
        CacheSpotLight();
        CachePoints();
    }

    void FixedUpdate()
    {
        if (triggerOnce && hasCaughtPlayer) return;
        if (lightOrigin == null || spotLight == null || playerRoot == null) return;

        if (CheckPoint(playerRoot.position) ||
            CheckPoint(GetShadowPoint()) ||
            CheckPoint(GetSwordPoint()))
        {
            PlayerCaught();
        }
    }

    void CacheSpotLight()
    {
        Transform spot = FindChild(transform, spotObjectName);

        if (spot == null) return;

        lightOrigin = spot;
        spotLight = spot.GetComponent<Light>();
    }

    void CachePoints()
    {
        playerShadow = FindChild(playerRoot, shadowName);
        playerSword = FindChild(playerRoot, swordName);
    }

    Transform FindChild(Transform parent, string name)
    {
        if (parent == null) return null;
        if (parent.name == name) return parent;

        foreach (Transform child in parent)
        {
            Transform result = FindChild(child, name);
            if (result != null) return result;
        }

        return null;
    }

    bool CheckPoint(Vector3 point)
    {
        Vector3 origin = lightOrigin.position;
        Vector3 forward = lightOrigin.forward;

        Vector3 toPoint = point - origin;
        float dist = toPoint.magnitude;

        if (dist <= 0f || dist > spotLight.range) return false;

        Vector3 dir = toPoint / dist;

        float limit = Mathf.Cos((spotLight.spotAngle * 0.5f) * Mathf.Deg2Rad);
        float dot = Vector3.Dot(forward, dir);

        if (dot < limit) return false;

        Vector3 rayOrigin = origin + dir * rayStartOffset;
        float rayDist = dist - rayStartOffset;

        if (Physics.Raycast(rayOrigin, dir, out RaycastHit hit, rayDist, detectionMask, QueryTriggerInteraction.Ignore))
        {
            return hit.transform == playerRoot || hit.transform.IsChildOf(playerRoot);
        }

        return false;
    }

    Vector3 GetShadowPoint()
    {
        if (playerShadow != null) return playerShadow.position;
        return playerRoot.position;
    }

    Vector3 GetSwordPoint()
    {
        if (playerSword != null) return playerSword.position;
        return playerRoot.position;
    }

    public void PlayerCaught()
    {
        if (triggerOnce && hasCaughtPlayer) return;

        hasCaughtPlayer = true;
        onPlayerCaught.Invoke();
    }

    public void ResetDetection()
    {
        hasCaughtPlayer = false;
    }

    public void RefreshReferences()
    {
        CacheSpotLight();
        CachePoints();
    }
}