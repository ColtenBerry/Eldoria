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
    }

    private void ApplyVisuals()
    {
        // set icon, color, etc.

        if (!isPlayer)
        {
            TextMeshProUGUI lordNameText = transform.Find("NameTag/Canvas/NameTagText").GetComponent<TextMeshProUGUI>();
            lordNameText.text = lord.UnitName;

            SpriteRenderer spriteRenderer = transform.Find("NameTag").GetComponent<SpriteRenderer>();
            spriteRenderer.color = Color.green;
        }

    }

    public void Interact()
    {
        Debug.Log("Attempting interaction with " + lord.UnitName);
    }

    public List<InteractionOption> GetInteractionOptions()
    {
        List<InteractionOption> options = new();

        if (lord.Faction != "Player") options.Add(new InteractionOption("Attack Party", () => Debug.Log("Attempting to Attack"), "combat"));
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
    }

}
