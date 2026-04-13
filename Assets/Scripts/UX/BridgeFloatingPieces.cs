using System.Collections.Generic;
using UnityEngine;

public class BridgeFloatingPieces : MonoBehaviour
{
    [System.Serializable]
    private class FloatingPiece
    {
        public Transform piece;
        public float amplitude = 0.35f;
        public float speed = 1f;
        public float phaseOffset;
    }

    [SerializeField] private List<Transform> floatingPieces = new List<Transform>();
    [SerializeField] private float amplitudeMin = 0.15f;
    [SerializeField] private float amplitudeMax = 0.45f;
    [SerializeField] private float speedMin = 0.8f;
    [SerializeField] private float speedMax = 1.6f;

    private readonly List<FloatingPiece> activePieces = new List<FloatingPiece>();
    private bool isFloating;

    public void BeginFloating()
    {
        if (isFloating) return;

        activePieces.Clear();

        for (int i = 0; i < floatingPieces.Count; i++)
        {
            Transform piece = floatingPieces[i];
            if (piece == null) continue;

            FloatingPiece newPiece = new FloatingPiece();
            newPiece.piece = piece;
            newPiece.amplitude = Random.Range(amplitudeMin, amplitudeMax);
            newPiece.speed = Random.Range(speedMin, speedMax);
            newPiece.phaseOffset = Random.Range(0f, 10f);

            activePieces.Add(newPiece);
        }

        isFloating = true;
    }

    private void Update()
    {
        if (!isFloating) return;

        for (int i = 0; i < activePieces.Count; i++)
        {
            FloatingPiece piece = activePieces[i];
            if (piece.piece == null) continue;

            Vector3 pos = piece.piece.position;
            float yOffset = Mathf.Sin((Time.time + piece.phaseOffset) * piece.speed) * piece.amplitude;
            piece.piece.position = new Vector3(pos.x, yOffset + GetBaseY(piece), pos.z);
        }
    }

    private float GetBaseY(FloatingPiece piece)
    {
        if (!baseHeights.TryGetValue(piece.piece, out float value))
        {
            value = piece.piece.position.y;
            baseHeights[piece.piece] = value;
        }

        return value;
    }

    private readonly Dictionary<Transform, float> baseHeights = new Dictionary<Transform, float>();
}