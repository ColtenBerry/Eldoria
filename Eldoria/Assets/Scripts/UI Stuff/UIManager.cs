using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private MainMenuController mainMenuController;
    [SerializeField] private WaitingMenuController waitingMenuController;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple UIManagers in scene!");
            return;
        }
        Instance = this;
    }

    public void OpenCombatMenu(PartyController enemyParty, bool isPlayerAttacking)
    {
        mainMenuController.gameObject.SetActive(true);
        mainMenuController.OpenSubMenu("CombatMenu", new CombatMenuContext(enemyParty, isPlayerAttacking));
    }

    public void OpenSubMenu(string id, object context = null)
    {
        mainMenuController.gameObject.SetActive(true);
        mainMenuController.OpenSubMenu(id, context);
    }

    public void OpenWaitingMenu(string key)
    {
        waitingMenuController.gameObject.SetActive(true);
        waitingMenuController.OpenMenu(key);
    }

    public void CloseWaitingMenu()
    {
        waitingMenuController.gameObject.SetActive(false);
    }

    public void CloseAllMenus()
    {
        mainMenuController.CloseAllMenus();
    }
}
