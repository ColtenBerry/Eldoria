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

    private readonly float defaultSpeed = 1.0f;
    [SerializeField, ReadOnly] private float speed;
    public float Speed => speed;

    void Awake()
    {
        partyController = GetComponent<PartyController>();
        if (partyController == null) Debug.LogError("PartyController not found on PartyPresence GameObject");
        // I don't love this but it does allow partypresence to listen to relevant changes in partycontroller w/o the controller referencing presence
        else
        {
            partyController.OnPartyUpdated += CalculateSpeed;
            partyController.OnPrisonersUpdated += CalculateSpeed;
        }


        ApplyVisuals();

    }

    void Start()
    {
        InitializeParty();

        gameObject.name = Lord.Lord.UnitName;
    }

    private void InitializeParty()
    {
        foreach (SoldierData unit in lord.StartingUnits)
        {
            Debug.Log("adding unit: presence");
            partyController.AddUnit(new SoldierInstance(unit));
        }
        FactionsManager.Instance.RegisterParty(this);

        CalculateSpeed();
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

    public void CalculateSpeed()
    {
        List<UnitInstance> members = new(partyController.PartyMembers)
        {
            Lord.Lord
        };
        int partySize = members?.Count ?? 0;
        int prisonerCount = partyController.Prisoners.Count;

        if (partySize == 0)
        {
            speed = defaultSpeed;
            return;
        }

        // Base average speed of units
        float totalSpeed = 0f;
        foreach (UnitInstance m in members)
            totalSpeed += m.Speed;
        float averageSpeed = totalSpeed / partySize;

        int SOFTCAP = 20; // later we can change this so characters improve their cap and softcap?
        // Party size modifier
        float sizeModifier;
        if (partySize <= 2) sizeModifier = 3f;
        else if (partySize <= 4)
            sizeModifier = 2f; // very quick
        else if (partySize <= 10)
            sizeModifier = 1.5f;  // quick
        else if (partySize <= 16)
            sizeModifier = 1.2f;  // fast 
        else if (partySize <= SOFTCAP)
            sizeModifier = 1.0f;  // normal
        else
            sizeModifier = 0.9f - ((partySize - SOFTCAP) * 0.05f); // harsher dropoff after soft max. lose 5% per partymember

        sizeModifier = Mathf.Max(sizeModifier, 0.05f); // clamp floor 5%

        // Prisoner penalty
        float prisonerModifier = 1f;
        int safeThreshold = partySize / 2;

        if (prisonerCount > safeThreshold)
        {
            int excess = prisonerCount - safeThreshold;
            prisonerModifier -= excess * 0.05f; // 5% per excess prisoner
        }

        prisonerModifier = Mathf.Max(prisonerModifier, 0.05f); // clamp floor 5%

        // Set final speed
        speed = defaultSpeed * averageSpeed * sizeModifier * prisonerModifier;
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
