using UnityEngine;

public class GridCell : MonoBehaviour
{
    private int x;
    private int y;
    private LevelBuilder levelBuilder;

    public void Initialize(int x, int y, LevelBuilder builder)
    {
        this.x = x;
        this.y = y;
        this.levelBuilder = builder;
    }

    private void OnMouseDown()
    {
        if (levelBuilder != null)
        {
            levelBuilder.OnCellClicked(x, y);
        }
    }
} 