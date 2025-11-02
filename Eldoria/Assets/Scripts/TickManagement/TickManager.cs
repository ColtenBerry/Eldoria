using System;
using UnityEngine;

public class TickManager : MonoBehaviour
{
    public static TickManager Instance { get; private set; }

    [Header("Tick Settings")]
    [SerializeField] private int ticksPerDay = 10;
    [SerializeField] private int daysPerWeek = 7;

    public int TickCount { get; private set; } = 0;
    public int DayCount { get; private set; } = 0;
    public int WeekCount { get; private set; } = 0;

    public event Action<int> OnTick;
    public event Action<int> OnDayPassed;
    public event Action<int> OnWeekPassed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Advances the simulation by one tick.
    /// </summary>
    public void AdvanceTick()
    {
        TickCount++;
        OnTick?.Invoke(TickCount);

        if (TickCount % ticksPerDay == 0)
        {
            DayCount++;
            OnDayPassed?.Invoke(DayCount);

            if (DayCount % daysPerWeek == 0)
            {
                WeekCount++;
                OnWeekPassed?.Invoke(WeekCount);
            }
        }
    }

    /// <summary>
    /// Resets all counters (optional utility).
    /// </summary>
    public void ResetTime()
    {
        TickCount = 0;
        DayCount = 0;
        WeekCount = 0;
    }

    /// <summary>
    /// Advance multiple ticks at once (optional utility).
    /// </summary>
    public void AdvanceTicks(int count)
    {
        for (int i = 0; i < count; i++)
        {
            AdvanceTick();
        }
    }
}
