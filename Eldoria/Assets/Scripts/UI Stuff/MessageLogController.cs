using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MessageLog : MonoBehaviour
{
    [SerializeField] private Transform messageContainer;
    [SerializeField] private GameObject messagePrefab;
    [SerializeField] private int maxMessages = 50;
    [SerializeField] private int messageLifetimeTicks = 10; // expire after X ticks

    private readonly Queue<(WorldMessage msg, GameObject ui)> activeMessages = new();

    private void OnEnable()
    {
        if (TickManager.Instance != null)
        {
            TickManager.Instance.OnTick += HandleTick;
        }
    }
    private void OnDisable()
    {
        if (TickManager.Instance != null)
        {
            TickManager.Instance.OnTick -= HandleTick;
        }
    }
    bool subscribed = false;
    private void Update()
    {
        if (TickManager.Instance != null && !subscribed)
        {
            TickManager.Instance.OnTick += HandleTick;
            subscribed = true;
        }
    }


    public void LogMessage(WorldMessage msg)
    {
        var go = Instantiate(messagePrefab, messageContainer);
        var text = go.GetComponent<TextMeshProUGUI>();
        text.text = msg.Text;

        activeMessages.Enqueue((msg, go));

        if (activeMessages.Count > maxMessages)
        {
            var oldest = activeMessages.Dequeue();
            Debug.Log("destorying message: " + oldest.msg.Text);
            Destroy(oldest.ui);
        }
    }

    private void HandleTick(int tickCount)
    {
        // Expire messages after lifetime
        if (activeMessages.Count == 0) return;

        var oldest = activeMessages.Peek();
        if (tickCount - oldest.msg.Tick >= messageLifetimeTicks)
        {
            var expired = activeMessages.Dequeue();
            Destroy(expired.ui);
        }
    }
}


public class WorldMessage
{
    public string Text { get; }
    public Sprite Icon { get; }
    public int Tick { get; }
    public int Day { get; }
    public int Week { get; }

    public WorldMessage(string text, Sprite icon = null)
    {
        Text = text;
        Icon = icon;

        // Pull standardized time from TickManager
        Tick = TickManager.Instance.TickCount;
        Day = TickManager.Instance.DayCount;
        Week = TickManager.Instance.WeekCount;
    }
}
