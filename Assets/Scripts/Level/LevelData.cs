using UnityEngine;

[System.Serializable]
public class LevelData
{
    public int width;
    public int height;
    public int[,] grid;

    public LevelData(int width, int height)
    {
        this.width = width;
        this.height = height;
        this.grid = new int[width, height];
    }
} 