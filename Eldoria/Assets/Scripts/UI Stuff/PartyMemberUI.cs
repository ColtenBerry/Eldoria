using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using Microsoft.Unity.VisualStudio.Editor;

public class PartyMemberUI : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text nameText;
    public TMP_Text healthText;
    public TMP_Text experienceText;
    public GameObject upgradeIndicator;
    private ICardHandler<UnitInstance> handler;
    private UnitInstance unit;

    public void Setup(UnitInstance member, ICardHandler<UnitInstance> handler)
    {
        this.handler = handler;
        nameText.text = member.UnitName;
        unit = member;
        healthText.text = $"Health: {member.Health} / {member.MaxHealth}";

        if (unit.Health == 0)
        {
            gameObject.GetComponent<UnityEngine.UI.Image>().color = Color.red;
        }

        experienceText.text = member.CurrentExperience.ToString();
        upgradeIndicator.SetActive(member.CanUpgrade);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Clicked");
        handler.OnCardClicked(unit);
    }



}
