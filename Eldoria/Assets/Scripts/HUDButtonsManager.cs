using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDButtonsManager : MonoBehaviour
{

    // Exists to combine buttons and their menu panel
    [System.Serializable]
    public class MenuBUttonBinding
    {
        public Button button;
        public GameObject menuPanel;
    }

    [SerializeField]
    private GameObject mainPanel;
    [SerializeField]
    private List<MenuBUttonBinding> bindings = new();
    private GameObject activePanel = null;

    void Awake()
    {
        foreach (var binding in bindings)
        {
            binding.button.onClick.AddListener(() => TryOpenPanel(binding.menuPanel));
        }
    }
    private void TryOpenPanel(GameObject targetPanel)
    {
        if (activePanel != null) return; // ignore clicks while menu is open

        OpenPanel(targetPanel);
    }

    private void OpenPanel(GameObject panel)
    {

        Debug.Log("attempting to open a panel: " + panel.name);
        mainPanel.SetActive(true);
        panel.SetActive(true);
        activePanel = panel;
        InputGate.OnMenuOpened?.Invoke();
        SetButtonsInteractable(false);
    }
    private void CloseActivePanel()
    {
        if (activePanel != null)
        {
            activePanel.SetActive(false);
            InputGate.OnMenuClosed?.Invoke();
            activePanel = null;
            SetButtonsInteractable(true);
        }
    }
    private void SetButtonsInteractable(bool state)
    {
        foreach (var binding in bindings)
        {
            binding.button.interactable = state;
        }
    }
}
