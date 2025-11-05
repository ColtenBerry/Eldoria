using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecruitmentUIController : MenuController, IMenuWithSource, ICardHandler<UnitInstance>
{
    [SerializeField] private Transform recruitOptionsParent;
    [SerializeField] private Transform potentialRecruitParent;
    [SerializeField] private GameObject recruitPrefab;
    [SerializeField] private PartyController playerParty;

    [SerializeField] private Button confirmButton;

    private List<UnitInstance> recruitOptions = new();
    private List<UnitInstance> potentialRecruits = new();
    private RecruitmentSource recruitmentSource;

    void Awake()
    {
        confirmButton.onClick.AddListener(() =>
        {
            // add troops to party
            foreach (UnitInstance unit in potentialRecruits)
            {
                if (playerParty.AddUnit(unit))
                    recruitmentSource.recruitUnit(unit);
            }
            potentialRecruits.Clear();
            GetRecruits();
            PopulateGrids();
        });

    }
    public void OpenMenu(object source)
    {
        potentialRecruits.Clear();

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

    }

    private void PopulateGrids()
    {
        // clear recruitoptions
        foreach (Transform child in recruitOptionsParent)
            Destroy(child.gameObject);

        // populate recruitoptions
        foreach (UnitInstance recruit in recruitOptions)
        {
            var card = Instantiate(recruitPrefab, recruitOptionsParent);

            card.GetComponent<PartyMemberUI>().Setup(recruit, this);
        }

        // clear potential recruits
        foreach (Transform recruit in potentialRecruitParent)
        {
            Destroy(recruit.gameObject);
        }

        foreach (UnitInstance recruit in potentialRecruits)
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

    public void OnCardClicked(UnitInstance unit)
    {

        if (potentialRecruits.Contains(unit))
        {
            potentialRecruits.Remove(unit);
            recruitOptions.Add(unit);
        }
        else if (recruitOptions.Contains(unit))
        {
            potentialRecruits.Add(unit);
            recruitOptions.Remove(unit);
        }
        else
        {
            Debug.Log("Seemingly impossible result reached...");
        }

        PopulateGrids();

    }


}
