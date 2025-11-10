using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private MainMenuController mainMenuController;
    [SerializeField] private WaitingMenuController waitingMenuController;
    [SerializeField] private InteractionMenuUI interactionMenuUI;
    [SerializeField] private UpgradeUIController upgradeUIController;


    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple UIManagers in scene!");
            return;
        }
        Instance = this;
    }

    public void OpenCombatMenu(CombatMenuContext ctx)
    {
        InputGate.OnMenuOpened?.Invoke();
        TimeGate.PauseTime();
        mainMenuController.gameObject.SetActive(true);
        mainMenuController.OpenSubMenu("combat", ctx);

    }

    public void OpenSubMenu(string id, object context = null)
    {
        mainMenuController.gameObject.SetActive(true);
        mainMenuController.OpenSubMenu(id, context);
        InputGate.OnMenuOpened?.Invoke();
        TimeGate.PauseTime();
    }

    public void OpenWaitingMenu(string key)
    {
        waitingMenuController.gameObject.SetActive(true);
        waitingMenuController.OpenMenu(key);
        InputGate.OnMenuOpened?.Invoke();
        TimeGate.ResumeTime(); // the interaction menu pauses time so we must resume it
    }

    public void OpenInteractionMenu(List<InteractionOption> options, IInteractable interactable)
    {
        interactionMenuUI.ShowOptions(options, interactable);
        interactionMenuUI.gameObject.SetActive(true);
        InputGate.OnMenuOpened?.Invoke();
        TimeGate.PauseTime();
    }
    public void OpenUpgradeUnitMenu(UpgradeObject upgradeObject)
    {
        upgradeUIController.gameObject.SetActive(true);
        upgradeUIController.OpenMenu(upgradeObject);
        InputGate.OnMenuOpened?.Invoke();
        TimeGate.PauseTime();
    }
    public void CloseUpgradeUnitMenu()
    {
        upgradeUIController.gameObject.SetActive(false);
        // update partyui??

        // partyui should still be open, so no need to close inputgate or time
    }

    public void CloseAllMenus()
    {
        mainMenuController.CloseAllMenus();
        waitingMenuController.gameObject.SetActive(false);
        InputGate.OnMenuClosed?.Invoke();
        TimeGate.ResumeTime();
    }
}


public static class TimeGate
{
    public static bool IsTimePaused { get; private set; }

    public static void PauseTime()
    {
        if (IsTimePaused) return;
        IsTimePaused = true;
        TickManager.Instance.PauseTicks();
    }

    public static void ResumeTime()
    {
        if (!IsTimePaused) return;
        IsTimePaused = false;
        TickManager.Instance.ResumeTicks();
    }
}