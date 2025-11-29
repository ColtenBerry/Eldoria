using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Settlement))]
public class RecruitmentSource : MonoBehaviour, IInteractable
{
    [SerializeField] private List<SoldierData> spawnableUnits = new(); // possible spawns

    [SerializeField] private List<SoldierInstance> recruitableUnits = new(); // actual list of recruits

    void Start()
    {
        TickManager.Instance.OnWeekPassed += RepopulateRecruitableUnits;
    }

    void OnDisable()
    {
        TickManager.Instance.OnWeekPassed -= RepopulateRecruitableUnits;
    }

    void Awake()
    {
        CalculatePotentiallyRecruitableUnits();
    }

    private void RepopulateRecruitableUnits(int i)
    {
        CalculatePotentiallyRecruitableUnits();
    }

    public List<SoldierInstance> GetRecruitableUnits()
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
        int totalUnitsRecruitable = prosperity / 200;

        if (totalUnitsRecruitable < 2) totalUnitsRecruitable = 2; // set min
        if (totalUnitsRecruitable > 20) totalUnitsRecruitable = 20; // set max

        // generate random list
        for (int i = 0; i < totalUnitsRecruitable; i++)
        {
            // pick a unit at random
            UnitData randUnit = spawnableUnits[Random.Range(0, spawnableUnits.Count)];

            // creat instance
            SoldierInstance randUnitInstance = new SoldierInstance(randUnit);

            // add unit to list
            recruitableUnits.Add(randUnitInstance);
        }

    }

    public void recruitUnit(SoldierInstance unit)
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
