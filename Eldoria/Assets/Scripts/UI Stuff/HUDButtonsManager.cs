using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDButtonsManager : MonoBehaviour
{
    [System.Serializable]
    public class MenuButtonBinding
    {
        public Button button;
        public string subMenuId; // ID used by UIManager to open the submenu
    }

    [SerializeField] private GameObject mainPanel;
    [SerializeField] private List<MenuButtonBinding> bindings = new();

    private bool menuOpen = false;

    private void Awake()
    {
        foreach (var binding in bindings)
        {
            var id = binding.subMenuId;
            binding.button.onClick.AddListener(() => TryOpenSubMenu(id));
        }
    }

    private void OnEnable()
    {
        InputGate.OnMenuClosed += HandleMenuClosed;
    }

    private void OnDisable()
    {
        InputGate.OnMenuClosed -= HandleMenuClosed;
    }

    private void TryOpenSubMenu(string subMenuId)
    {
        if (menuOpen) return;

        OpenSubMenu(subMenuId);
    }

    private void OpenSubMenu(string subMenuId)
    {
        Debug.Log($"HUDButtonsManager: Opening submenu '{subMenuId}'");
        mainPanel.SetActive(true);
        UIManager.Instance.OpenSubMenu(subMenuId);
        menuOpen = true;
        SetButtonsInteractable(false);
    }

    private void HandleMenuClosed()
    {
        Debug.Log("HUDButtonsManager: Menu closed");
        menuOpen = false;
        SetButtonsInteractable(true);
    }

    private void SetButtonsInteractable(bool state)
    {
        foreach (var binding in bindings)
        {
            binding.button.interactable = state;
        }
    }
}
