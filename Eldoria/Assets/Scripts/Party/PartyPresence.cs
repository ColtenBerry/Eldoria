using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class PartyPresence : MonoBehaviour, IInteractable
{
    public PartyProfile partyProfile;
    private PartyController partyController;

    [SerializeField] CharacterInstance lord;
    public CharacterInstance Lord
    {
        get
        {
            return lord;
        }
    }

    void Awake()
    {
        partyController = GetComponent<PartyController>();
        if (partyController == null) Debug.LogError("PartyController not found on PartyPresence GameObject");
        InitializeParty();
        ApplyVisuals();

        lord = new CharacterInstance(partyProfile.Lord);
    }

    private void InitializeParty()
    {
        foreach (UnitData unit in partyProfile.startingUnits)
        {
            Debug.Log("adding unit: presence");
            partyController.AddUnit(unit);
        }
    }

    private void ApplyVisuals()
    {
        // set icon, color, etc.
    }

    public void Interact()
    {
        Debug.Log("Attempting interaction with " + lord.UnitName);
    }

    public List<InteractionOption> GetInteractionOptions()
    {
        List<InteractionOption> options = new();

        if (lord.Faction != "Player") options.Add(new InteractionOption("Attack Party", () => Debug.Log("Attempting to Attack")));
        options.Add(new InteractionOption("Talk to Leader", () => Debug.Log("Attempting to spaek")));
        options.Add(new InteractionOption("Leave", () => Debug.Log("Attempting to Leave")));


        return options;
    }
}
