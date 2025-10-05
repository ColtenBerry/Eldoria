using System.Collections.Generic;
using UnityEngine;

public class RecruitmentUIController : MonoBehaviour, IMenuWithSource
{
    [SerializeField] private Transform recruitOptionsParent;
    [SerializeField] private Transform potentialRecruitParent;
    [SerializeField] private GameObject recruitPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void OpenMenu(object source)
    {
        // Step 1: Cast the interface back to a Component
        var component = source as Component;
        if (component == null)
        {
            Debug.LogWarning("RecruitmentScreen expected a Component-based interface");
            return;
        }

        // Step 2: Get the RecruitmentSource from the same GameObject
        var recruitmentSource = component.GetComponent<RecruitmentSource>();
        if (recruitmentSource == null)
        {
            Debug.LogWarning("RecruitmentSource not found on source GameObject");
            return;
        }

        List<UnitInstance> potentialRecruits = recruitmentSource.GetRecruitableUnits();
        PopulateGrid(potentialRecruits);
    }

    private void PopulateGrid(List<UnitInstance> recruits)
    {
        foreach (Transform child in recruitOptionsParent)
            Destroy(child.gameObject);

        foreach (UnitInstance recruit in recruits)
        {
            var card = Instantiate(recruitPrefab, recruitOptionsParent);

            card.GetComponent<PartyMemberUI>().Setup(recruit);
        }
    }


}
