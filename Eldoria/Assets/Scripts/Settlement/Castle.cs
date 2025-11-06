using System.Collections.Generic;
using UnityEngine;

public class Castle : Settlement, IHasBoundVillages
{
    private PartyController garrisonController;

    public PartyController GarrisonController => garrisonController;

    [SerializeField] private int defenseBonus = 20;

    [SerializeField] private List<Village> boundVillages;

    public List<Village> BoundVillages => boundVillages;


    void Awake()
    {
        garrisonController = GetComponent<PartyController>();
        if (garrisonController == null) Debug.LogWarning("Expected party controller");
    }


    public override void Start()
    {
        base.Start();
        ApplyVisuals();

    }

    private void ApplyVisuals()
    {
        SpriteRenderer spriteRenderer = transform.Find("LeftFlag").GetComponent<SpriteRenderer>();
        spriteRenderer.color = GetFaction().factionColor;
        spriteRenderer = transform.Find("RightFlag").GetComponent<SpriteRenderer>();
        spriteRenderer.color = GetFaction().factionColor;
    }


    public override List<InteractionOption> GetInteractionOptions()
    {
        List<InteractionOption> options = new();

        if (FactionsManager.Instance.AreEnemies(GetFaction(), FactionsManager.Instance.GetFactionByName("Player")))
        {
            options.Add(new InteractionOption("Besiege Castle", () => BesiegeCastle()));
        }

        else if (TerritoryManager.Instance.GetLordOf(this) == GameManager.Instance.PlayerProfile)
        {
            // allow upgrades TODO: make an upgrade menu
            options.Add(new InteractionOption("Upgrade Defences", () => FortifyCastle()));

            options.Add(new InteractionOption("Wait Inside", () => WaitInsideCastle()));
        }

        else
        {
            options.Add(new InteractionOption("Wait Inside", () => WaitInsideCastle()));
        }

        options.Add(new InteractionOption("Leave", () => { InputGate.OnMenuClosed?.Invoke(); }));
        return options;
    }

    private void WaitInsideCastle()
    {
        // open wait menu
    }

    private void BesiegeCastle()
    {
        // open siege menu

        // pause any production

        // start timer for battlescreen to open
    }

    private void FortifyCastle()
    {
        defenseBonus += 5;
    }

    public override void Interact()
    {
        Debug.Log("Attempting interaction with castle");
    }

}
