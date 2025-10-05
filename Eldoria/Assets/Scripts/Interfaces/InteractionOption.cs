using System;
using UnityEditor.VersionControl;
using UnityEngine;

public class InteractionOption
{
    public string label; // "Attack", "Enter Town", "Recruit"
    public Action callback; // What happens when selected (not opening submenus)
    public string subMenuName; // name of the submenu to be opened

    public InteractionOption(string label, Action callback, string subMenuName = null)
    {
        this.label = label;
        this.callback = callback;
        this.subMenuName = subMenuName;
    }
}
