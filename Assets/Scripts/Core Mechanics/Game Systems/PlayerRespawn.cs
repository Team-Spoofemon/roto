using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    public static PlayerRespawn Instance { get; private set; }
    public Vector3 respawnPoint = Vector3.zero;

    public void SetRespawnPoint(Vector3 newRespawnPoint) => respawnPoint = newRespawnPoint;
    public Vector3 GetRespawnPoint() => respawnPoint;

    private void Start()
    {
        if (respawnPoint == Vector3.zero)
            respawnPoint = transform.position;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Respawn(Transform respawnable)
    {
        Debug.Log($"Respawning at {respawnPoint}");
        respawnable.SetPositionAndRotation(respawnPoint, Quaternion.identity);
    }
}
