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
            if (ctx.isSieging)
            {
                ctx.Fief.OnSiegeEnd();
            }

            else
            {
                if (ctx.Fief.SiegeController.IsUnderSiege)
                {
                    Debug.Log("Fief is under siege, cannot leave!");
                    return;

                }
                GameManager.Instance.player.GetComponent<PartyPresence>().LeaveFief();
            }
            ctx.Fief.RemoveParty(GameManager.Instance.player.GetComponent<PartyPresence>());
            UIManager.Instance.CloseAllMenus();
        });
    }

    WaitingMenuContext ctx;

    public void OpenMenu(WaitingMenuContext source)
    {
        if (source is not WaitingMenuContext)
        {
            Debug.LogWarning("PrisonerAndLootMenuContext expected as source");
            return;
        }
        ctx = source as WaitingMenuContext;

        if (ctx.isSieging)
        {
            keyText.text = "Sieging: ";
        }
        else
        {
            keyText.text = "Waiting: ";
        }
    }
    private void IncrementTick(int i)
    {
        ticksPassed += 1;
        ticksText.text = ticksPassed.ToString() + "ticks";

    }

}

public class WaitingMenuContext
{
    public bool isSieging;
    public SiegableSettlement Fief;

    public WaitingMenuContext(bool isSieging, SiegableSettlement Fief)
    {
        this.isSieging = isSieging;
        this.Fief = Fief;
    }
}
