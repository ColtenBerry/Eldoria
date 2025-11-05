using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform buttonContainer;

    private void OnEnable()
    {
        InputGate.OnMenuOpened?.Invoke();
        StatsPanelUIController.NotifyProfileUpdated();
        // Optionally disable HUD buttons here if needed
    }

    private void OnDisable()
    {
        // Optionally re-enable HUD buttons here if needed
    }

    public void ShowOptions(List<InteractionOption> options, IInteractable source)
    {
        gameObject.SetActive(true);

        // Clear previous buttons
        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        // Create new buttons
        foreach (InteractionOption option in options)
        {
            var buttonGO = Instantiate(buttonPrefab, buttonContainer);
            var button = buttonGO.GetComponent<Button>();
            var label = buttonGO.GetComponentInChildren<TMP_Text>();
            label.text = option.label;

            button.onClick.AddListener(() =>
            {
                option.callback?.Invoke();

                if (!string.IsNullOrEmpty(option.subMenuName))
                {
                    UIManager.Instance.OpenSubMenu(option.subMenuName, source);
                }

                CloseMenu();
            });
        }
    }

    private void CloseMenu()
    {
        gameObject.SetActive(false);
    }
}
