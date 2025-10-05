using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDButtonsManager : MonoBehaviour
{

    // Exists to combine buttons and their menu panel
    [System.Serializable]
    public class MenuButtonBinding
    {
        public Button button;
        public GameObject menuPanel;
    }

    [SerializeField]
    private GameObject mainPanel;
    [SerializeField]
    private List<MenuButtonBinding> bindings = new();
    private GameObject activePanel = null;

    void OnEnable()
    {
        InputGate.OnMenuClosed += CloseActivePanel;
    }
    void OnDisable()
    {
        // InputGate.OnMenuClosed -= CloseActivePanel;
    }

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
        Debug.Log("closing panel");
        if (activePanel != null)
        {
            activePanel.SetActive(false);
            activePanel = null;
            SetButtonsInteractable(true);
        }
    }

    private void SetButtonsInteractable(bool state)
    {
        foreach (var binding in bindings)
        {
            //binding.button.interactable = state;
            gameObject.SetActive(state);
        }
    }
}
