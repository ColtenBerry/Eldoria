using System.Collections.Generic;
using System.Linq;
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

        int prosperity = GetComponent<Settlement>().GetProsperity();
        int totalUnitsRecruitable = Mathf.Clamp(prosperity / 200, 2, 20);
        int totalFunds = prosperity;

        // Separate units into tiers
        var lowTier = spawnableUnits.Where(u => u.recruitmentCost <= 200).ToList();
        var midTier = spawnableUnits.Where(u => u.recruitmentCost > 200 && u.recruitmentCost <= 400).ToList();
        var highTier = spawnableUnits.Where(u => u.recruitmentCost > 400).ToList();

        // Fill majority with low-tier units
        int lowCount = Mathf.RoundToInt(totalUnitsRecruitable * 0.6f);
        int midCount = Mathf.RoundToInt(totalUnitsRecruitable * 0.2f);
        int highCount = totalUnitsRecruitable - lowCount - midCount;

        AddUnits(lowTier, lowCount, ref totalFunds);
        AddUnits(midTier, midCount, ref totalFunds);
        AddUnits(highTier, highCount, ref totalFunds);
    }

    private void AddUnits(List<SoldierData> pool, int count, ref int funds)
    {
        for (int i = 0; i < count; i++)
        {
            if (pool.Count == 0) return;

            SoldierData unit = pool[Random.Range(0, pool.Count)];
            if (funds >= unit.recruitmentCost)
            {
                recruitableUnits.Add(new SoldierInstance(unit));
                funds -= unit.recruitmentCost;
            }
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
