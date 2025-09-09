using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public abstract class MenuController : MonoBehaviour
{
    protected virtual void OnEnable()
    {
        MainMenuController.OnCloseSubPanels += HandleClose;
    }
    protected virtual void OnDisable()
    {
        MainMenuController.OnCloseSubPanels -= HandleClose;
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
    public static event Action OnCloseSubPanels;
    private void Awake()
    {

        closeButton.onClick.AddListener(() =>
        {
            HandleClose();
            OnCloseSubPanels.Invoke();
        });
    }

}
