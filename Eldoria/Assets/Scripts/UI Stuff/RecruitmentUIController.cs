using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecruitmentUIController : MenuController, IMenuWithSource, ICardHandler<SoldierInstance>
{
    [SerializeField] private Transform recruitOptionsParent;
    [SerializeField] private Transform potentialRecruitParent;
    [SerializeField] private GameObject recruitPrefab;
    [SerializeField] private PartyController playerParty;
    [SerializeField] private TextMeshProUGUI accumulatedCostText;

    [SerializeField] private Button confirmButton;

    private List<SoldierInstance> recruitOptions = new();
    private List<SoldierInstance> potentialRecruits = new();
    private RecruitmentSource recruitmentSource;
    private int accumulatedCost = 0;

    void Awake()
    {
        confirmButton.onClick.AddListener(() =>
        {
            // add troops to party
            foreach (SoldierInstance unit in potentialRecruits)
            {
                RecruitmentUtility.TryRecruitUnit(unit, playerParty, GameManager.Instance.PlayerProfile, recruitmentSource);
            }
            potentialRecruits.Clear();
            GetRecruits();
            PopulateGrids();
            accumulatedCost = 0;
            UpdateAccumulatedCostText();
        });

    }
    public void OpenMenu(object source)
    {
        potentialRecruits.Clear();
        accumulatedCost = 0;

        // Step 1: Cast the interface back to a Component
        var component = source as Component;
        if (component == null)
        {
            Debug.LogWarning("RecruitmentScreen expected a Component-based interface");
            return;
        }

        // Step 2: Get the RecruitmentSource from the same GameObject
        recruitmentSource = component.GetComponent<RecruitmentSource>();
        if (recruitmentSource == null)
        {
            Debug.LogWarning("RecruitmentSource not found on source GameObject");
            return;
        }

        GetRecruits();

        PopulateGrids();

        UpdateAccumulatedCostText();

    }

    private void UpdateAccumulatedCostText()
    {
        accumulatedCostText.text = "Gold: " + accumulatedCost.ToString();
        if (GameManager.Instance.PlayerProfile != null)
        {
            if (GameManager.Instance.PlayerProfile.CanAfford(accumulatedCost))
            {
                accumulatedCostText.color = Color.white;
                confirmButton.interactable = true;
            }
            else
            {
                accumulatedCostText.color = Color.red;
                confirmButton.interactable = false;
            }
        }
    }

    private void PopulateGrids()
    {
        // clear recruitoptions
        foreach (Transform child in recruitOptionsParent)
            Destroy(child.gameObject);

        // populate recruitoptions
        foreach (SoldierInstance recruit in recruitOptions)
        {
            var card = Instantiate(recruitPrefab, recruitOptionsParent);

            card.GetComponent<PartyMemberUI>().Setup(recruit, this);
        }

        // clear potential recruits
        foreach (Transform recruit in potentialRecruitParent)
        {
            Destroy(recruit.gameObject);
        }

        foreach (SoldierInstance recruit in potentialRecruits)
        {
            var card = Instantiate(recruitPrefab, potentialRecruitParent);

            card.GetComponent<PartyMemberUI>().Setup(recruit, this);
        }


    }

    private void GetRecruits()
    {
        recruitOptions.Clear();
        foreach (var item in recruitmentSource.GetRecruitableUnits())
        {
            recruitOptions.Add(item);
        }
    }

    public void OnCardClicked(SoldierInstance unit)
    {

        if (potentialRecruits.Contains(unit))
        {
            potentialRecruits.Remove(unit);
            recruitOptions.Add(unit);
            accumulatedCost -= unit.unitData.recruitmentCost;
        }
        else if (recruitOptions.Contains(unit))
        {
            potentialRecruits.Add(unit);
            recruitOptions.Remove(unit);
            accumulatedCost += unit.unitData.recruitmentCost;
        }
        else
        {
            Debug.Log("Seemingly impossible result reached...");
        }

        PopulateGrids();
        UpdateAccumulatedCostText();

    }


}
