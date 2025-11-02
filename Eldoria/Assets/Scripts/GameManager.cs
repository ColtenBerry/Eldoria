using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public GameObject player;
    public LordProfileSO playerProfileSO;
    private LordProfile playerProfile;

    void Start()
    {
        // setup player profile
        playerProfile = new LordProfile(playerProfileSO);

        // spawn player
        Vector3 spawnPoint = new Vector3(0, 0, 0);
        PartyPresence partyPresence = player.GetComponent<PartyPresence>();

        // Generate LordProfile
        playerProfile.AddActiveParty(partyPresence);

        player.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
