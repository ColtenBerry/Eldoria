using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class PartyPresence : MonoBehaviour, IInteractable
{
    private PartyController partyController;
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
    public PartyController PartyController => partyController;

    void Awake()
    {
        partyController = GetComponent<PartyController>();
        if (partyController == null) Debug.LogError("PartyController not found on PartyPresence GameObject");

        ApplyVisuals();

    }

    void Start()
    {
        InitializeParty();

        gameObject.name = Lord.Lord.UnitName;
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
                CombatSimulator.StartBattle(gameObject.transform.position, FactionsManager.Instance.GetFactionByName("Player"), Lord.Faction, true);
            }
                ));
        }
        options.Add(new InteractionOption("Talk to Leader", () => Debug.Log("Attempting to spaek")));
        options.Add(new InteractionOption("Leave", () => UIManager.Instance.CloseAllMenus()));


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

    public void SetTransparency(GameObject root, float alpha)
    {
        foreach (var sr in root.GetComponentsInChildren<SpriteRenderer>(includeInactive: true))
        {
            Color color = sr.color;
            color.a = alpha;
            sr.color = color;
        }

        // Handle TextMeshPro components
        foreach (var tmp in root.GetComponentsInChildren<TextMeshPro>(includeInactive: true))
        {
            Color color = tmp.color;
            color.a = alpha;
            tmp.color = color;
        }

        foreach (var tmpUGUI in root.GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true))
        {
            Color color = tmpUGUI.color;
            color.a = alpha;
            tmpUGUI.color = color;
        }
    }

    public void WaitInFief()
    {
        // code
        gameObject.layer = LayerMask.NameToLayer("");
        SetTransparency(gameObject, 0.0f);
        partyController.SetIsHealing(true);
        GetComponent<CircleCollider2D>().isTrigger = false;
    }

    public void LeaveFief()
    {
        gameObject.layer = LayerMask.NameToLayer("Interactable");
        if (gameObject == GameManager.Instance.player) gameObject.layer = LayerMask.NameToLayer("Player");
        SetTransparency(gameObject, 1.0f);
        partyController.SetIsHealing(false);
        GetComponent<CircleCollider2D>().isTrigger = true;
    }
    internal bool IsInFief()
    {
        return gameObject.layer == LayerMask.NameToLayer("");
    }


}
