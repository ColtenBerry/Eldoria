using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class PartyPresence : MonoBehaviour, IInteractable
{
    public PartyController partyController;
    public bool isPlayer;

    [SerializeField] private LordProfile lord;
    public LordProfile Lord
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

        ApplyVisuals();

    }

    void Start()
    {
        InitializeParty();
    }

    private void InitializeParty()
    {
        foreach (UnitData unit in lord.StartingUnits)
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
            lordNameText.text = lord.Lord.UnitName;

            SpriteRenderer spriteRenderer = transform.Find("NameTag").GetComponent<SpriteRenderer>();
            spriteRenderer.color = lord.Faction.factionColor;
        }

    }

    public void Interact()
    {
        Debug.Log("Attempting interaction with " + lord.Lord.UnitName);
    }

    public List<InteractionOption> GetInteractionOptions()
    {
        List<InteractionOption> options = new();

        if (FactionsManager.Instance.AreEnemies(lord.Faction, FactionsManager.Instance.GetFactionByName("Player")))
        {
            options.Add(new InteractionOption("Attack Party", () =>
            {
                Debug.Log("Attempting to Attack");
                CombatSimulator.InitiateCombat(partyController, true);
            }
                ));
        }
        options.Add(new InteractionOption("Talk to Leader", () => Debug.Log("Attempting to spaek")));
        options.Add(new InteractionOption("Leave", () => Debug.Log("Attempting to Leave")));


        return options;
    }

    public void SetSpawnerManager(SpawnerManager manager) // should only be used for bandits?
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

    public void SetLordProfile(LordProfile profile)
    {
        lord = profile;
    }


}
