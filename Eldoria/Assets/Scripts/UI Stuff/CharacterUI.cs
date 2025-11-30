using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using Microsoft.Unity.VisualStudio.Editor;

public class CharacterUI : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text nameText;
    [SerializeField] private UnityEngine.UI.Image healthBar;
    [SerializeField] private UnityEngine.UI.Image experienceBar;
    public TMP_Text healthText;
    public TMP_Text experienceText;
    private ICardHandler<CharacterInstance> handler;
    private CharacterInstance character;

    [SerializeField] private UnityEngine.UI.Image sprite;

    public void Setup(CharacterInstance character, ICardHandler<CharacterInstance> handler)
    {
        this.handler = handler;
        nameText.text = character.UnitName;
        this.character = character;
        healthText.text = $"Health: {character.Health} / {character.MaxHealth}";
        healthBar.fillAmount = (float)character.Health / character.MaxHealth;
        sprite.sprite = character.characterData.sprite;

        if (this.character.Health == 0)
        {
            gameObject.GetComponent<UnityEngine.UI.Image>().color = Color.red;
        }
        experienceBar.fillAmount = (float)character.CurrentExperience / character.ExperienceToNextLevel;
        experienceText.text = $"{character.CurrentExperience} / {character.ExperienceToNextLevel}";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Clicked");
        handler.OnCardClicked(character);
    }



}
