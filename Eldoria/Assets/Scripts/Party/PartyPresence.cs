using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class PartyPresence : MonoBehaviour, IInteractable
{
    public PartyProfile partyProfile;
    public PartyController partyController;
    public bool isPlayer;

    [SerializeField] private CharacterInstance lord;
    public CharacterInstance Lord
    {
        get
        {
            return lord;
        }
    }

    private SpawnerManager spawner;

    void Awake()
    {
        partyController = GetComponent<PartyController>();
        if (partyController == null) Debug.LogError("PartyController not found on PartyPresence GameObject");
        lord = new CharacterInstance(partyProfile.Lord);
        Debug.Log("lord: " + lord);

        InitializeParty();
        ApplyVisuals();

    }

    private void InitializeParty()
    {
        foreach (UnitData unit in partyProfile.startingUnits)
        {
            Debug.Log("adding unit: presence");
            partyController.AddUnit(new UnitInstance(unit));
        }
        FactionsManager.Instance.RegisterParty(this);
    }

    private void ApplyVisuals()
    {
        // set icon, color, etc.

        if (!isPlayer)
        {
            TextMeshProUGUI lordNameText = transform.Find("NameTag/Canvas/NameTagText").GetComponent<TextMeshProUGUI>();
            lordNameText.text = lord.UnitName;

            SpriteRenderer spriteRenderer = transform.Find("NameTag").GetComponent<SpriteRenderer>();
            spriteRenderer.color = lord.Faction.factionColor;
        }

    }

    public void Interact()
    {
        Debug.Log("Attempting interaction with " + lord.UnitName);
    }

    public List<InteractionOption> GetInteractionOptions()
    {
        List<InteractionOption> options = new();

        if (FactionsManager.Instance.AreEnemies(lord.Faction, FactionsManager.Instance.GetFactionByName("Player"))) options.Add(new InteractionOption("Attack Party", () => Debug.Log("Attempting to Attack"), "combat"));
        options.Add(new InteractionOption("Talk to Leader", () => Debug.Log("Attempting to spaek")));
        options.Add(new InteractionOption("Leave", () => Debug.Log("Attempting to Leave")));


        return options;
    }

    public void SetSpawnerManager(SpawnerManager manager)
    {
        spawner = manager;
    }

    void OnDestroy()
    {
        if (spawner != null)
        {
            spawner.UnregisterParty(this);
        }
        FactionsManager.Instance.UnregisterParty(this);
    }

    public int GetStrengthEstimate()
    {
        int strength = 0;
        foreach (UnitInstance unit in partyController.PartyMembers)
        {
            strength += ((unit.Attack + unit.Defence) * unit.Health);
        }
        return strength;
    }

}
