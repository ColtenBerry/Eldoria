using System;
using UnityEditor.VersionControl;
using UnityEngine;

public class InteractionOption
{
    public string label; // "Attack", "Enter Town", "Recruit"
    public Action callback; // What happens when selected (not opening submenus)

    public InteractionOption(string label, Action callback)
    {
        this.label = label;
        this.callback = callback;
    }
}
