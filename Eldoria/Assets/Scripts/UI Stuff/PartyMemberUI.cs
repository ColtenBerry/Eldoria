using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using Microsoft.Unity.VisualStudio.Editor;

public class PartyMemberUI : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text nameText;
    [SerializeField] private UnityEngine.UI.Image healthBar;
    [SerializeField] private UnityEngine.UI.Image experienceBar;
    public TMP_Text healthText;
    public TMP_Text experienceText;
    public GameObject upgradeIndicator;
    private ICardHandler<SoldierInstance> handler;
    private SoldierInstance unit;

    [SerializeField] private UnityEngine.UI.Image sprite;

    public void Setup(SoldierInstance member, ICardHandler<SoldierInstance> handler)
    {
        this.handler = handler;
        nameText.text = member.UnitName;
        unit = member;
        healthText.text = $"Health: {member.Health} / {member.MaxHealth}";
        healthBar.fillAmount = (float)member.Health / member.MaxHealth;
        sprite.sprite = member.soldierData.sprite;

        if (unit.Health == 0)
        {
            gameObject.GetComponent<UnityEngine.UI.Image>().color = Color.red;
        }
        experienceBar.fillAmount = (float)member.CurrentExperience / member.ExperienceToNextLevel;
        experienceText.text = $"{member.CurrentExperience} / {member.ExperienceToNextLevel}";
        if (member.CanUpgrade)
        {
            Debug.Log("party member can upgrade");
            upgradeIndicator.SetActive(member.CanUpgrade);

        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Clicked");
        handler.OnCardClicked(unit);
    }



}
