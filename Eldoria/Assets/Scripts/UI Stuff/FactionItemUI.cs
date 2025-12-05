using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FactionItemUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image factionImage;
    [SerializeField] private TextMeshProUGUI factionName;

    private ICardHandler<Faction> handler;
    private Faction faction;
    public Faction Faction => faction;

    public void Setup(Faction faction, ICardHandler<Faction> cardHandler)
    {
        this.faction = faction;
        handler = cardHandler;

        ApplyVisuals();
    }

    public void RefreshVisuals()
    {
        ApplyRelationalVisuals();
    }

    private void ApplyVisuals()
    {
        factionImage.color = faction.factionColor;
        factionName.text = faction.factionName;

        ApplyRelationalVisuals();
    }
    private void ApplyRelationalVisuals()
    {
        factionName.color = FactionsManager.Instance.AreEnemies(GameManager.Instance.PlayerProfile.Faction, faction) ? Color.red : Color.white;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        handler.OnCardClicked(faction);
    }


}
