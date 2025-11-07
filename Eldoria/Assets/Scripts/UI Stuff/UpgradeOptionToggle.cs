using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeOptionToggle : MonoBehaviour
{
    [SerializeField] private Text label;
    private Toggle toggle;
    private UnitData data;


    void Awake()
    {
        toggle = GetComponent<Toggle>();
    }
    public void Initialize(UnitData data, ToggleGroup group)
    {
        this.data = data;
        label.text = data.unitName;
        toggle.group = group;
    }
    public UnitData GetUnitData()
    {
        return data;
    }
    public bool IsSelected => toggle.isOn;
}
