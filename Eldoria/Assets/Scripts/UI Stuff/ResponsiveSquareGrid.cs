using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(GridLayoutGroup))]
public class ResponsiveSquareGrid : MonoBehaviour
{
    private GridLayoutGroup grid;
    private RectTransform rectTransform;

    [Header("Spacing & Padding")]
    public int minCellSize = 80;
    public int maxCellSize = 200;

    void Awake()
    {
        grid = GetComponent<GridLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();
        UpdateGrid();
    }

    void OnRectTransformDimensionsChange()
    {
        UpdateGrid();
    }

    private void UpdateGrid()
    {
        if (grid == null || rectTransform == null)
            return;

        float containerWidth = rectTransform.rect.width;
        float totalPadding = grid.padding.left + grid.padding.right;
        float availableWidth = containerWidth - totalPadding;

        float spacing = grid.spacing.x;
        int maxColumns = Mathf.FloorToInt((availableWidth + spacing) / (minCellSize + spacing));
        maxColumns = Mathf.Max(1, maxColumns);

        float cellSize = Mathf.Floor((availableWidth - spacing * (maxColumns - 1)) / maxColumns);
        cellSize = Mathf.Clamp(cellSize, minCellSize, maxCellSize);

        grid.cellSize = new Vector2(cellSize, cellSize);
    }
}
