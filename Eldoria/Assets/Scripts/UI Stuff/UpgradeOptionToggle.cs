using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeOptionToggle : MonoBehaviour
{
    [SerializeField] private Text label;
    private Toggle toggle;
    private SoldierData data;


    void Awake()
    {
        toggle = GetComponent<Toggle>();
    }
    public void Initialize(SoldierData data, ToggleGroup group)
    {
        this.data = data;
        label.text = data.unitName;
        toggle.group = group;
    }
    public SoldierData GetUnitData()
    {
        return data;
    }
    public bool IsSelected => toggle.isOn;
}
