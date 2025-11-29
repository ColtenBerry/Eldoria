using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class PartyUI : MonoBehaviour, ICardHandler<SoldierInstance>
{
    public PartyController partyController;
    public Transform partyParent; // e.g. a Vertical Layout Group
    public Transform prisonerParent;
    public PartyMemberUI partyMemberUIPrefab; //Prefab for unit display

    [SerializeField] private Image spriteImage;

    public Button upgradeButton;

    private SoldierInstance selectedUnit;

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
            List<SoldierInstance> upgradableUnits = partyController.PartyMembers.Where(unit => unit.CanUpgrade).ToList();

            // open upgrade menu with new units
            UpgradeObject upgradeObject = new UpgradeObject(selectedUnit, selectedUnit.soldierData.upgradeOptions);
            UIManager.Instance.OpenUpgradeUnitMenu(upgradeObject);
        });
    }

    private void RefreshUI()
    {
        spriteImage.sprite = null;
        spriteImage.color = new Color(spriteImage.color.r, spriteImage.color.g, spriteImage.color.b, 0.0f);
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

    private void UpdateSpriteImage()
    {
        spriteImage.color = new Color(spriteImage.color.r, spriteImage.color.g, spriteImage.color.b, 1.0f);
        spriteImage.sprite = selectedUnit.soldierData.sprite;
    }

    public void OnCardClicked(SoldierInstance unit)
    {
        selectedUnit = unit;
        if (unit.CanUpgrade)
        {
            upgradeButton.interactable = true;
        }
        else
        {
            upgradeButton.interactable = false;
        }
        UpdateSpriteImage();
    }
}
