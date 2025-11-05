using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class StatsPanelUIController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI playerNameText;
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] TextMeshProUGUI goldText;
    private LordProfile lordProfile;

    public static event Action OnPlayerProfileUpdated;

    private void OnEnable()
    {
        OnPlayerProfileUpdated += RefreshUI;
    }

    private void OnDisable()
    {
        OnPlayerProfileUpdated -= RefreshUI;
    }

    public static void NotifyProfileUpdated()
    {
        OnPlayerProfileUpdated?.Invoke();
    }


    private void Awake()
    {
        StartCoroutine(WaitForProfile());
    }

    private IEnumerator WaitForProfile()
    {
        // Debug.Log("📌 Starting WaitForProfile coroutine...");

        // Wait until GameManager.Instance is available
        while (GameManager.Instance == null)
        {
            // Debug.Log("⏳ Waiting for GameManager.Instance...");
            yield return null;
        }

        // Wait until PlayerProfile is available
        while (GameManager.Instance.PlayerProfile == null)
        {
            // Debug.Log("⏳ Waiting for PlayerProfile...");
            yield return null;
        }

        // Debug.Log("✅ PlayerProfile found. Initializing UI.");
        lordProfile = GameManager.Instance.PlayerProfile;
        RefreshUI();
    }

    private void RefreshUI()
    {
        Debug.Log("⏳ Waiting for PlayerProfile to be assigned...");
        if (lordProfile == null || lordProfile.Lord == null)
        {
            Debug.LogWarning("LordProfile or Lord is null — cannot refresh UI.");
            return;
        }
        playerNameText.text = "Name: " + lordProfile.Lord.UnitName;
        healthText.text = "Health: " + ((lordProfile.Lord.Health / lordProfile.Lord.MaxHealth) * 100f) + "%";
        goldText.text = "Gold: " + lordProfile.GoldAmount.ToString();
    }
}
