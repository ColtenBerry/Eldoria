using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Settlement))]
public class RecruitmentSource : MonoBehaviour, IInteractable
{
    [SerializeField] private List<UnitData> spawnableUnits = new(); // possible spawns

    [SerializeField] private List<UnitInstance> recruitableUnits = new(); // actual list of recruits

    void Awake()
    {
        CalculatePotentiallyRecruitableUnits();
    }

    public List<UnitInstance> GetRecruitableUnits()
    {
        return recruitableUnits;
    }

    public void CalculatePotentiallyRecruitableUnits()
    {
        recruitableUnits.Clear();
        // generate unit list

        // get prosperity
        int prosperity = GetComponent<Settlement>().GetProsperity();

        // get total # units
        int totalUnitsRecruitable = prosperity / 100;

        if (totalUnitsRecruitable < 2) totalUnitsRecruitable = 2; // set min
        if (totalUnitsRecruitable > 20) totalUnitsRecruitable = 20; // set max

        // generate random list
        for (int i = 0; i < totalUnitsRecruitable; i++)
        {
            // pick a unit at random
            UnitData randUnit = spawnableUnits[Random.Range(0, spawnableUnits.Count)];

            // creat instance
            UnitInstance randUnitInstance = new UnitInstance(randUnit);

            // add unit to list
            recruitableUnits.Add(randUnitInstance);
        }

    }

    public void recruitUnit(UnitInstance unit)
    {
        recruitableUnits.Remove(unit);
    }

    public void Interact()
    {
        //
    }

    public List<InteractionOption> GetInteractionOptions()
    {
        List<InteractionOption> options = new();
        options.Add(new InteractionOption("Recruit", () => UIManager.Instance.OpenSubMenu("recruit", this)));
        return options;

    }
}
