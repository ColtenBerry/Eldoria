using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUIController : MonoBehaviour, IMenuWithSource
{
    [SerializeField] private PartyController playerPartyController; // i don't like this here, but this is the easiest way to update partyui

    [SerializeField] private TextMeshProUGUI previousTroopTextName;
    [SerializeField] private TextMeshProUGUI previousTroopTextStats;
    [SerializeField] private TextMeshProUGUI upgradedTroopTextName;
    [SerializeField] private TextMeshProUGUI upgradedTroopTextStats;
    [SerializeField] private TextMeshProUGUI upgradeCostText;
    [SerializeField] private Button closeMenuButton;

    private UnitData currentUnitSelected;
    private UpgradeObject upgradeData;

    [SerializeField] private Transform upgradeOptionsLayout;
    [SerializeField] private ToggleGroup upgradeOptionsToggleGroup;
    [SerializeField] private UpgradeOptionToggle toggleOptionPrefab;

    [SerializeField] private Button upgradeButton;

    private void Awake()
    {
        upgradeButton.onClick.AddListener(() =>
        {
            // upgrade unit
            upgradeData.previousUnit.ApplyUpgrade(currentUnitSelected);

            // alert partymenu ui that something changed
            playerPartyController.NotifyPartyUpdated();

            GameManager.Instance.PlayerProfile.ChangeGold(-currentUnitSelected.upgradeCost);

            UIManager.Instance.CloseUpgradeUnitMenu();

        });

        closeMenuButton.onClick.AddListener(() =>
        {
            UIManager.Instance.CloseUpgradeUnitMenu();
        });
    }

    public void OpenMenu(object source)
    {
        upgradeData = source as UpgradeObject;
        if (upgradeData == null)
        {
            Debug.LogError("UpgradeUIController.OpenMenu received invalid source type.");
            return;
        }

        previousTroopTextName.text = upgradeData.previousUnit.baseData.unitName;
        string[] lines = { $"Health: {upgradeData.previousUnit.baseData.health}", $"Attack: {upgradeData.previousUnit.baseData.attack}", $"Defense: {upgradeData.previousUnit.baseData.defence}" };
        previousTroopTextStats.text = string.Join("\n", lines);

        currentUnitSelected = upgradeData.unitOptions.First();
        PopulateUpgradeOptionInfo(currentUnitSelected);
        PopulateUpgradeOptions(upgradeData.unitOptions);

    }

    private void PopulateUpgradeOptionInfo(UnitData unit)
    {
        upgradedTroopTextName.text = unit.unitName;
        string[] lines = { $"Health: {unit.health}", $"Attack: {unit.attack}", $"Defense: {unit.defence}" };
        upgradedTroopTextStats.text = string.Join("\n", lines);
    }

    private void PopulateUpgradeOptions(List<UnitData> options)
    {
        foreach (Transform child in upgradeOptionsLayout)
        {
            Destroy(child.gameObject);
        }

        foreach (UnitData unitData in options)
        {
            UpgradeOptionToggle newToggle = Instantiate(toggleOptionPrefab, upgradeOptionsLayout);
            newToggle.Initialize(unitData, upgradeOptionsToggleGroup);

            Toggle toggle = newToggle.GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(_ => UpdateSelectedUnitFromToggle());
        }

    }

    private void UpdateSelectedUnitFromToggle()
    {
        Toggle selectedToggle = upgradeOptionsToggleGroup.ActiveToggles().FirstOrDefault();
        if (selectedToggle == null) return;

        UpgradeOptionToggle selectedOption = selectedToggle.GetComponent<UpgradeOptionToggle>();
        if (selectedOption == null) return;

        upgradeCostText.text = $"Cost: {selectedOption.GetUnitData().upgradeCost}";

        if (GameManager.Instance.PlayerProfile.CanAfford(selectedOption.GetUnitData().upgradeCost))
        {
            upgradeCostText.color = Color.white;
            upgradeButton.interactable = true;
        }
        else
        {
            upgradeCostText.color = Color.red;
            upgradeButton.interactable = false;
        }


        currentUnitSelected = selectedOption.GetUnitData();
        PopulateUpgradeOptionInfo(currentUnitSelected);
    }





}

public class UpgradeObject
{
    public UnitInstance previousUnit;
    public List<UnitData> unitOptions;

    public UpgradeObject(UnitInstance previousUnit, List<UnitData> unitOptions)
    {
        this.previousUnit = previousUnit;
        this.unitOptions = unitOptions;
    }
}
