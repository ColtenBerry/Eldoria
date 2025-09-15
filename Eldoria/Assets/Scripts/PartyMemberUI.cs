using UnityEngine;
using TMPro;

public class PartyMemberUI : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text powerText;

    public void Setup(PartyMember member)
    {
        nameText.text = member.unitData.unitName;
        powerText.text = $"Power: {member.currentPower}";
    }
}
