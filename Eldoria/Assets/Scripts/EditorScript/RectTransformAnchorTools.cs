using UnityEngine;
using UnityEditor;

public class RectTransformAnchorTools
{
    [MenuItem("Tools/UI/Set Anchors To Rect %#a")] // Ctrl+Shift+A shortcut
    private static void SetAnchorsToRect()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            RectTransform rt = obj.GetComponent<RectTransform>();
            if (rt == null || rt.parent == null) continue;

            RectTransform parent = rt.parent as RectTransform;

            Vector2 newMin = new Vector2(
                rt.anchorMin.x + rt.offsetMin.x / parent.rect.width,
                rt.anchorMin.y + rt.offsetMin.y / parent.rect.height
            );

            Vector2 newMax = new Vector2(
                rt.anchorMax.x + rt.offsetMax.x / parent.rect.width,
                rt.anchorMax.y + rt.offsetMax.y / parent.rect.height
            );

            Undo.RecordObject(rt, "Set Anchors To Rect");
            rt.anchorMin = newMin;
            rt.anchorMax = newMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
