using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(PartyController))]
public class Castle : SiegableSettlement, IHasBoundVillages
{

    // bound villages

    [SerializeField] private List<Village> boundVillages;

    [Header("Garrison")]


    public List<Village> BoundVillages => boundVillages;

    // siege

    private void Awake()
    {
        garrison = GetComponent<PartyController>();
        if (garrison == null)
            Debug.LogError("SiegeController requires a partycontroller component");
    }


    protected override void Start()
    {
        base.Start();
        ApplyVisuals();
    }

    public override void ApplyVisuals()
    {
        SpriteRenderer spriteRenderer = transform.Find("LeftFlag").GetComponent<SpriteRenderer>();
        spriteRenderer.color = GetFaction().factionColor;
        spriteRenderer = transform.Find("RightFlag").GetComponent<SpriteRenderer>();
        spriteRenderer.color = GetFaction().factionColor;


        TextMeshProUGUI castleNameText = transform.Find("Canvas/CastleNameText").GetComponent<TextMeshProUGUI>();
        castleNameText.text = gameObject.name;
    }

    public override List<InteractionOption> GetInteractionOptions()
    {
        List<InteractionOption> options = new();

        options.Add(new InteractionOption("Leave", () => { UIManager.Instance.CloseAllMenus(); }));
        return options;
    }



    public override void Interact()
    {
        Debug.Log("Attempting interaction with castle");
    }

    int FINALEXPERIENCEAMOUNT = 5;

    public override void AwardGarrisonXP(int tickCount)
    {
        int finalAmount = FINALEXPERIENCEAMOUNT;
        foreach (UnitInstance unit in garrison.PartyMembers)
        {
            unit.GainExperience(finalAmount);
        }
    }
}
