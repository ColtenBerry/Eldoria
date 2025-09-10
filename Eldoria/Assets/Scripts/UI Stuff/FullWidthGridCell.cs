using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(GridLayoutGroup))]
public class FullWidthGridCell : MonoBehaviour
{
    private GridLayoutGroup grid;
    private RectTransform rectTransform;

    void Awake()
    {
        grid = GetComponent<GridLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();
        UpdateCellSize();
    }

    void OnRectTransformDimensionsChange()
    {
        UpdateCellSize();
    }

    private void UpdateCellSize()
    {
        if (grid == null || rectTransform == null)
            return;

        float parentWidth = rectTransform.rect.width;
        float totalPadding = grid.padding.left + grid.padding.right;
        float cellWidth = parentWidth - totalPadding;

        grid.cellSize = new Vector2(cellWidth, grid.cellSize.y);
    }
}
