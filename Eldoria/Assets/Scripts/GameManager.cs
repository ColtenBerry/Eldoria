using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameObject player;
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

    void Start()
    {

    }

    void Update()
    {
        // Optional: game-wide update logic
    }

    // Optional: expose playerProfile if needed
    public LordProfile GetPlayerProfile()
    {
        return playerProfile;
    }
}
