using System;
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
    private void Awake()
    {

        closeButton.onClick.AddListener(() =>
        {
            CloseAllMenus();
        });
    }

    private void CloseAllMenus()
    {
        InputGate.OnMenuClosed?.Invoke();
    }

}
