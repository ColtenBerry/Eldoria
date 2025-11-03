using System.Collections.Generic;
using UnityEngine;

public class Village : Settlement
{

    public override List<InteractionOption> GetInteractionOptions()
    {
        List<InteractionOption> options = new();

        if (FactionsManager.Instance.AreEnemies(GetFaction(), FactionsManager.Instance.GetFactionByName("Player"))) options.Add(new InteractionOption("Raid", () => Debug.Log("Attempting to Raid")));
        options.Add(new InteractionOption("Trade", () => Debug.Log("Attempting to Trade"), "trade"));
        options.Add(new InteractionOption("Recruit", () => Debug.Log("Attempting to Recruit"), "recruit"));
        options.Add(new InteractionOption("Leave", () =>
        {
            Debug.Log("Attempting to leave");
            InputGate.OnMenuClosed?.Invoke();

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