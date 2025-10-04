using UnityEngine;

public class InteractionOption
{
    public string label; // "Attack", "Enter Town", "Recruit"
    public Action callback; // What happens when selected
    
    public InteractionOption(string label, Action callback) {
        label = label;
        callback = callback;
    }
}
