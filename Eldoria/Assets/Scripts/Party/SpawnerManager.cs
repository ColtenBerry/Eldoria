using System.Collections.Generic;
using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
    public GameObject npcPartyPrefab;
    public LordProfileSO banditLordTemplate;
    [SerializeField] private int idealNumParties;
    [SerializeField] private float spawnRadius = 5.0f;

    private List<PartyPresence> activeParties = new();

    void Update()
    {
        if (activeParties.Count < idealNumParties)
        {
            SpawnParty();
        }
    }
    public void RegisterParty(PartyPresence npcParty)
    {
        npcParty.SetSpawnerManager(this);
        if (!activeParties.Contains(npcParty)) activeParties.Add(npcParty);
    }

    public void UnregisterParty(PartyPresence npcParty)
    {
        activeParties.Remove(npcParty);
    }
    public Vector3 GetRandomPointInCircle(Vector3 center, float radius)
    {
        Vector2 randomCircle = Random.insideUnitCircle * radius;
        return new Vector3(center.x + randomCircle.x, center.y, center.z + randomCircle.y);
    }




    private void SpawnParty()
    {
        // get location
        Vector3 spawnPoint = GetRandomPointInCircle(transform.position, spawnRadius);
        // instantiate party
        GameObject newParty = Instantiate(npcPartyPrefab, spawnPoint, Quaternion.identity);
        PartyPresence partyPresence = newParty.GetComponent<PartyPresence>();

        // Generate LordProfile
        LordProfile profile = new LordProfile(banditLordTemplate);
        profile.AddActiveParty(partyPresence);



        RegisterParty(partyPresence);

        newParty.SetActive(true);


    }
}
