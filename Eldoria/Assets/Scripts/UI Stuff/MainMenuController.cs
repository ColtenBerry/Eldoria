using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public abstract class MenuController : MonoBehaviour
{
    protected virtual void OnEnable()
    {
        InputGate.OnMenuClosed += HandleClose;
    }
    protected virtual void OnDisable()
    {
        InputGate.OnMenuClosed -= HandleClose;

    }

    protected virtual void HandleClose()
    {
        gameObject.SetActive(false);
    }

}

public class MainMenuController : MenuController
{
    [SerializeField]
    private Button closeButton;

    [SerializeField] private List<SubMenuEntry> subMenuEntries;
    private Dictionary<string, GameObject> subMenus; // dictionary mapping submenu names to approprate menu
    private void Awake()
    {

        closeButton.onClick.AddListener(() =>
        {
            CloseAllMenus();
        });

        subMenus = new Dictionary<string, GameObject>();
        foreach (SubMenuEntry entry in subMenuEntries)
        {
            print("reached");
            if (!subMenus.ContainsKey(entry.id))
                subMenus.Add(entry.id, entry.panel);
        }
        Debug.Log("subscribing to combatsimulator.oncombatmenurequested");
        CombatSimulator.OnCombatMenuRequested += HandleCombatMenu;

    }

    private void Start()
    {
    }

    private void HandleCombatMenu(PartyController enemyParty, bool isPlayerAttacking)
    {
        Debug.Log("attempting to open combat menu");
        gameObject.SetActive(true);
        OpenSubMenu("CombatMenu", new CombatMenuContext(enemyParty, isPlayerAttacking));
    }

    private void CloseAllMenus()
    {
        InputGate.OnMenuClosed?.Invoke();
    }

    public void OpenSubMenu(string id, object source)
    {
        InputGate.OnMenuOpened?.Invoke();
        foreach (GameObject panel in subMenus.Values)
        {
            panel.SetActive(false);
        }

        if (subMenus.TryGetValue(id, out var panel2))
        {
            if (panel2.TryGetComponent<IMenuWithSource>(out var menuWithSource))
            {
                panel2.SetActive(true);
                menuWithSource.OpenMenu(source);
            }
        }
        else Debug.LogWarning($"Submenu '{id}' not found");
    }

}


[System.Serializable]
public class SubMenuEntry
{
    public string id;
    public GameObject panel;
}
