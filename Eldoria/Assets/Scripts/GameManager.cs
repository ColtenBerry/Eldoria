using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;


[DefaultExecutionOrder(0)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameObject player;

    public GameObject partyPrefab;
    public LordProfileSO playerProfileSO;
    private LordProfile playerProfile;

    public LordProfile PlayerProfile => playerProfile;

    void OnEnable() => Debug.Log("GameManager enabled");
    void OnDisable() => Debug.Log("GameManager disabled");


    void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Prevent duplicates
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional: persist across scenes






        LordRegistry.Instance.Initialize();
        TerritoryManager.Instance.InitializeOwnership(LordRegistry.Instance.GetAllLords());

        playerProfile = LordRegistry.Instance.GetLordBySO(playerProfileSO);
        PartyPresence partyPresence = player.GetComponent<PartyPresence>();
        if (partyPresence == null)
            Debug.LogError("PartyPresence missing on player!");


        playerProfile.AddActiveParty(partyPresence);

        player.SetActive(true);



    }

    public void Start()
    {
        foreach (LordProfile profile in LordRegistry.Instance.GetAllLords())
        {
            if (profile.SourceData == playerProfileSO) continue;
            SpawnParty(profile);
        }

        // subscribe to weekly tick
        TickManager.Instance.OnWeekPassed += TerritoryManager.Instance.DistributeWeeklyEarnings;
    }

    public void SpawnParty(LordProfile lordProfile)
    {
        Vector3 spawnPoint = TerritoryManager.Instance.GetSettlementsOf(lordProfile)[0].gameObject.transform.position;

        // no territories
        if (spawnPoint == null)
        {
            spawnPoint = TerritoryManager.Instance.GetSettlementsOfFaction(lordProfile.Faction).First().gameObject.transform.position;
        }

        // faction has no territories
        if (spawnPoint == null)
        {
            return; // don't spawn i guess
        }

        GameObject newParty = Instantiate(partyPrefab, spawnPoint, Quaternion.identity);
        PartyPresence partyPresence = newParty.GetComponent<PartyPresence>();

        if (partyPresence == null)
            Debug.LogError("PartyPresence missing on " + lordProfile.Lord.UnitName + "!");


        lordProfile.AddActiveParty(partyPresence);

        FactionsManager.Instance.RegisterParty(partyPresence);

        newParty.SetActive(true);
    }


    public LordProfile GetPlayerProfile()
    {
        return playerProfile;
    }
}
