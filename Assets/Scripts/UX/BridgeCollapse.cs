using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class BridgeCollapse : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private string playerTag = "Player";

    [Header("Pieces")]
    [SerializeField] private List<GameObject> collapsePieces = new List<GameObject>();
    [SerializeField] private float maxStartDelay = 1f;

    [Header("Impulse")]
    [SerializeField] private CinemachineImpulseSource impulseSource;

    private BridgeFloatingPieces floatingPieces;
    private bool triggered;

    private void Awake()
    {
        floatingPieces = GetComponent<BridgeFloatingPieces>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag(playerTag)) return;

        triggered = true;
        StartCollapse();
    }

    public void StartCollapse()
    {
        StartCoroutine(CollapseRoutine());
    }

    private IEnumerator CollapseRoutine()
    {
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulse();
        }

        for (int i = 0; i < collapsePieces.Count; i++)
        {
            GameObject piece = collapsePieces[i];
            if (piece == null) continue;

            float delay = Random.Range(0f, maxStartDelay);
            StartCoroutine(DropPiece(piece, delay));
        }

        yield return new WaitForSeconds(maxStartDelay);

        if (floatingPieces != null)
        {
            floatingPieces.BeginFloating();
        }
    }

    private IEnumerator DropPiece(GameObject piece, float delay)
    {
        yield return new WaitForSeconds(delay);

        Rigidbody rb = piece.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = piece.AddComponent<Rigidbody>();
        }

        rb.isKinematic = false;
        rb.useGravity = true;

        Vector3 randomForce = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(1f, 2f),
            Random.Range(-1f, 1f)
        );

        rb.AddForce(randomForce, ForceMode.Impulse);
    }
}