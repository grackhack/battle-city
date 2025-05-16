#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class LevelEditor : EditorWindow
{
    private int[,] levelData;
    private int levelWidth = 13;
    private int levelHeight = 13;
    private string levelName = "level1";
    private Vector2 scrollPosition;
    private int selectedTile = 0;
    private bool showHelp = true;

    private const int EMPTY = 0;
    private const int BRICK_WALL = 1;
    private const int METAL_WALL = 2;
    private const int PLAYER_SPAWN = 3;

    [MenuItem("Window/Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditor>("Редактор уровней");
    }

    private void OnEnable()
    {
        InitializeLevelData();
    }

    private void InitializeLevelData()
    {
        levelData = new int[levelWidth, levelHeight];
    }

    private void OnGUI()
    {
        GUILayout.Label("Редактор уровней", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        // Настройки уровня
        EditorGUILayout.BeginHorizontal();
        levelWidth = EditorGUILayout.IntField("Ширина", levelWidth);
        levelHeight = EditorGUILayout.IntField("Высота", levelHeight);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Создать новый уровень"))
        {
            InitializeLevelData();
        }

        EditorGUILayout.Space();

        // Выбор тайла
        EditorGUILayout.LabelField("Выберите тайл:", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        
        // Используем EditorStyles.toolbarButton для кнопок
        if (GUILayout.Button("Пусто", selectedTile == EMPTY ? EditorStyles.toolbarButton : EditorStyles.miniButton))
            selectedTile = EMPTY;
        if (GUILayout.Button("Кирпич", selectedTile == BRICK_WALL ? EditorStyles.toolbarButton : EditorStyles.miniButton))
            selectedTile = BRICK_WALL;
        if (GUILayout.Button("Металл", selectedTile == METAL_WALL ? EditorStyles.toolbarButton : EditorStyles.miniButton))
            selectedTile = METAL_WALL;
        if (GUILayout.Button("Спавн", selectedTile == PLAYER_SPAWN ? EditorStyles.toolbarButton : EditorStyles.miniButton))
            selectedTile = PLAYER_SPAWN;
            
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Сетка уровня
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        EditorGUILayout.BeginVertical();

        for (int y = levelHeight - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < levelWidth; x++)
            {
                string buttonText = GetTileSymbol(levelData[x, y]);
                if (GUILayout.Button(buttonText, GUILayout.Width(30), GUILayout.Height(30)))
                {
                    levelData[x, y] = selectedTile;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        // Сохранение уровня
        EditorGUILayout.BeginHorizontal();
        levelName = EditorGUILayout.TextField("Имя уровня", levelName);
        if (GUILayout.Button("Сохранить уровень", GUILayout.Width(120)))
        {
            SaveLevel();
        }
        EditorGUILayout.EndHorizontal();

        // Подсказка
        if (showHelp)
        {
            EditorGUILayout.HelpBox(
                "Как использовать редактор:\n" +
                "1. Выберите размеры уровня\n" +
                "2. Выберите тип тайла\n" +
                "3. Кликайте по сетке для размещения тайлов\n" +
                "4. Сохраните уровень",
                MessageType.Info);
        }
    }

    private string GetTileSymbol(int tileType)
    {
        switch (tileType)
        {
            case EMPTY: return "□";
            case BRICK_WALL: return "■";
            case METAL_WALL: return "▣";
            case PLAYER_SPAWN: return "P";
            default: return "?";
        }
    }

    private void SaveLevel()
    {
        string directory = "Assets/StreamingAssets";
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
        AssetDatabase.Refresh();
    }
}
#endif 