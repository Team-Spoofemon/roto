using System.Collections.Generic;
using UnityEngine;

public class FloatingRocks : MonoBehaviour
{
    [System.Serializable]
    private class FloatingPiece
    {
        public Transform piece;
        public float startY;
        public float amplitude;
        public float speed;
        public float offset;
    }

    [SerializeField] private List<Transform> rocks = new List<Transform>();
    [SerializeField] private float amplitudeMin = 0.2f;
    [SerializeField] private float amplitudeMax = 0.5f;
    [SerializeField] private float speedMin = 0.8f;
    [SerializeField] private float speedMax = 1.5f;

    private List<FloatingPiece> active = new List<FloatingPiece>();

    private void Start()
    {
        active.Clear();

        for (int i = 0; i < rocks.Count; i++)
        {
            if (rocks[i] == null) continue;

            FloatingPiece p = new FloatingPiece();
            p.piece = rocks[i];
            p.startY = rocks[i].position.y;
            p.amplitude = Random.Range(amplitudeMin, amplitudeMax);
            p.speed = Random.Range(speedMin, speedMax);
            p.offset = Random.Range(0f, 10f);

            active.Add(p);
        }
    }

    private void Update()
    {
        for (int i = 0; i < active.Count; i++)
        {
            if (active[i].piece == null) continue;

            Vector3 pos = active[i].piece.position;
            float y = active[i].startY + Mathf.Sin((Time.time + active[i].offset) * active[i].speed) * active[i].amplitude;
            active[i].piece.position = new Vector3(pos.x, y, pos.z);
        }
    }
}