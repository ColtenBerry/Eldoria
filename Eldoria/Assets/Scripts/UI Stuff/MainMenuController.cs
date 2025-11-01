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
    [SerializeField] private Button closeButton;
    [SerializeField] private List<SubMenuEntry> subMenuEntries;

    private Dictionary<string, GameObject> subMenus;

    private void Awake()
    {
        closeButton.onClick.AddListener(() => InputGate.OnMenuClosed?.Invoke());

        subMenus = new();
        foreach (var entry in subMenuEntries)
        {
            if (!subMenus.ContainsKey(entry.id))
                subMenus.Add(entry.id, entry.panel);
        }
    }

    public void OpenSubMenu(string id, object context = null)
    {
        InputGate.OnMenuOpened?.Invoke();

        foreach (var panel in subMenus.Values)
            panel.SetActive(false);

        if (subMenus.TryGetValue(id, out var panelToOpen))
        {
            panelToOpen.SetActive(true);

            if (panelToOpen.TryGetComponent<IMenuWithSource>(out var menuWithSource))
            {
                menuWithSource.OpenMenu(context);
            }
        }
        else
        {
            Debug.LogWarning($"MainMenuController: Submenu '{id}' not found.");
        }
    }

    public void CloseAllMenus()
    {
        foreach (var panel in subMenus.Values)
            panel.SetActive(false);
        InputGate.OnMenuClosed?.Invoke();
        gameObject.SetActive(false);
    }
}


[System.Serializable]
public class SubMenuEntry
{
    public string id;
    public GameObject panel;
}
