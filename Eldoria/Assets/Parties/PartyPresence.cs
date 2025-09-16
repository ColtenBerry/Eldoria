using JetBrains.Annotations;
using UnityEngine;

public class PartyPresence : MonoBehaviour, IInteractable
{
    public PartyProfile partyProfile;
    private PartyController partyController;

    [SerializeField] UnitData lord;
    public UnitData Lord
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

        lord = partyProfile.Lord;
    }

    private void InitializeParty()
    {
        foreach (UnitData unit in partyProfile.startingUnits)
        {
            partyController.AddUnit(unit);
        }
    }

    private void ApplyVisuals()
    {
        // set icon, color, etc.
    }

    public void Interact()
    {
        Debug.Log("Attempting interaction with " + lord.unitName);
    }
}
