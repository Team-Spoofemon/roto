using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private CombatManager combatManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log("GameManager duplicate created!");
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            GenerateManager<CombatManager>(ref combatManager);
            DontDestroyOnLoad(this.gameObject);
            Debug.Log("GameManager created!");
        }
    }

    private void GenerateManager<T>(ref T comp)
        where T : Component
    {
        comp = GetComponent<T>();
        if (comp == null)
        {
            comp = gameObject.AddComponent<T>();
            Debug.Log($"{typeof(T).Name} created in the GameManager!");
        }
    }
}
