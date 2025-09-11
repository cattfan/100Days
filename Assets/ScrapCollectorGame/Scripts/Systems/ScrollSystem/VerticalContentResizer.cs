using UnityEngine;
using UnityEngine.UI;

public class VerticalContentResizer : MonoBehaviour
{
    public GridLayoutGroup grid;

    void LateUpdate()
    {
        int childCount = transform.childCount;

        // Chiều cao tính theo số item, spacing, padding
        float height = childCount * (grid.cellSize.y + grid.spacing.y)
                       + grid.padding.top + grid.padding.bottom;

        // Cộng thêm 100px cho top và 100px cho bottom
        height += 200f;

        RectTransform rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);
    }
}
