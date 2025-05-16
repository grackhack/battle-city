using UnityEngine;

public class GridVisualizer : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 13;
    [SerializeField] private int gridHeight = 13;
    [SerializeField] private float cellSize = 0.5f;
    [SerializeField] private Color gridColor = new Color(1f, 1f, 1f, 0.2f);
    [SerializeField] private Color mainLineColor = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private bool showGrid = true;
    [SerializeField] private int sortingOrder = 100; // Высокий порядок отрисовки

    private LineRenderer[] verticalLines;
    private LineRenderer[] horizontalLines;
    private GameObject gridContainer;
    private Camera mainCamera;

    private void Start()
    {
        if (!showGrid) return;
        mainCamera = Camera.main;
        CreateGridLines();
    }

    private void CreateGridLines()
    {
        // Создаем контейнер для линий
        gridContainer = new GameObject("GridLines");
        gridContainer.transform.SetParent(transform);
        gridContainer.transform.localPosition = Vector3.zero;

        // Вычисляем размеры сетки в мировых координатах
        float gridWidthWorld = (gridWidth + 1) * cellSize; // +1 для учета границ
        float gridHeightWorld = (gridHeight + 1) * cellSize; // +1 для учета границ

        // Вычисляем смещение для центрирования сетки
        float offsetX = -gridWidthWorld / 2f;
        float offsetY = -gridHeightWorld / 2f;

        // Создаем вертикальные линии
        verticalLines = new LineRenderer[gridWidth + 2]; // +2 для границ
        for (int x = 0; x <= gridWidth + 1; x++)
        {
            GameObject lineObj = new GameObject($"VerticalLine_{x}");
            lineObj.transform.SetParent(gridContainer.transform);
            
            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startWidth = 0.02f;
            line.endWidth = 0.02f;
            line.sortingOrder = sortingOrder;
            line.positionCount = 2;
            
            float posX = offsetX + x * cellSize;
            line.SetPosition(0, new Vector3(posX, offsetY, 0));
            line.SetPosition(1, new Vector3(posX, offsetY + gridHeightWorld, 0));
            
            line.startColor = line.endColor = x % 2 == 0 ? mainLineColor : gridColor;
            verticalLines[x] = line;
        }

        // Создаем горизонтальные линии
        horizontalLines = new LineRenderer[gridHeight + 2]; // +2 для границ
        for (int y = 0; y <= gridHeight + 1; y++)
        {
            GameObject lineObj = new GameObject($"HorizontalLine_{y}");
            lineObj.transform.SetParent(gridContainer.transform);
            
            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startWidth = 0.02f;
            line.endWidth = 0.02f;
            line.sortingOrder = sortingOrder;
            line.positionCount = 2;
            
            float posY = offsetY + y * cellSize;
            line.SetPosition(0, new Vector3(offsetX, posY, 0));
            line.SetPosition(1, new Vector3(offsetX + gridWidthWorld, posY, 0));
            
            line.startColor = line.endColor = y % 2 == 0 ? mainLineColor : gridColor;
            horizontalLines[y] = line;
        }

        // Создаем точки на пересечениях
        for (int x = 0; x <= gridWidth + 1; x++)
        {
            for (int y = 0; y <= gridHeight + 1; y++)
            {
                GameObject pointObj = new GameObject($"GridPoint_{x}_{y}");
                pointObj.transform.SetParent(gridContainer.transform);
                
                SpriteRenderer pointRenderer = pointObj.AddComponent<SpriteRenderer>();
                pointRenderer.sprite = CreatePointSprite();
                pointRenderer.color = Color.yellow;
                pointRenderer.sortingOrder = sortingOrder;
                
                float posX = offsetX + x * cellSize;
                float posY = offsetY + y * cellSize;
                pointObj.transform.position = new Vector3(posX, posY, 0);
                pointObj.transform.localScale = new Vector3(0.1f, 0.1f, 1f);
            }
        }

        // Устанавливаем позицию контейнера в центр экрана
        gridContainer.transform.position = mainCamera.transform.position + new Vector3(0, 0, 10);
    }

    private Sprite CreatePointSprite()
    {
        // Создаем текстуру для точки
        Texture2D texture = new Texture2D(8, 8);
        Color[] colors = new Color[64];
        for (int i = 0; i < 64; i++)
        {
            colors[i] = Color.white;
        }
        texture.SetPixels(colors);
        texture.Apply();

        // Создаем спрайт из текстуры
        return Sprite.Create(texture, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f));
    }

    private void OnDestroy()
    {
        if (gridContainer != null)
        {
            Destroy(gridContainer);
        }
    }
}