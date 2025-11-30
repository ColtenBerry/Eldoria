using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrisonerSellUIController : MonoBehaviour, ICardHandler<SoldierInstance>, ICardHandler<CharacterInstance>, IMenuWithSource
{

    [SerializeField] private Transform currentPrisonersParent;
    [SerializeField] private Transform prisonersToSellParent;
    [SerializeField] private GameObject prisonerPrefab;
    [SerializeField] private CharacterUI characterPrefab;
    private PartyController playerParty;
    [SerializeField] private TextMeshProUGUI accumulatedCostText;

    [SerializeField] private Button confirmButton;

    private List<UnitInstance> curentPrisoners = new();
    private List<UnitInstance> prisonersToSell = new();
    private int accumulatedCost = 0;

    void Awake()
    {
        confirmButton.onClick.AddListener(() =>
        {
            // remove sold prisoners from party
            foreach (UnitInstance prisoner in prisonersToSell)
            {
                if (prisoner is SoldierInstance soldier)
                {
                    playerParty.ReleasePrisoner(soldier);
                }
                else if (prisoner is CharacterInstance character)
                {
                    playerParty.ReleasePrisoner(prisoner);
                    Debug.Log("Selling Character!");
                    LordProfile p = LordRegistry.Instance.GetLordByName(character.UnitName);
                    FactionWarManager warManager = FactionsManager.Instance.GetWarManager(p.Faction);
                    if (warManager == null)
                    {
                        // faction must have been a bandit
                    }
                    else
                    {
                        warManager.AddToPendingRespawns(p);
                    }
                }
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


    // TODO: repeated code
    private void PopulateGrids()
    {
        foreach (Transform child in currentPrisonersParent)
            Destroy(child.gameObject);

        foreach (Transform recruit in prisonersToSellParent)
        {
            Destroy(recruit.gameObject);
        }

        foreach (UnitInstance recruit in curentPrisoners)
        {
            if (recruit is SoldierInstance soldier)
            {
                var card = Instantiate(prisonerPrefab, currentPrisonersParent);

                card.GetComponent<PartyMemberUI>().Setup(soldier, this);

            }
            else if (recruit is CharacterInstance character)
            {
                CharacterUI card = Instantiate(characterPrefab, currentPrisonersParent);
                card.Setup(character, this);
            }
        }
        foreach (UnitInstance recruit in prisonersToSell)
        {
            if (recruit is SoldierInstance soldier)
            {
                var card = Instantiate(prisonerPrefab, prisonersToSellParent);

                card.GetComponent<PartyMemberUI>().Setup(soldier, this);

            }
            else if (recruit is CharacterInstance character)
            {
                CharacterUI card = Instantiate(characterPrefab, prisonersToSellParent);
                card.Setup(character, this);
            }


        }
    }


    // TODO: same code for each, maybe centralize? 
    public void OnCardClicked(SoldierInstance unit)
    {

        if (curentPrisoners.Contains(unit))
        {
            curentPrisoners.Remove(unit);
            prisonersToSell.Add(unit);
            accumulatedCost += CalculateCost(unit.soldierData);
        }
        else if (prisonersToSell.Contains(unit))
        {
            prisonersToSell.Remove(unit);
            curentPrisoners.Add(unit);
            accumulatedCost -= CalculateCost(unit.soldierData);
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



    public void OnCardClicked(CharacterInstance data)
    {
        if (curentPrisoners.Contains(data))
        {
            curentPrisoners.Remove(data);
            prisonersToSell.Add(data);
            accumulatedCost += CalculateCost(data.characterData);
        }
        else if (prisonersToSell.Contains(data))
        {
            prisonersToSell.Remove(data);
            curentPrisoners.Add(data);
            accumulatedCost -= CalculateCost(data.characterData);
        }
        else
        {
            Debug.Log("Seemingly impossible result reached...");
        }

        PopulateGrids();
        UpdateAccumulatedCostText();
    }
}
