using UnityEngine;

public class PanelSwitcher : MonoBehaviour
{
    public GameObject[] panels;

    public void ShowPanel(int index)
    {
        for (int i = 0; i < panels.Length; i++)
            panels[i].SetActive(i == index);
    }
}
