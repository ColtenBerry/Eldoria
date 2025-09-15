using UnityEngine;
using System.Collections.Generic;

public class PartyUI : MonoBehaviour
{
    public PartyController partyController;
    public Transform partyParent; // e.g. a Vertical Layout Group
    public Transform prisonerParent;
    public PartyMemberUI partyMemberUIPrefab;

    //private List<PartyMemberUI> activeUIElements = new();

    void OnEnable()
    {
        partyController.OnPartyUpdated += RefreshUI;
        partyController.OnPrisonersUpdated += RefreshPrisonerUI;
        RefreshUI();
        RefreshPrisonerUI();
    }

    void OnDisable()
    {
        partyController.OnPartyUpdated -= RefreshUI;
        partyController.OnPrisonersUpdated -= RefreshPrisonerUI;
    }

    void RefreshUI()
    {
        // Clear old UI
        foreach (Transform go in partyParent)
            Destroy(go.gameObject);
        // activeUIElements.Clear();

        // Add Lord
        var lordUI = Instantiate(partyMemberUIPrefab, partyParent);
        lordUI.GetComponent<PartyMemberUI>().Setup(partyController.Lord);
        //  activeUIElements.Add(lordUI);

        // Add Party Members
        foreach (var member in partyController.PartyMembers)
        {
            var ui = Instantiate(partyMemberUIPrefab, partyParent);
            ui.GetComponent<PartyMemberUI>().Setup(member);
            //      activeUIElements.Add(ui);
        }
    }

    void RefreshPrisonerUI()
    {
        // Clear and repopulate prisoner UI
        foreach (Transform child in prisonerParent)
            Destroy(child.gameObject);

        foreach (var prisoner in partyController.Prisoners)
        {
            var ui = Instantiate(partyMemberUIPrefab, prisonerParent);
            ui.GetComponent<PartyMemberUI>().Setup(prisoner);
            // Optional: visually tag as prisoner
        }
    }
}
