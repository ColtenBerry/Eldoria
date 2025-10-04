using TMPro;
using UnityEngine;

public class CharacterMenuUI : MonoBehaviour
{

    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text experienceText;
    [SerializeField] private TMP_Text statsText;


    [SerializeField] private PartyPresence partyPresence;
    private CharacterInstance character;


    public void Awake()
    {
        // initialize button listeners


        character = partyPresence.Lord;
        RefreshUI();
    }


    private void RefreshUI()
    {
        nameText.text = character.UnitName;
        levelText.text = $"Level: {character.CurrentLevel}";
        experienceText.text = $"XP: {character.CurrentExperience}/{character.ExperienceToNextLevel}";
        statsText.text = $"STR: {character.Strength}\nAGI: {character.Agility}\nINT: {character.Intelligence}\nCHA: {character.Charisma}\nEND: {character.Endurance}";
    }
}
