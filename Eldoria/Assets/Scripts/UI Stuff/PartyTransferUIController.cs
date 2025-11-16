using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyTransferUIController : MonoBehaviour, IMenuWithSource, ICardHandler<UnitInstance>
{

    [SerializeField] private Transform otherPartyGrid;
    [SerializeField] private Transform playerPartyGrid;
    [SerializeField] private GameObject partyMemberPrefab;
    [SerializeField] private TextMeshProUGUI otherPartyNameText;
    [SerializeField] private TextMeshProUGUI playerPartyNameText;
    [SerializeField] private Button confirmButton;

    private PartyController playerParty;
    private PartyController otherParty;

    private List<UnitInstance> otherPartyPotentialResult = new();
    private List<UnitInstance> playerPartyPotentialResult = new();

    public void Awake()
    {
        confirmButton.onClick.AddListener(() =>
        {
            // Clear current members
            otherParty.PartyMembers.Clear();
            playerParty.PartyMembers.Clear();

            // Add new members based on potential results
            foreach (var member in otherPartyPotentialResult)
            {
                otherParty.AddUnit(member);
            }
            foreach (var member in playerPartyPotentialResult)
            {
                playerParty.AddUnit(member);
            }

            // Close menu
            UIManager.Instance.CloseAllMenus();
        });
    }

    public void OpenMenu(object source)
    {
        if (source is PartyTransferMenuContext ctx)
        {
            otherParty = ctx.party1;
            playerParty = ctx.playerParty;

            otherPartyNameText.text = otherParty.gameObject.name;
            playerPartyNameText.text = playerParty.gameObject.name;

            otherPartyPotentialResult = new List<UnitInstance>(otherParty.PartyMembers);
            playerPartyPotentialResult = new List<UnitInstance>(playerParty.PartyMembers);

            PopulateGrids();
        }

    }

    private void PopulateGrids()
    {
        // clear old items
        foreach (Transform child in otherPartyGrid)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in playerPartyGrid)
        {
            Destroy(child.gameObject);
        }

        // populate other party members
        foreach (var member in otherPartyPotentialResult)
        {
            var itemGO = Instantiate(partyMemberPrefab, otherPartyGrid);
            PartyMemberUI itemController = itemGO.GetComponent<PartyMemberUI>();
            itemController.Setup(member, this);
        }

        // populate player party members
        foreach (var member in playerPartyPotentialResult)
        {
            var itemGO = Instantiate(partyMemberPrefab, playerPartyGrid);
            PartyMemberUI itemController = itemGO.GetComponent<PartyMemberUI>();
            itemController.Setup(member, this);
        }
    }
    public void OnCardClicked(UnitInstance unit)
    {
        if (playerPartyPotentialResult.Contains(unit))
        {
            playerPartyPotentialResult.Remove(unit);
            otherPartyPotentialResult.Add(unit);
        }
        else if (otherPartyPotentialResult.Contains(unit))
        {
            playerPartyPotentialResult.Add(unit);
            otherPartyPotentialResult.Remove(unit);
        }
        else
        {
            Debug.Log("Seemingly impossible result reached...");
        }

        PopulateGrids();

        // check if confirm button should be interactable
        confirmButton.interactable = (otherParty.MaxPartyMembers > otherPartyPotentialResult.Count) && (playerParty.MaxPartyMembers > playerPartyPotentialResult.Count);
    }

}


public class PartyTransferMenuContext
{
    public PartyController party1;
    public PartyController playerParty;

    public PartyTransferMenuContext(PartyController party1, PartyController playerParty)
    {
        this.party1 = party1;
        this.playerParty = playerParty;
    }
}

