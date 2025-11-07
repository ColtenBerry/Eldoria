using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaitingMenuController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI keyText;
    [SerializeField] private TextMeshProUGUI ticksText;

    [SerializeField] private Button stopButton;


    int ticksPassed = 0;
    void OnEnable()
    {
        ticksPassed = 0;
        TickManager.Instance.OnTick += IncrementTick;
    }

    void OnDisable()
    {
        TickManager.Instance.OnTick -= IncrementTick;
    }

    void Awake()
    {
        Debug.Log($"WaitingMenuController on {gameObject.name}");

        stopButton.onClick.AddListener(() =>
        {
            UIManager.Instance.CloseAllMenus();
            GameManager.Instance.player.GetComponent<PartyPresence>().LeaveFief();
        });
    }

    public void OpenMenu(string key)
    {
        if (key == "wait")
        {
            keyText.text = "Waiting: ";
        }
        else if (key == "siege")
        {
            keyText.text = "Sieging: ";
        }
    }
    private void IncrementTick(int i)
    {
        ticksPassed += 1;
        ticksText.text = ticksPassed.ToString() + "ticks";

    }

}
