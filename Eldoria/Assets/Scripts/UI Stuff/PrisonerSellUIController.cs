using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrisonerSellUIController : MonoBehaviour, ICardHandler<SoldierInstance>, IMenuWithSource
{

    [SerializeField] private Transform currentPrisonersParent;
    [SerializeField] private Transform prisonersToSellParent;
    [SerializeField] private GameObject prisonerPrefab;
    private PartyController playerParty;
    [SerializeField] private TextMeshProUGUI accumulatedCostText;

    [SerializeField] private Button confirmButton;

    private List<SoldierInstance> curentPrisoners = new();
    private List<SoldierInstance> prisonersToSell = new();
    private int accumulatedCost = 0;

    void Awake()
    {
        confirmButton.onClick.AddListener(() =>
        {
            // remove sold prisoners from party
            foreach (SoldierInstance prisoner in prisonersToSell)
            {
                playerParty.ReleasePrisoner(prisoner);
            }

            prisonersToSell.Clear();
            // add gold to player profile
            GameManager.Instance.PlayerProfile.AddGold(accumulatedCost);

            // reset stuff
            PopulateGrids();
            accumulatedCost = 0;
            UpdateAccumulatedCostText();
        });

    }
    public void OpenMenu(object source)
    {
        playerParty = GameManager.Instance.player.GetComponent<PartyController>();
        if (playerParty == null)
        {
            Debug.LogWarning("Player Party Controller not found");
            return;
        }
        curentPrisoners.Clear();
        accumulatedCost = 0;

        curentPrisoners.AddRange(playerParty.Prisoners);

        PopulateGrids();

        UpdateAccumulatedCostText();

    }

    private void UpdateAccumulatedCostText()
    {
        accumulatedCostText.text = "Gold: " + accumulatedCost.ToString();
    }

    private void PopulateGrids()
    {
        foreach (Transform child in currentPrisonersParent)
            Destroy(child.gameObject);

        foreach (Transform recruit in prisonersToSellParent)
        {
            Destroy(recruit.gameObject);
        }

        foreach (SoldierInstance recruit in curentPrisoners)
        {
            var card = Instantiate(prisonerPrefab, currentPrisonersParent);

            card.GetComponent<PartyMemberUI>().Setup(recruit, this);
        }
        foreach (SoldierInstance recruit in prisonersToSell)
        {
            var card = Instantiate(prisonerPrefab, prisonersToSellParent);

            card.GetComponent<PartyMemberUI>().Setup(recruit, this);
        }


    }

    public void OnCardClicked(SoldierInstance unit)
    {

        if (curentPrisoners.Contains(unit))
        {
            curentPrisoners.Remove(unit);
            prisonersToSell.Add(unit);
            accumulatedCost += CalculateCost(unit.unitData);
        }
        else if (prisonersToSell.Contains(unit))
        {
            prisonersToSell.Remove(unit);
            curentPrisoners.Add(unit);
            accumulatedCost -= CalculateCost(unit.unitData);
        }
        else
        {
            Debug.Log("Seemingly impossible result reached...");
        }

        PopulateGrids();
        UpdateAccumulatedCostText();

    }

    private int CalculateCost(UnitData unitData)
    {
        int DIVISOR = 2;
        return unitData.recruitmentCost / DIVISOR;
    }


}
