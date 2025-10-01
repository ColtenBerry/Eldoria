using UnityEngine;
using TMPro;

public class PartyMemberUI : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text powerText;

    public void Setup(UnitInstance member)
    {
        nameText.text = member.UnitName;
        powerText.text = $"Power: {member.PowerStat}";
    }
}
