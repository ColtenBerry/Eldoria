using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameObject player;

    public GameObject partyPrefab;
    public LordProfileSO playerProfileSO;
    private LordProfile playerProfile;

    public LordProfile PlayerProfile => playerProfile;

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


        // setup player profile
        playerProfile = new LordProfile(playerProfileSO);

        // spawn player
        PartyPresence partyPresence = player.GetComponent<PartyPresence>();

        // Generate LordProfile
        playerProfile.AddActiveParty(partyPresence);

        player.SetActive(true);



        LordRegistry.Instance.Initialize();
        TerritoryManager.Instance.InitializeOwnership(LordRegistry.Instance.GetAllLords());

    }

    public void Start()
    {
        foreach (LordProfile profile in LordRegistry.Instance.GetAllLords())
        {
            if (profile.SourceData == playerProfileSO) continue;
            SpawnParty(profile);
        }
    }

    public void SpawnParty(LordProfile lordProfile)
    {
        Vector3 spawnPoint = TerritoryManager.Instance.GetSettlementsOf(lordProfile)[0].gameObject.transform.position;

        GameObject newParty = Instantiate(partyPrefab, spawnPoint, Quaternion.identity);
        PartyPresence partyPresence = newParty.GetComponent<PartyPresence>();

        lordProfile.AddActiveParty(partyPresence);

        FactionsManager.Instance.RegisterParty(partyPresence);

        newParty.SetActive(true);
    }


    public LordProfile GetPlayerProfile()
    {
        return playerProfile;
    }
}
