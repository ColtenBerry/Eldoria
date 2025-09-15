using UnityEngine;

public class PartyPresence : MonoBehaviour, IInteractable
{
    public PartyProfile partyProfile;
    private PartyController partyController;

    void Awake()
    {
        partyController = GetComponent<PartyController>();
        if (partyController == null) Debug.LogError("PartyController not found on PartyPresence GameObject");
        InitializeParty();
        ApplyVisuals();
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
        Debug.Log("Attempting interaction with " + partyController.lordData.unitName);
    }
}
