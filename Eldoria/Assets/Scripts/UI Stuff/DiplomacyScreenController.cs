using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiplomacyScreenController : MonoBehaviour, ICardHandler<Faction>
{
    [SerializeField] private TextMeshProUGUI castlesText;
    [SerializeField] private TextMeshProUGUI townsText;
    [SerializeField] private TextMeshProUGUI villagesText;
    [SerializeField] private TextMeshProUGUI lordsText;

    [SerializeField] private Button peaceOrWarButton;

    [SerializeField] private Transform factionsTransform;

    [SerializeField] private FactionItemUI factionItemPrefab;

    private List<Faction> factionsList = new List<Faction>();
    private List<FactionItemUI> factionItemUIs = new List<FactionItemUI>();
    private int selectedIndex = 0;

    private Faction selectedFaction;
    private Faction playerFaction;
    private void OnEnable()
    {
        RefreshList();
        PopulateContent();
    }

    private void Awake()
    {
        RefreshList(); // just for safety
        PopulateContent();
        peaceOrWarButton.onClick.AddListener(() =>
        {
            // declare war or peace

            // if enemies
            if (FactionsManager.Instance.AreEnemies(playerFaction, selectedFaction))
            {
                // request peace
                // TOOD: this should request peace and let the faction war manger sort whether
                FactionsManager.Instance.GetWarManager(selectedFaction).ConsiderPeace(playerFaction);
            }
            // if friends
            else
            {
                // declare war
                FactionsManager.Instance.DeclareWar(playerFaction, selectedFaction);
            }
            SetButtonText();
            //selectedFactionItem.RefreshVisuals(); // for some reason this doesn't work

            PopulateContent();
        });
    }

    // no source needed

    private void RefreshList()
    {
        // refresh the factionslist
        factionsList.Clear();
        factionsList.AddRange(FactionsManager.Instance.AllFactions);

        playerFaction = GameManager.Instance.PlayerProfile.Faction;

    }

    private void PopulateContent()
    {
        // Clear content
        foreach (Transform go in factionsTransform)
        {
            Destroy(go.gameObject);
        }

        // populate
        foreach (Faction faction in factionsList)
        {
            FactionItemUI item = Instantiate(factionItemPrefab, factionsTransform);
            item.Setup(faction, this);
            factionItemUIs.Add(item);
        }

        //
        if (factionsList.Count < selectedIndex)
        {
            Debug.LogWarning("faction list count changed");
            return;
        }
        SelectFaction(factionsList[selectedIndex]);
    }

    public void OnCardClicked(Faction data)
    {
        SelectFaction(data);
    }

    private void SelectFaction(Faction faction)
    {
        selectedFaction = faction;
        //selectedFactionItem = factionItemUIs.Where(item => item.Faction == faction).FirstOrDefault();
        selectedIndex = factionItemUIs.FindIndex(item => item.Faction == selectedFaction);
        // populate the fields
        int numCastles = TerritoryManager.Instance.GetSettlementsOfFaction(faction).Where(c => c is Castle).Count();
        // int numTowns = TerritoryManager.Instance.GetSettlementsOfFaction(data).Where(c => c is Town).Count()
        int numVillages = TerritoryManager.Instance.GetSettlementsOfFaction(faction).Where(c => c is Village).Count();
        int numLords = FactionsManager.Instance.GetLordsOfFaction(faction).Count;
        castlesText.text = $"Castles: {numCastles}";
        townsText.text = "not yet";
        villagesText.text = $"Villages: {numVillages}";
        lordsText.text = $"Lords: {numLords}";

        SetButtonText();
        //selectedFactionItem.RefreshVisuals();
    }

    private void SetButtonText()
    {
        // set the button text
        TextMeshProUGUI text = peaceOrWarButton.GetComponentInChildren<TextMeshProUGUI>();
        text.text = FactionsManager.Instance.AreEnemies(playerFaction, selectedFaction) ? "Request Peace" : "Declare War";
    }



}
