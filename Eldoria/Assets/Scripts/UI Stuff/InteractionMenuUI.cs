using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class InteractionMenuUI : MonoBehaviour
{
    public MainMenuController mainMenuController;
    public GameObject buttonPrefab;
    public Transform buttonContainer;

    void OnEnable()
    {
        // disable hud buttons?
    }

    void OnDisable()
    {
        // disable hud buttons?
    }

    public void ShowOptions(List<InteractionOption> options)
    {
        InputGate.OnMenuOpened?.Invoke();

        gameObject.SetActive(true);
        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        foreach (InteractionOption option in options)
        {

            var buttonGO = Instantiate(buttonPrefab, buttonContainer);
            var button = buttonGO.GetComponent<Button>();
            var label = buttonGO.GetComponentInChildren<TMP_Text>();
            label.text = option.label;
            button.onClick.AddListener(() =>
            {
                option.callback.Invoke();
                if (option.subMenuName != null)
                {

                    mainMenuController.OpenSubMenu(option.subMenuName);
                    gameObject.SetActive(false); // close this panel
                }

            });
        }
    }
}
