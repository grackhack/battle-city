using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class LevelManager : MonoBehaviour
{
    [Header("Настройки уровня")]
    [SerializeField] private int levelWidth = 11;
    [SerializeField] private int levelHeight = 11;
    [SerializeField] private float cellSize = 16f; // Теперь это размер в пикселях
    
    [Header("Префабы")]
    [SerializeField] private GameObject brickWallPrefab;
    [SerializeField] private GameObject metalWallPrefab;
    [SerializeField] private GameObject playerTankPrefab;
    
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI levelInfoText;
    
    private GameObject playerTank;
    private int currentLevel;
    private int[,] levelData;

    private const int EMPTY = 0;        // Пустое пространство
    private const int BRICK_WALL = 1;   // Кирпичная стена
    private const int METAL_WALL = 2;   // Металлическая стена
    private const int PLAYER_SPAWN = 3; // Точка спавна игрока

    private void Start()
    {
        // Устанавливаем ориентацию экрана
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        
        // Инициализируем данные уровня
        levelData = new int[levelWidth, levelHeight];
        
        // Пытаемся загрузить уровень
        bool levelLoaded = false;
        try
        {
            LoadSelectedLevel();
            levelLoaded = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка при загрузке уровня: {e.Message}");
            levelLoaded = false;
        }
        
        // Если уровень не загружен, создаем уровень по умолчанию
        if (!levelLoaded || levelData == null)
        {
            Debug.Log("Создаем уровень по умолчанию...");
            CreateDefaultLevel();
        }
        
        // Создаем уровень и спавним игрока
        CreateLevelFromData();
        SpawnPlayer();
    }

    private void LoadSelectedLevel()
    {
        string selectedLevel = PlayerPrefs.GetString("SelectedLevel", "1");
        currentLevel = int.Parse(selectedLevel);
        string path = Path.Combine(Application.streamingAssetsPath, "level" + selectedLevel + ".txt");
        
        Debug.Log($"Загрузка уровня из файла: {path}");
        
        if (!File.Exists(path))
        {
            Debug.LogWarning($"Файл уровня не найден: {path}. Создаем уровень по умолчанию.");
            CreateDefaultLevel();
            return;
        }

        string[] lines = File.ReadAllLines(path);
        if (lines.Length == 0)
        {
            Debug.LogError("Файл уровня пуст! Создаем уровень по умолчанию.");
            CreateDefaultLevel();
            return;
        }

        // Первая строка содержит размеры уровня
        string[] dimensions = lines[0].Split(',');
        if (dimensions.Length != 2)
        {
            Debug.LogError("Неверный формат размеров уровня! Создаем уровень по умолчанию.");
            CreateDefaultLevel();
            return;
        }

        levelWidth = int.Parse(dimensions[0]);
        levelHeight = int.Parse(dimensions[1]);
        levelData = new int[levelWidth, levelHeight];

        Debug.Log($"Загрузка уровня {selectedLevel}. Размеры: {levelWidth}x{levelHeight}");
        ShowLevelInfo($"Уровень {selectedLevel}");

        // Заполняем массив данными из файла
        for (int y = 0; y < levelHeight; y++)
        {
            if (y + 1 >= lines.Length)
            {
                Debug.LogError($"Недостаточно строк в файле уровня! Создаем уровень по умолчанию.");
                CreateDefaultLevel();
                return;
            }
            
            string[] row = lines[y + 1].Split(',');
            for (int x = 0; x < levelWidth; x++)
            {
                if (x < row.Length)
                {
                    if (int.TryParse(row[x], out int value))
                    {
                        levelData[x, y] = value;
                        if (value == PLAYER_SPAWN)
                        {
                            Debug.Log($"Найдена точка спавна игрока на позиции [{x},{y}]");
                        }
                    }
                    else
                    {
                        Debug.LogError($"Неверное значение в позиции [{x},{y}]. Создаем уровень по умолчанию.");
                        CreateDefaultLevel();
                        return;
                    }
                }
                else
                {
                    Debug.LogError($"Недостаточно значений в строке {y + 1}. Создаем уровень по умолчанию.");
                    CreateDefaultLevel();
                    return;
                }
            }
        }
    }

    private void CreateDefaultLevel()
    {
        Debug.Log("Создание уровня по умолчанию");
        
        // Устанавливаем размеры уровня по умолчанию
        levelWidth = 13;
        levelHeight = 13;
        levelData = new int[levelWidth, levelHeight];

        // Создаем границы уровня
        for (int x = 0; x < levelWidth; x++)
        {
            for (int y = 0; y < levelHeight; y++)
            {
                // Границы уровня - металлические стены
                if (x == 0 || x == levelWidth - 1 || y == 0 || y == levelHeight - 1)
                {
                    levelData[x, y] = METAL_WALL;
                }
                // Внутренние стены - кирпичные
                else if ((x == 3 || x == 9) && (y >= 3 && y <= 9))
                {
                    levelData[x, y] = BRICK_WALL;
                }
                else if ((y == 3 || y == 9) && (x >= 3 && x <= 9))
                {
                    levelData[x, y] = BRICK_WALL;
                }
                // Точка спавна игрока в центре
                else if (x == 6 && y == 6)
                {
                    levelData[x, y] = PLAYER_SPAWN;
                }
                // Остальное пространство пустое
                else
                {
                    levelData[x, y] = EMPTY;
                }
            }
        }

        Debug.Log("Уровень по умолчанию создан");
        ShowLevelInfo("Уровень по умолчанию");
    }

    private Vector3 GetWorldPosition(int x, int y)
    {
        // Вычисляем размеры игрового поля в пикселях
        float fieldWidth = levelWidth * cellSize;
        float fieldHeight = levelHeight * cellSize;
        
        // Вычисляем смещение для центрирования
        float offsetX = -fieldWidth / 2f;
        float offsetY = -fieldHeight / 2f;
        
        // Вычисляем позицию в мире (в пикселях) с учетом центрирования
        float posX = offsetX + (x * cellSize);
        float posY = offsetY + (y * cellSize);
        
        return new Vector3(posX, posY, 0);
    }

    private void SpawnPlayer()
    {
        if (playerTankPrefab == null)
        {
            Debug.LogError("Префаб танка игрока не назначен!");
            return;
        }

        // Находим позицию спавна игрока
        Vector2Int spawnPos = FindPlayerSpawnPosition();
        Vector3 worldPos = GetWorldPosition(spawnPos.x, spawnPos.y);
        
        Debug.Log($"Создание танка на позиции [{spawnPos.x},{spawnPos.y}] -> {worldPos}");
        
        // Создаем танк игрока
        playerTank = Instantiate(playerTankPrefab, worldPos, Quaternion.identity);
        playerTank.name = "PlayerTank";
        
        // Проверяем компоненты танка
        if (playerTank.GetComponent<PlayerTank>() == null)
        {
            Debug.LogError("На префабе танка отсутствует компонент PlayerTank!");
        }
        if (playerTank.GetComponent<SpriteRenderer>() == null)
        {
            Debug.LogError("На префабе танка отсутствует компонент SpriteRenderer!");
        }
        else
        {
            Debug.Log($"SpriteRenderer найден, спрайт: {playerTank.GetComponent<SpriteRenderer>().sprite != null}");
        }

        // Отключаем Rigidbody2D на время инициализации
        Rigidbody2D rb = playerTank.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = false;
        }
        
        // Устанавливаем позицию после создания всех компонентов
        playerTank.transform.position = worldPos;
        
        // Включаем Rigidbody2D обратно
        if (rb != null)
        {
            rb.simulated = true;
        }
        
        Debug.Log($"Танк игрока создан на позиции: {playerTank.transform.position}");
    }

    private Vector2Int FindPlayerSpawnPosition()
    {
        // Ищем точку спавна игрока
        for (int y = 0; y < levelHeight; y++)
        {
            for (int x = 0; x < levelWidth; x++)
            {
                if (levelData[x, y] == PLAYER_SPAWN)
                {
                    Debug.Log($"Найдена точка спавна игрока: [{x},{y}]");
                    return new Vector2Int(x, y);
                }
            }
        }
        
        Debug.LogWarning("Точка спавна игрока не найдена! Используем позицию по умолчанию.");
        return new Vector2Int(0, 0);
    }

    private void CreateWall(GameObject wallPrefab, int x, int y)
    {
        if (wallPrefab == null)
        {
            Debug.LogError($"Префаб стены не назначен! Позиция [{x},{y}]");
            return;
        }

        Vector3 position = GetWorldPosition(x, y);
        GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity, transform);
        wall.tag = "Wall";
        
        // Устанавливаем размер коллайдера равным размеру спрайта
        BoxCollider2D collider = wall.GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.size = new Vector2(cellSize, cellSize);
            collider.offset = Vector2.zero;
        }

        // Устанавливаем размер спрайта
        SpriteRenderer spriteRenderer = wall.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.size = new Vector2(cellSize, cellSize);
        }
    }

    private void CreateLevelFromData()
    {
        for (int x = 0; x < levelWidth; x++)
        {
            for (int y = 0; y < levelHeight; y++)
            {
                switch (levelData[x, y])
                {
                    case BRICK_WALL:
                        CreateWall(brickWallPrefab, x, y);
                        break;
                    case METAL_WALL:
                        CreateWall(metalWallPrefab, x, y);
                        break;
                }
            }
        }
    }

    public void LevelCompleted()
    {
        // Проверяем наличие следующего уровня
        string nextLevelPath = Path.Combine(Application.streamingAssetsPath, "level" + (currentLevel + 1) + ".txt");
        
        if (File.Exists(nextLevelPath))
        {
            // Загружаем следующий уровень
            PlayerPrefs.SetString("SelectedLevel", (currentLevel + 1).ToString());
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            Debug.Log("Поздравляем! Вы прошли все уровни!");
            ReturnToMainMenu();
        }
    }

    private void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void ShowLevelInfo(string info)
    {
        if (levelInfoText != null)
        {
            levelInfoText.text = info;
        }
        Debug.Log($"Текущий уровень: {info}");
    }
}