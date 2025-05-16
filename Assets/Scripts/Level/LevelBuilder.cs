using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class LevelBuilder : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private int levelWidth = 13;
    [SerializeField] private int levelHeight = 13;
    [SerializeField] private float cellSize = 0.25f;
    [SerializeField] private string levelName = "level1";

    [Header("Префабы")]
    [SerializeField] private GameObject brickWallPrefab;
    [SerializeField] private GameObject metalWallPrefab;
    [SerializeField] private GameObject playerTankPrefab;

    [Header("UI")]
    [SerializeField] private Button emptyButton;
    [SerializeField] private Button brickButton;
    [SerializeField] private Button metalButton;
    [SerializeField] private Button spawnButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private InputField levelNameInput;
    [SerializeField] private GameObject gridCellPrefab;
    [SerializeField] private Transform gridContainer;

    private int[,] levelData;
    private int selectedTile = 0;
    private GameObject[,] gridCells;
    private GameObject playerTank;

    private const int EMPTY = 0;
    private const int BRICK_WALL = 1;
    private const int METAL_WALL = 2;
    private const int PLAYER_SPAWN = 3;

    private void Start()
    {
        ValidateReferences();
        InitializeLevelData();
        CreateGrid();
        SetupUI();
    }

    private void ValidateReferences()
    {
        if (brickWallPrefab == null) Debug.LogError("Brick Wall Prefab не назначен!");
        if (metalWallPrefab == null) Debug.LogError("Metal Wall Prefab не назначен!");
        if (playerTankPrefab == null) Debug.LogError("Player Tank Prefab не назначен!");
        if (emptyButton == null) Debug.LogError("Empty Button не назначена!");
        if (brickButton == null) Debug.LogError("Brick Button не назначена!");
        if (metalButton == null) Debug.LogError("Metal Button не назначена!");
        if (spawnButton == null) Debug.LogError("Spawn Button не назначена!");
        if (saveButton == null) Debug.LogError("Save Button не назначена!");
        if (levelNameInput == null) Debug.LogError("Level Name Input не назначен!");
        if (gridCellPrefab == null) Debug.LogError("Grid Cell Prefab не назначен!");
        if (gridContainer == null) Debug.LogError("Grid Container не назначен!");
    }

    private void InitializeLevelData()
    {
        levelData = new int[levelWidth, levelHeight];
        gridCells = new GameObject[levelWidth, levelHeight];
    }

    private void CreateGrid()
    {
        if (gridContainer == null || gridCellPrefab == null)
        {
            Debug.LogError("Не удалось создать сетку: отсутствуют необходимые ссылки");
            return;
        }

        // Очищаем существующую сетку
        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }

        // Создаем новую сетку
        for (int y = 0; y < levelHeight; y++)
        {
            for (int x = 0; x < levelWidth; x++)
            {
                GameObject cell = Instantiate(gridCellPrefab, gridContainer);
                cell.transform.localPosition = new Vector3(x * cellSize, y * cellSize, 0);
                
                // Добавляем компонент для обработки кликов
                GridCell gridCell = cell.AddComponent<GridCell>();
                gridCell.Initialize(x, y, this);
                
                gridCells[x, y] = cell;
                UpdateCellVisual(x, y);
            }
        }
    }

    private void SetupUI()
    {
        if (emptyButton != null)
        {
            emptyButton.onClick.AddListener(() => {
                selectedTile = EMPTY;
                Debug.Log("Выбрана пустая ячейка");
            });
        }
        if (brickButton != null)
        {
            brickButton.onClick.AddListener(() => {
                selectedTile = BRICK_WALL;
                Debug.Log("Выбрана кирпичная стена");
            });
        }
        if (metalButton != null)
        {
            metalButton.onClick.AddListener(() => {
                selectedTile = METAL_WALL;
                Debug.Log("Выбрана металлическая стена");
            });
        }
        if (spawnButton != null)
        {
            spawnButton.onClick.AddListener(() => {
                selectedTile = PLAYER_SPAWN;
                Debug.Log("Выбрана точка спавна");
            });
        }
        if (saveButton != null)
        {
            saveButton.onClick.AddListener(() => {
                SaveLevel();
                Debug.Log("Уровень сохранен");
            });
        }

        if (levelNameInput != null)
        {
            levelNameInput.text = levelName;
            levelNameInput.onValueChanged.AddListener((value) => {
                levelName = value;
                Debug.Log("Имя уровня изменено на: " + value);
            });
        }
    }

    public void OnCellClicked(int x, int y)
    {
        // Удаляем существующий танк, если он есть
        if (levelData[x, y] == PLAYER_SPAWN && playerTank != null)
        {
            Destroy(playerTank);
        }

        levelData[x, y] = selectedTile;
        UpdateCellVisual(x, y);

        // Если выбран спавн, создаем танк
        if (selectedTile == PLAYER_SPAWN)
        {
            Vector3 position = new Vector3(x * cellSize, y * cellSize, 0);
            playerTank = Instantiate(playerTankPrefab, position, Quaternion.identity);
        }
    }

    private void UpdateCellVisual(int x, int y)
    {
        if (gridCells == null || x >= levelWidth || y >= levelHeight)
            return;

        GameObject cell = gridCells[x, y];
        if (cell == null)
            return;

        SpriteRenderer renderer = cell.GetComponent<SpriteRenderer>();
        if (renderer == null)
            return;

        switch (levelData[x, y])
        {
            case EMPTY:
                renderer.sprite = null;
                break;
            case BRICK_WALL:
                if (brickWallPrefab != null)
                    renderer.sprite = brickWallPrefab.GetComponent<SpriteRenderer>().sprite;
                break;
            case METAL_WALL:
                if (metalWallPrefab != null)
                    renderer.sprite = metalWallPrefab.GetComponent<SpriteRenderer>().sprite;
                break;
            case PLAYER_SPAWN:
                renderer.sprite = null;
                break;
        }
    }

    private void SaveLevel()
    {
        string directory = Path.Combine(Application.streamingAssetsPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string path = Path.Combine(directory, levelName + ".txt");
        using (StreamWriter writer = new StreamWriter(path))
        {
            // Записываем размеры
            writer.WriteLine($"{levelWidth},{levelHeight}");

            // Записываем данные уровня
            for (int y = 0; y < levelHeight; y++)
            {
                string line = "";
                for (int x = 0; x < levelWidth; x++)
                {
                    line += levelData[x, y];
                    if (x < levelWidth - 1) line += ",";
                }
                writer.WriteLine(line);
            }
        }

        Debug.Log($"Уровень сохранен в {path}");
    }
} 