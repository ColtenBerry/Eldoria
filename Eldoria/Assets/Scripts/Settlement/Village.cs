using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Village : Settlement
{
    [SerializeField] private TextMeshProUGUI nameText;
    public override void ApplyVisuals()
    {
        nameText.text = gameObject.name;
    }

    public override List<InteractionOption> GetInteractionOptions()
    {
        List<InteractionOption> options = new();

        if (FactionsManager.Instance.AreEnemies(GetFaction(), FactionsManager.Instance.GetFactionByName("Player"))) options.Add(new InteractionOption("Raid", () => Debug.Log("Attempting to Raid")));
        options.Add(new InteractionOption("Trade", () => UIManager.Instance.OpenSubMenu("trade", this)));
        options.Add(new InteractionOption("Leave", () =>
        {
            UIManager.Instance.CloseAllMenus();

        })
        );


        return options;
    }

    public override void Interact()
    {
        Debug.Log("Attempting interaction with " + settlementName);
        // controller.AddUnit(militiaTroop);

    }
}