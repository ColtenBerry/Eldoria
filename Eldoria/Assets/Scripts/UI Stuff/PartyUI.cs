using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class PartyUI : MonoBehaviour, ICardHandler<UnitInstance>
{
    public PartyController partyController;
    public Transform partyParent; // e.g. a Vertical Layout Group
    public Transform prisonerParent;
    public PartyMemberUI partyMemberUIPrefab; //Prefab for unit display

    public Button upgradeButton;

    private UnitInstance selectedUnit;

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

    void Awake()
    {
        upgradeButton.onClick.AddListener(() =>
        {
            // open upgrade menu. 

            // get all upgradable units:
            List<UnitInstance> upgradableUnits = partyController.PartyMembers.Where(unit => unit.CanUpgrade).ToList();

            // open upgrade menu with new units
            UpgradeObject upgradeObject = new UpgradeObject(selectedUnit, selectedUnit.baseData.upgradeOptions);
            UIManager.Instance.OpenUpgradeUnitMenu(upgradeObject);
        });
    }

    private void RefreshUI()
    {
        upgradeButton.interactable = false;
        // Clear old UI
        foreach (Transform go in partyParent)
            Destroy(go.gameObject);
        // activeUIElements.Clear();

        // Add Lord
        //var lordUI = Instantiate(partyMemberUIPrefab, partyParent);
        //lordUI.GetComponent<PartyMemberUI>().Setup(partyController.Lord);
        //  activeUIElements.Add(lordUI);

        // Add Party Members
        foreach (var member in partyController.PartyMembers)
        {
            var ui = Instantiate(partyMemberUIPrefab, partyParent);
            ui.GetComponent<PartyMemberUI>().Setup(member, this);
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
            ui.GetComponent<PartyMemberUI>().Setup(prisoner, this);
            // Optional: visually tag as prisoner
        }
    }

    public void OnCardClicked(UnitInstance unit)
    {
        if (unit.CanUpgrade)
        {
            upgradeButton.interactable = true;
            selectedUnit = unit;
        }
        else
        {
            upgradeButton.interactable = false;
            selectedUnit = null;
        }
    }
}
