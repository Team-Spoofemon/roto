using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private CombatManager combatManager = new CombatManager();

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
            GenerateManagers();
            DontDestroyOnLoad(this.gameObject);
            Debug.Log("GameManager created!");
        }
    }

    private void GenerateManagers()
    {
        combatManager = GetComponent<CombatManager>();
        if (combatManager == null)
        {
            combatManager = gameObject.AddComponent<CombatManager>();
            Debug.Log("CombatManager created in the GameManager!");
        }
    }
}
