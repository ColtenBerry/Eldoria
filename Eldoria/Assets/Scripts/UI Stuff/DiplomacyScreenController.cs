using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiplomacyScreenController : MonoBehaviour, ICardHandler<Faction>
{
    [SerializeField] private TextMeshProUGUI castlesText;
    [SerializeField] private TextMeshProUGUI townsText;
    [SerializeField] private TextMeshProUGUI villagesText;
    [SerializeField] private TextMeshProUGUI lordsText;

    [SerializeField] private Button peaceOrWarButton;

    [SerializeField] private Transform factionsTransform;

    private List<Faction> factionsList;

    private void Awake()
    {
        peaceOrWarButton.onClick.AddListener(() =>
        {

        });
    }

    // no source needed

    private void RefreshList()
    {
        // refresh the factionslist
    }


}
