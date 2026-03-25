using UnityEngine;

public class TextureOffset : MonoBehaviour
{
    // Scroll the main texture based on time

    [SerializeField] private float scrollSpeed = 0.5f;
    Renderer rend;
    [SerializeField] private bool offsetX;
    [SerializeField] private bool offsetY;

    void Start()
    {
        rend = GetComponent<Renderer> ();
    }

    void Update()
    {
        float offset = Time.time * scrollSpeed;

        if (offsetX == true){
        rend.material.mainTextureOffset = new Vector2(offset, 0);
        }

        else if (offsetY == true)
        {
            rend.material.mainTextureOffset = new Vector2(0, -offset);
        }
        else
        {
            Debug.Log("No offset direction set.");
        }
        
    }
}